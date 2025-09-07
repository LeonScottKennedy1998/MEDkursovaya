using MedAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MedAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Менеджер")]
    public class AnalyticsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AnalyticsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("sales")]
        public async Task<IActionResult> GetSalesAnalytics()
        {
            var orderDetails = await _context.OrderDetails
                .Include(od => od.Order)
                .Include(od => od.Product)
                .ToListAsync();

            var byProduct = orderDetails
                .GroupBy(od => od.Product.NameProduct)
                .Select(g => new
                {
                    ProductName = g.Key,
                    TotalSales = g.Sum(od => od.TotalPrice)
                })
                .ToList();

            var byDate = orderDetails
                .GroupBy(od => od.Order.OrderDate.Date)
                .Select(g => new
                {
                    Date = g.Key.ToString("yyyy-MM-dd"),
                    TotalSales = g.Sum(od => od.TotalPrice)
                })
                .ToList();

            return Ok(new
            {
                ByProduct = byProduct,
                ByDate = byDate
            });
        }
    }

}
