using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using task.Data;
using task.Models;

namespace task.Controllers;

/// <summary>Справочник терминалов</summary>
[ApiController]
[Route("api/[controller]")]
public class TerminalsController(AppDbContext db) : ControllerBase
{
    /// <summary>
    /// Найти терминалы по названию города и/или коду региона (первые 2 символа КЛАДР-кода).
    /// </summary>
    /// <param name="city">Название города (регистронезависимо)</param>
    /// <param name="region">Код региона — первые 2 цифры КЛАДР-кода города (например, "77" для Москвы)</param>
    /// <param name="ct">Токен отмены</param>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<OfficeResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetByCityAndRegion(
        [FromQuery] string? city,
        [FromQuery] string? region,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(city) && string.IsNullOrWhiteSpace(region))
            return BadRequest("Укажите хотя бы один параметр: city или region");

        var query = db.Offices
            .Include(o => o.City)
            .Include(o => o.Phones)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(city))
            query = query.Where(o => EF.Functions.ILike(o.City.Name, city.Trim()));

        if (!string.IsNullOrWhiteSpace(region))
            query = query.Where(o => o.CityCode.StartsWith(region.Trim()));

        var offices = await query
            .Select(o => new OfficeResponse(
                o.Id,
                o.Name,
                o.City.Name,
                o.City.CityId,
                o.CityCode,
                o.FullAddress ?? o.Address,
                o.OfficeType,
                o.Phones.Select(p => p.Number).ToList()))
            .ToListAsync(ct);

        return Ok(offices);
    }

    /// <summary>
    /// Получить числовой идентификатор города (cityID) по идентификатору офиса.
    /// </summary>
    /// <param name="id">Идентификатор офиса</param>
    /// <param name="ct">Токен отмены</param>
    [HttpGet("{id}/city")]
    [ProducesResponseType(typeof(CityIdResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCityId(string id, CancellationToken ct)
    {
        var result = await db.Offices
            .AsNoTracking()
            .Where(o => o.Id == id)
            .Select(o => new CityIdResponse(o.Id, o.City.Name, o.City.CityId, o.CityCode))
            .FirstOrDefaultAsync(ct);

        if (result is null)
            return NotFound($"Офис '{id}' не найден");

        return Ok(result);
    }
}

public record OfficeResponse(
    string Id,
    string Name,
    string CityName,
    long CityId,
    string CityCode,
    string? Address,
    OfficeType OfficeType,
    List<string> Phones);

public record CityIdResponse(
    string OfficeId,
    string CityName,
    long CityId,
    string CityCode);
