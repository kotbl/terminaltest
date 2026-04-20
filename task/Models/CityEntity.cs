using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace task.Models;

[Table("cities")]
public class CityEntity
{
    [Key]
    [Column("id")]
    public string Id { get; set; } = null!;

    [Column("name")]
    public string Name { get; set; } = null!;

    /// <summary>КЛАДР-код города</summary>
    [Column("code")]
    public string Code { get; set; } = null!;

    /// <summary>Числовой идентификатор города в системе перевозчика</summary>
    [Column("city_id")]
    public long CityId { get; set; }

    [Column("latitude")]
    public decimal? Latitude { get; set; }

    [Column("longitude")]
    public decimal? Longitude { get; set; }

    [Column("timeshift")]
    public string? TimeShift { get; set; }

    [Column("request_end_time")]
    public string? RequestEndTime { get; set; }

    [Column("day2day_request")]
    public bool Day2DayRequest { get; set; }

    [Column("free_storage_days")]
    public int FreeStorageDays { get; set; }

    public ICollection<OfficeEntity> Offices { get; set; } = [];
}
