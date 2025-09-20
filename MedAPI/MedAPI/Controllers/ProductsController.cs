using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MedAPI.Models;

namespace MedAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Админ,Менеджер")]
    public class ProductsController : Controller
    {
        private readonly AppDbContext _appDbContext;
        private readonly ILogger<ProductsController> _logger;

        public ProductsController(AppDbContext context, ILogger<ProductsController> logger)
        {
            _appDbContext = context;
            _logger = logger;
        }

        [HttpGet("products")]
        public async Task<ActionResult<IEnumerable<Product>>> GetProducts() =>
            Ok(await _appDbContext.Products
                .Include(p => p.Category)
                .Include(p => p.Supplier)
                .ToListAsync());

        [HttpPost("products")]
        public async Task<ActionResult<Product>> CreateProduct([FromBody] Product product)
        {
            var supplierExists = await _appDbContext.Suppliers.AnyAsync(s => s.SupplierID == product.SupplierID);
            if (!supplierExists)
                return BadRequest("Указанный поставщик не найден");

            _appDbContext.Products.Add(product);
            await _appDbContext.SaveChangesAsync();
            return CreatedAtAction(nameof(GetProduct), new { id = product.ProductID }, product);
        }

        [HttpGet("products/{id}")]
        public async Task<ActionResult<Product>> GetProduct(int id)
        {
            var product = await _appDbContext.Products
                .Include(p => p.Category)
                .Include(p => p.Supplier)
                .FirstOrDefaultAsync(p => p.ProductID == id);

            return product == null ? NotFound() : Ok(product);
        }

        [HttpPut("products/{id}")]
        public async Task<IActionResult> UpdateProduct(int id, [FromBody] Product product)
        {
            if (id != product.ProductID) return BadRequest();

            var supplierExists = await _appDbContext.Suppliers.AnyAsync(s => s.SupplierID == product.SupplierID);
            if (!supplierExists)
                return BadRequest("Указанный поставщик не найден");


            _appDbContext.Entry(product).State = EntityState.Modified;
            await _appDbContext.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("products/{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _appDbContext.Products.FindAsync(id);
            if (product == null) return NotFound();

            _appDbContext.Products.Remove(product);
            await _appDbContext.SaveChangesAsync();
            return NoContent();
        }
    }
}
