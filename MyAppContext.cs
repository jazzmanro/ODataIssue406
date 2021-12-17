using Microsoft.EntityFrameworkCore;
using ODataIssue406.Models;

namespace ODataIssue406
{
  public class MyAppContext : DbContext
  {
    public MyAppContext(DbContextOptions<MyAppContext> options) : base(options)
    {
    }

    public DbSet<Customer> Customers { get; set; }
    public DbSet<Order> Orders { get; set; }
  }
}
