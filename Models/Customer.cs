using Microsoft.OData.ModelBuilder;
using System.ComponentModel.DataAnnotations;

namespace ODataIssue406.Models
{
  public class Customer
  {
    [Key]
    [Required]
    public string CustomerId { get; set; } // Some friendly ID like 'john@doe.com'
    public string Name { get; set; } = string.Empty;

    [Contained]
    public ICollection<Order> Orders { get; set; } = new List<Order>();
  }
}
