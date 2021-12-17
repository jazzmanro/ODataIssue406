using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Results;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.EntityFrameworkCore;

namespace ODataIssue406.Controllers
{
  [ApiController]
  [Route("v1/customers/{customerId}/orders")]
  public class CustomerOrdersController : ODataController
  {
    private readonly ILogger<CustomerOrdersController> _logger;
    private readonly MyAppContext _context;

    public CustomerOrdersController(ILogger<CustomerOrdersController> logger, MyAppContext context)
    {
      _logger = logger;
      _context = context;
    }

    [HttpGet]
    [EnableQuery]
    public async Task<IActionResult> GetOrdersForCustomer(string customerId)
    {
      var customer = await _context.Customers.FirstOrDefaultAsync(c => c.CustomerId == customerId);
      if (customer == null)
      {
        return NotFound();
      }

      return Ok(_context.Orders.Where(o => o.CustomerId == customerId));
    }

    [HttpGet("{dateId}")]
    [EnableQuery]
    public async Task<IActionResult> GetOrderForCustomer(string customerId, string dateId)
    {
      var customer = await _context.Customers.FirstOrDefaultAsync(c => c.CustomerId == customerId);
      if (customer == null)
      {
        return NotFound();
      }

      var order = _context.Orders.Where(o => o.CustomerId == customerId && o.DateId == dateId);
      if (order.Count() == 0)
      {
        return NotFound();
      }

      return Ok(SingleResult.Create(order));
    }
  }
}