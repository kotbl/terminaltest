using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace task.Models;

[Table("phones")]
public class PhoneEntity
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("office_id")]
    public string OfficeId { get; set; } = null!;

    [Column("number")]
    public string Number { get; set; } = null!;

    [Column("type")]
    public string? Type { get; set; }

    [Column("is_primary")]
    public bool IsPrimary { get; set; }

    [ForeignKey(nameof(OfficeId))]
    public OfficeEntity Office { get; set; } = null!;
}
