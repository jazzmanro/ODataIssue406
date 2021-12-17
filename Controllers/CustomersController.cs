using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Results;
using Microsoft.AspNetCore.OData.Routing.Controllers;

namespace ODataIssue406.Controllers
{
  [ApiController]
  [Route("v1/customers")]
  public class CustomersController : ODataController
  {
    private readonly ILogger<CustomersController> _logger;
    private readonly MyAppContext _context;

    public CustomersController(ILogger<CustomersController> logger, MyAppContext context)
    {
      _logger = logger;
      _context = context;
    }

    [HttpGet]
    [EnableQuery]
    public IActionResult GetChannels()
    {
      return Ok(_context.Customers);
    }

    [HttpGet("{customerId}")]
    [EnableQuery]
    public IActionResult GetCustomer(string customerId)
    {
      var customer = _context.Customers.Where(c => c.CustomerId == customerId);

      if (customer.Count() == 0)
      {
        return NotFound();
      }

      return Ok(SingleResult.Create(customer));
    }
  }
}