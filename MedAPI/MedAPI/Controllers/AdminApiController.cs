using MedAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MedAPI.Controllers
{
    [Route("api/admin")]
    [ApiController]
    [Authorize(Roles = "Админ")]
    public class AdminApiController : ControllerBase
    {
        private readonly AppDbContext _appDbContext;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ILogger<AdminApiController> _logger;

        public AdminApiController(
            AppDbContext appDbContext,
            IWebHostEnvironment webHostEnvironment,
            ILogger<AdminApiController> logger)
        {
            _appDbContext = appDbContext;
            _webHostEnvironment = webHostEnvironment;
            _logger = logger;
        }

        [HttpGet("alldata")]
        public async Task<ActionResult<AdminTables>> GetAllData()
        {
            var data = new AdminTables
            {
                Users = await _appDbContext.Users.ToListAsync(),
                Products = await _appDbContext.Products.Include(p => p.Category).Include(p => p.Supplier).ToListAsync(),
                Orders = await _appDbContext.Orders.Include(o => o.User).Include(o => o.OrderDetails).ToListAsync(),
                Categories = await _appDbContext.Categories.Include(c => c.Products).ToListAsync(),
                Suppliers = await _appDbContext.Suppliers.Include(c => c.Products).ToListAsync(),
                Roles = await _appDbContext.Roles.Include(r => r.Users).ToListAsync(),
                OrderDetails = await _appDbContext.OrderDetails.Include(od => od.Product).Include(od => od.Order).ToListAsync()
            };
            return Ok(data);
        }
        
        

        [HttpGet("orders")]
        public async Task<ActionResult<IEnumerable<Order>>> GetOrders()
        {
            var orders = await _appDbContext.Orders
                .Include(o => o.User)
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .ToListAsync();

            return Ok(orders);
        }


        [HttpGet("orders/{id}")]
        public async Task<ActionResult<Order>> GetOrder(int id)
        {
            var order = await _appDbContext.Orders
                .Include(o => o.User)
                .Include(o => o.OrderDetails)
                .FirstOrDefaultAsync(o => o.OrderID == id);

            return order == null ? NotFound() : Ok(order);
        }

        [HttpPost("orders")]
        public async Task<ActionResult<Order>> CreateOrder([FromBody] Order order)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors);
                foreach (var error in errors) _logger.LogError(error.ErrorMessage);
                return BadRequest(ModelState);
            }

            _appDbContext.Orders.Add(order);
            await _appDbContext.SaveChangesAsync();
            return CreatedAtAction(nameof(GetOrder), new { id = order.OrderID }, order);
        }

        [HttpPut("orders/{id}")]
        public async Task<IActionResult> UpdateOrder(int id, [FromBody] Order order)
        {
            if (id != order.OrderID) return BadRequest("ID mismatch");
            if (!ModelState.IsValid) return BadRequest(ModelState);

            _appDbContext.Entry(order).State = EntityState.Modified;
            await _appDbContext.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("orders/{id}")]
        public async Task<IActionResult> DeleteOrder(int id)
        {
            var order = await _appDbContext.Orders
                .Include(o => o.OrderDetails)
                .FirstOrDefaultAsync(o => o.OrderID == id);

            if (order == null) return NotFound();

            _appDbContext.OrderDetails.RemoveRange(order.OrderDetails);
            _appDbContext.Orders.Remove(order);
            await _appDbContext.SaveChangesAsync();
            return NoContent();
        }


        [HttpGet("auditlogs")]
        public async Task<IActionResult> GetAuditLogs()
        {
            var logs = await _appDbContext.AuditLogs
                                     .OrderByDescending(a => a.ChangedAt)
                                     .ToListAsync();
            return Ok(logs);
        }






    }
}
