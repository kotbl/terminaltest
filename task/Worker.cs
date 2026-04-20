using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using task.Data;
using task.Json;
using task.Models;

namespace task;

public class Worker(ILogger<Worker> logger, IServiceProvider services, IConfiguration config) : BackgroundService
{
    private static readonly TimeZoneInfo MskTz = TimeZoneInfo.FindSystemTimeZoneById("Russian Standard Time");

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Terminal import worker started");

        while (!stoppingToken.IsCancellationRequested)
        {
            var delay = GetDelayUntilNextRun();
            logger.LogInformation("Next import scheduled in {Delay:hh\\:mm\\:ss} at 02:00 MSK", delay);

            await Task.Delay(delay, stoppingToken);

            if (stoppingToken.IsCancellationRequested)
                break;

            await ImportAsync(stoppingToken);
        }

        logger.LogInformation("Terminal import worker stopped");
    }

    private async Task ImportAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Starting terminal import");
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        try
        {
            var jsonPath = config["TerminalsJsonPath"]
                ?? Path.Combine(AppContext.BaseDirectory, "files", "terminals.json");

            if (!File.Exists(jsonPath))
            {
                logger.LogError("terminals.json not found at {Path}", jsonPath);
                return;
            }

            await using var stream = File.OpenRead(jsonPath);
            var cities = await JsonSerializer.DeserializeAsync<List<CityJson>>(
                stream,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true },
                stoppingToken);

            if (cities is null || cities.Count == 0)
            {
                logger.LogWarning("Deserialized 0 cities from {Path}", jsonPath);
                return;
            }

            logger.LogInformation("Deserialized {CityCount} cities, {TerminalCount} terminals",
                cities.Count, cities.Sum(c => c.Terminals.Count));

            using var scope = services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            await using var transaction = await db.Database.BeginTransactionAsync(stoppingToken);

            var deletedPhones = await db.Phones.ExecuteDeleteAsync(stoppingToken);
            var deletedOffices = await db.Offices.ExecuteDeleteAsync(stoppingToken);
            var deletedCities = await db.Cities.ExecuteDeleteAsync(stoppingToken);

            logger.LogInformation(
                "Cleared DB: {Cities} cities, {Offices} offices, {Phones} phones",
                deletedCities, deletedOffices, deletedPhones);

            var cityEntities = new List<CityEntity>(cities.Count);
            var officeEntities = new List<OfficeEntity>();
            var phoneEntities = new List<PhoneEntity>();

            foreach (var city in cities)
            {
                var cityEntity = new CityEntity
                {
                    Id = city.Id,
                    Name = city.Name,
                    Code = city.Code,
                    CityId = city.CityId,
                    Latitude = ParseDecimal(city.Latitude),
                    Longitude = ParseDecimal(city.Longitude),
                    TimeShift = city.TimeShift,
                    RequestEndTime = city.RequestEndTime,
                    Day2DayRequest = city.Day2DayRequest == "1",
                    FreeStorageDays = int.TryParse(city.FreeStorageDays, out var fsd) ? fsd : 0
                };
                cityEntities.Add(cityEntity);

                foreach (var t in city.Terminals)
                {
                    var officeType = t.IsPVZ == "1" ? OfficeType.PVZ : OfficeType.WAREHOUSE;

                    var office = new OfficeEntity
                    {
                        Id = t.Id,
                        Name = t.Name,
                        CityId = city.Id,
                        CityCode = city.Code,
                        Address = t.Address,
                        FullAddress = t.FullAddress,
                        Latitude = ParseDecimal(t.Latitude),
                        Longitude = ParseDecimal(t.Longitude),
                        OfficeType = officeType,
                        CashOnDelivery = t.CashOnDelivery == "1",
                        Storage = t.Storage == "1",
                        ReceiveCargo = t.ReceiveCargo == "1",
                        GiveoutCargo = t.GiveoutCargo == "1",
                        MaxWeight = ParseJsonDecimal(t.MaxWeight),
                        MaxLength = ParseJsonDecimal(t.MaxLength),
                        MaxWidth = ParseJsonDecimal(t.MaxWidth),
                        MaxHeight = ParseJsonDecimal(t.MaxHeight),
                        WorkTime = t.Worktables.HasValue
                            ? t.Worktables.Value.GetRawText()
                            : null
                    };
                    officeEntities.Add(office);

                    foreach (var p in t.Phones)
                    {
                        phoneEntities.Add(new PhoneEntity
                        {
                            OfficeId = t.Id,
                            Number = p.Number,
                            Type = p.Type,
                            IsPrimary = p.Primary == "1"
                        });
                    }
                }
            }

            await db.Cities.AddRangeAsync(cityEntities, stoppingToken);
            await db.SaveChangesAsync(stoppingToken);

            await db.Offices.AddRangeAsync(officeEntities, stoppingToken);
            await db.SaveChangesAsync(stoppingToken);

            await db.Phones.AddRangeAsync(phoneEntities, stoppingToken);
            await db.SaveChangesAsync(stoppingToken);

            await transaction.CommitAsync(stoppingToken);

            stopwatch.Stop();
            logger.LogInformation(
                "Import completed in {Elapsed:g}: {Cities} cities, {Offices} offices, {Phones} phones saved",
                stopwatch.Elapsed, cityEntities.Count, officeEntities.Count, phoneEntities.Count);
        }
        catch (OperationCanceledException)
        {
            logger.LogWarning("Import cancelled by shutdown signal");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Import failed after {Elapsed:g}", stopwatch.Elapsed);
        }
    }

    private static TimeSpan GetDelayUntilNextRun()
    {
        var now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, MskTz);
        var next = now.Date.AddHours(2);
        if (next <= now)
            next = next.AddDays(1);
        return next - now;
    }

    private static decimal? ParseDecimal(string? value) =>
        decimal.TryParse(value, System.Globalization.NumberStyles.Any,
            System.Globalization.CultureInfo.InvariantCulture, out var result)
            ? result
            : null;

    private static decimal? ParseJsonDecimal(System.Text.Json.JsonElement? element)
    {
        if (element is null) return null;
        return element.Value.ValueKind switch
        {
            System.Text.Json.JsonValueKind.Number => element.Value.TryGetDecimal(out var d) ? d : null,
            System.Text.Json.JsonValueKind.String => ParseDecimal(element.Value.GetString()),
            _ => null
        };
    }
}
