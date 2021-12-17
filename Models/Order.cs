using System.ComponentModel.DataAnnotations;

namespace ODataIssue406.Models
{
  public class Order
  {
    [Key]
    public string CustomerId { get; set; }
    [Key]
    [StringLength(30)]
    [Required]
    public string DateId { get; set; } // Some friendly Date acting as key like '2021-12-17' or '2021-12-17-2' if there's a second, etc.
    public DateTimeOffset Time { get; set; }
    public double Value { get; set; }

    public virtual Customer Customer { get; set; }
  }
}
