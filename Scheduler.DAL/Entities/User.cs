using System.ComponentModel.DataAnnotations;

namespace Scheduler.DAL.Entities;

public class User
{
    public int Id { get; set; }
    [MaxLength(256)]
    public string Name { get; set; } = null!;
    [MaxLength(256)]
    public string NameNormalized { get; set; } = null!;
}
