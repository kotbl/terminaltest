using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace task.Models;

[Table("offices")]
public class OfficeEntity
{
    [Key]
    [Column("id")]
    public string Id { get; set; } = null!;

    [Column("name")]
    public string Name { get; set; } = null!;

    [Column("city_id")]
    public string CityId { get; set; } = null!;

    /// <summary>КЛАДР-код города (денормализовано для быстрого поиска)</summary>
    [Column("city_code")]
    public string CityCode { get; set; } = null!;

    [Column("address")]
    public string? Address { get; set; }

    [Column("full_address")]
    public string? FullAddress { get; set; }

    [Column("latitude")]
    public decimal? Latitude { get; set; }

    [Column("longitude")]
    public decimal? Longitude { get; set; }

    [Column("office_type")]
    public OfficeType OfficeType { get; set; }

    [Column("cash_on_delivery")]
    public bool CashOnDelivery { get; set; }

    [Column("storage")]
    public bool Storage { get; set; }

    [Column("receive_cargo")]
    public bool ReceiveCargo { get; set; }

    [Column("giveout_cargo")]
    public bool GiveoutCargo { get; set; }

    [Column("max_weight")]
    public decimal? MaxWeight { get; set; }

    [Column("max_length")]
    public decimal? MaxLength { get; set; }

    [Column("max_width")]
    public decimal? MaxWidth { get; set; }

    [Column("max_height")]
    public decimal? MaxHeight { get; set; }

    /// <summary>Расписание работы (JSON)</summary>
    [Column("work_time", TypeName = "jsonb")]
    public string? WorkTime { get; set; }

    [ForeignKey(nameof(CityId))]
    public CityEntity City { get; set; } = null!;

    public ICollection<PhoneEntity> Phones { get; set; } = [];
}
