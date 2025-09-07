using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MedAPI.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace MedAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class OrdersController : Controller
    {
        private readonly AppDbContext _context;

        public OrdersController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetOrders()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var orders = await _context.Orders
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .Where(o => o.UserID == userId)
                .ToListAsync();

            var result = orders.Select(o => new OrderDto
            {
                OrderID = o.OrderID,
                OrderDate = o.OrderDate,
                OrderDetails = o.OrderDetails.Select(od => new OrderDetailDto
                {
                    ProductID = od.ProductID,
                    ProductName = od.Product?.NameProduct ?? "Unknown",
                    Quantity = od.Quantity,
                    TotalPrice = od.TotalPrice
                }).ToList()
            }).ToList();

            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] List<CartItemRequest> cartItems)
        {
            if (cartItems == null || !cartItems.Any())
                return BadRequest("Корзина пуста");

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return Unauthorized();

            var order = new Order
            {
                UserID = userId,
                OrderDate = DateTime.UtcNow,
                OrderDetails = new List<OrderDetail>()
            };

            var productIds = cartItems.Select(i => i.ProductID).ToList();
            var products = await _context.Products
                .Where(p => productIds.Contains(p.ProductID))
                .ToDictionaryAsync(p => p.ProductID);

            foreach (var item in cartItems)
            {
                if (!products.TryGetValue(item.ProductID, out var product))
                    return BadRequest($"Товар {item.ProductID} не найден");

                if (product.Stock < item.Quantity)
                    return BadRequest($"Недостаточно товара: {product.NameProduct}");

                order.OrderDetails.Add(new OrderDetail
                {
                    ProductID = item.ProductID,
                    Quantity = item.Quantity,
                    TotalPrice = product.Price * item.Quantity
                });

                product.Stock -= item.Quantity;
            }

            await _context.Orders.AddAsync(order);
            await _context.SaveChangesAsync();

            return Ok(new OrderDto
            {
                OrderID = order.OrderID,
                OrderDate = order.OrderDate,
                OrderDetails = order.OrderDetails.Select(od => new OrderDetailDto
                {
                    ProductID = od.ProductID,
                    ProductName = od.Product?.NameProduct ?? "Unknown",
                    Quantity = od.Quantity,
                    TotalPrice = od.TotalPrice
                }).ToList()
            });
        }


    }
}
