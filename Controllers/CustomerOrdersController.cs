using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Deltas;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Results;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.EntityFrameworkCore;
using ODataIssue406.Models;

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
    // Console warning at strartup:
    // The path template 'v1/customers/{customerId}/orders/{dateId}' on the action 'GetOrderForCustomer' in controller 'CustomerOrders' is not a valid OData path template. The number of keys specified in the URI does not match number of key properties for the resource 'API.Order'.
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

    [HttpPatch("{dateId}")]
    // Console warning at strartup:
    // The path template 'v1/customers/{customerId}/orders/{dateId}' on the action 'PartiallyUpdateOrderForCustomer' in controller 'CustomerOrders' is not a valid OData path template. The number of keys specified in the URI does not match number of key properties for the resource 'API.Order'.
    public async Task<IActionResult> PartiallyUpdateOrderForCustomer(string customerId, string dateId, [FromBody] Delta<Order> orderDelta)
    {
      // Unfortunately orderDelta not populated properly here, orderDelta.GetInstance() returns a clean one, no properties set.
      // Investigation shows that in this situation we don't have any ODataInputFormatter registered to handle the Delta<T> population, thus, the default SystemTextJsonInputFormatter does that but Delta<T> needs specific methods to be called.
      // This behavior is not intuitive BTW, I would have expected that Delta<T> can actually be use as a nice tool to do light and smart JSON patches on objects without requiring a recognized OData route.

      if (!ModelState.IsValid)
      {
        return BadRequest(ModelState);
      }

      var customer = await _context.Customers.FirstOrDefaultAsync(c => c.CustomerId == customerId);
      if (customer == null)
      {
        return NotFound();
      }

      var order = await _context.Orders.FirstOrDefaultAsync(o => o.CustomerId == customerId && o.DateId == dateId);
      if (order == null)
      {
        return NotFound();
      }

      orderDelta.Patch(order);

      await _context.SaveChangesAsync();

      return NoContent();
    }
  }
}