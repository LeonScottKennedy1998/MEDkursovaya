using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MedAPI.Models;

namespace MedAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Админ, Менеджер")]
    public class CategoriesController : Controller
    {
        private readonly AppDbContext _appDbContext;

        public CategoriesController(AppDbContext context)
        {
            _appDbContext = context;
        }

        [HttpGet("categories")]
        public async Task<ActionResult<IEnumerable<Category>>> GetCategories() =>
             Ok(await _appDbContext.Categories.Include(c => c.Products).ToListAsync());

        [HttpGet("categories/{id}")]
        public async Task<ActionResult<Category>> GetCategory(int id)
        {
            var category = await _appDbContext.Categories
                .Include(c => c.Products)
                .FirstOrDefaultAsync(c => c.CategoryID == id);

            return category == null ? NotFound() : Ok(category);
        }

        [HttpPost("categories")]
        public async Task<ActionResult<Category>> CreateCategory([FromBody] Category category)
        {
            if (await _appDbContext.Categories.AnyAsync(c => c.CategoryName == category.CategoryName))
                return Conflict("Категория с таким названием уже существует");

            _appDbContext.Categories.Add(category);
            await _appDbContext.SaveChangesAsync();
            return CreatedAtAction(nameof(GetCategory), new { id = category.CategoryID }, category);
        }

        [HttpPut("categories/{id}")]
        public async Task<IActionResult> UpdateCategory(int id, [FromBody] Category category)
        {
            if (id != category.CategoryID) return BadRequest("ID mismatch");

            var existing = await _appDbContext.Categories
                .AnyAsync(c => c.CategoryID != id && c.CategoryName == category.CategoryName);

            if (existing) return Conflict("Категория с таким названием уже существует");

            _appDbContext.Entry(category).State = EntityState.Modified;
            await _appDbContext.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("categories/{id}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var category = await _appDbContext.Categories
                .Include(c => c.Products)
                .FirstOrDefaultAsync(c => c.CategoryID == id);

            if (category == null) return NotFound();
            if (category.Products.Any()) return BadRequest("Невозможно удалить категорию с привязанными продуктами");

            _appDbContext.Categories.Remove(category);
            await _appDbContext.SaveChangesAsync();
            return NoContent();
        }

    }
}
