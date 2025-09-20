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
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ReviewsController : Controller
    {
        private readonly AppDbContext _context;
        private readonly ILogger<ReviewsController> _logger;


        public ReviewsController(AppDbContext context, ILogger<ReviewsController> logger)
        {
            _context = context;
            _logger = logger;
        }
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AddReview([FromBody] Review review)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userIdClaim == null) return Unauthorized();

            int userId = int.Parse(userIdClaim);

            _logger.LogInformation("User {UserId} пытается добавить отзыв на продукт {ProductId}", userId, review.ProductID);

            var userOrders = await _context.Orders
                .Where(o => o.UserID == userId)
                .Include(o => o.OrderDetails)
                .ToListAsync();

            _logger.LogInformation("Найдено {OrderCount} заказов для пользователя {UserId}", userOrders.Count, userId);

            foreach (var o in userOrders)
            {
                _logger.LogInformation("Заказ {OrderId} содержит продукты: {Products}", o.OrderID, string.Join(",", o.OrderDetails.Select(od => od.ProductID)));
            }

            bool hasPurchased = userOrders.Any(o => o.OrderDetails.Any(od => od.ProductID == review.ProductID));

            _logger.LogInformation("Проверка покупки для продукта {ProductId}: {HasPurchased}", review.ProductID, hasPurchased);

            if (!hasPurchased)
                return Forbid("Вы можете оставлять отзыв только на купленные товары.");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            review.UserID = userId;
            review.CreatedAt = DateTime.UtcNow;

            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetReviewById), new { id = review.ReviewID }, review);
        }

        [HttpPut("{id:int}")]
        [Authorize]
        public async Task<IActionResult> EditReview(int id, [FromBody] Review updatedReview)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null) return Unauthorized();
            int userId = int.Parse(userIdClaim);

            var review = await _context.Reviews.FirstOrDefaultAsync(r => r.ReviewID == id && r.UserID == userId);
            if (review == null) return NotFound();

            review.ReviewText = updatedReview.ReviewText;
            review.Rating = updatedReview.Rating;
            review.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return Ok(review);
        }



        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetReviewById(int id)
        {
            var review = await _context.Reviews
                .Include(r => r.User)
                .Include(r => r.Product)
                .FirstOrDefaultAsync(r => r.ReviewID == id);

            if (review == null)
                return NotFound();

            return Ok(review);
        }

        [HttpGet("product/{productId:int}")]
        public async Task<IActionResult> GetReviewsByProduct(int productId)
        {
            var reviews = await _context.Reviews
                .Where(r => r.ProductID == productId)
                .Include(r => r.User)
                .ToListAsync();

            return Ok(reviews);
        }

    }
}
