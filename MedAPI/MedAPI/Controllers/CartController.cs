using MedAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace MedAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CartController : Controller
    {
        private readonly AppDbContext _context;

        public CartController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost("items")]
        public async Task<IActionResult> GetCartItems([FromBody] List<CartItemRequest> clientCart)
        {
            if (clientCart == null || !clientCart.Any())
                return Ok(new List<CartItem>());

            var ids = clientCart.Select(c => c.ProductID).ToList();
            var products = await _context.Products
                                      .Where(p => ids.Contains(p.ProductID))
                                      .ToListAsync();

            var result = clientCart.Select(c =>
            {
                var product = products.FirstOrDefault(p => p.ProductID == c.ProductID);
                return new CartItem
                {
                    ProductID = c.ProductID,
                    Quantity = c.Quantity,
                    NameProduct = product?.NameProduct ?? "Unknown",
                    Price = product?.Price ?? 0m,
                    ImageUrl = product?.ImageUrl ?? ""
                };
            }).ToList();

            return Ok(result);
        }


        [HttpGet("check-stock/{productId}/{quantity}")]
        public async Task<IActionResult> CheckStock(int productId, int quantity)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null)
                return NotFound("Товар не найден");

            if (product.Stock < quantity)
                return BadRequest("Недостаточно товара на складе");

            return Ok("Достаточно");
        }
    }
}
