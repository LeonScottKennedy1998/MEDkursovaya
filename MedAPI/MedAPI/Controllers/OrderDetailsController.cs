using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MedAPI.Models;
using Microsoft.AspNetCore.Authorization;

namespace MedAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Админ")]

    public class OrderDetailsController : Controller
    {
        private readonly AppDbContext _appDbContext;

        public OrderDetailsController(AppDbContext context)
        {
            _appDbContext = context;
        }

        [HttpPost("orderdetails")]
        public async Task<ActionResult<OrderDetail>> CreateOrderDetail([FromBody] OrderDetail orderDetail)
        {
            var product = await _appDbContext.Products.FindAsync(orderDetail.ProductID);
            if (product == null) return BadRequest("Продукт не найден");

            orderDetail.TotalPrice = product.Price * orderDetail.Quantity;
            _appDbContext.OrderDetails.Add(orderDetail);
            await _appDbContext.SaveChangesAsync();
            return CreatedAtAction(nameof(GetOrderDetail), new { id = orderDetail.OrderDetailID }, orderDetail);
        }

        [HttpGet("orderdetails/{id}")]
        public async Task<ActionResult<OrderDetail>> GetOrderDetail(int id)
        {
            var detail = await _appDbContext.OrderDetails
            .Include(od => od.Product)
            .Include(od => od.Order)
            .FirstOrDefaultAsync(od => od.OrderDetailID == id);
            return detail == null ? NotFound() : Ok(detail);
        }


        [HttpGet("orderdetails")]
        public async Task<ActionResult<IEnumerable<OrderDetail>>> GetOrderDetails() =>
            Ok(await _appDbContext.OrderDetails
                .Include(od => od.Product)
                .Include(od => od.Order)
                .ToListAsync());


        [HttpPut("orderdetails/{id}")]
        public async Task<IActionResult> UpdateOrderDetail(int id, [FromBody] OrderDetail orderDetail)
        {
            if (id != orderDetail.OrderDetailID) return BadRequest("ID mismatch");

            var product = await _appDbContext.Products.FindAsync(orderDetail.ProductID);
            if (product == null) return BadRequest("Продукт не найден");

            orderDetail.TotalPrice = product.Price * orderDetail.Quantity;

            _appDbContext.Entry(orderDetail).State = EntityState.Modified;
            await _appDbContext.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("orderdetails/{id}")]
        public async Task<IActionResult> DeleteOrderDetail(int id)
        {
            var detail = await _appDbContext.OrderDetails.FindAsync(id);
            if (detail == null) return NotFound();

            _appDbContext.OrderDetails.Remove(detail);
            await _appDbContext.SaveChangesAsync();
            return NoContent();
        }
    }
}
