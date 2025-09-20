using MedAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MedAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CatalogController : Controller
    {
        private readonly AppDbContext _context;

        public CatalogController(AppDbContext context)
        {
            _context = context;
        }


        [HttpGet("catalog")]
        public async Task<IActionResult> GetCatalog([FromQuery] string? searchQuery, [FromQuery] int? categoryId)
        {
            var productsQuery = _context.Products
                .Include(p => p.Category)
                .Include(p => p.Reviews)
                    .ThenInclude(r => r.User)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchQuery))
                productsQuery = productsQuery.Where(p =>
                    p.NameProduct.Contains(searchQuery) || p.DescriptionProduct.Contains(searchQuery));

            if (categoryId.HasValue)
                productsQuery = productsQuery.Where(p => p.CategoryID == categoryId.Value);

            var dtoList = await productsQuery
        .Select(p => new Product
        {
            ProductID = p.ProductID,
            NameProduct = p.NameProduct,
            DescriptionProduct = p.DescriptionProduct,
            Price = p.Price,
            CategoryID = p.CategoryID,
            Stock = p.Stock,
            ImageUrl = p.ImageUrl,
            Category = new Category
            {
                CategoryID = p.Category.CategoryID,
                CategoryName = p.Category.CategoryName
            },
            Reviews = p.Reviews.Select(r => new Review
            {
                ReviewID = r.ReviewID,
                ReviewText = r.ReviewText,
                Rating = r.Rating,
                CreatedAt = r.CreatedAt,
                User = new User
                {
                    UserID = r.User.UserID,
                    NameUser = r.User.NameUser
                }
            }).ToList()
        })
        .ToListAsync();

            return Ok(dtoList);
        }



        [HttpGet("categories")]
        public async Task<IActionResult> GetCategories()
        {
            var categories = await _context.Categories.ToListAsync();
            return Ok(categories);
        }
    }
}
