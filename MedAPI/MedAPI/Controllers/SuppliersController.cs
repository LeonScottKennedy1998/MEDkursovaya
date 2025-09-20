using MedAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MedAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Админ, Менеджер")]
    public class SuppliersController : Controller
    {
        private readonly AppDbContext _appDbContext;

        public SuppliersController(AppDbContext context)
        {
            _appDbContext = context;
        }

        [HttpGet("suppliers")]
        public async Task<ActionResult<IEnumerable<Supplier>>> GetSuppliers() =>
             Ok(await _appDbContext.Suppliers
                 .Include(s => s.Products) // если есть связь
                 .ToListAsync());

        [HttpGet("suppliers/{id}")]
        public async Task<ActionResult<Supplier>> GetSupplier(int id)
        {
            var supplier = await _appDbContext.Suppliers
                .Include(s => s.Products)
                .FirstOrDefaultAsync(s => s.SupplierID == id);

            return supplier == null ? NotFound() : Ok(supplier);
        }

        [HttpPost("suppliers")]
        public async Task<ActionResult<Supplier>> CreateSupplier([FromBody] Supplier supplier)
        {
            if (await _appDbContext.Suppliers.AnyAsync(s => s.SupplierName == supplier.SupplierName))
                return Conflict("Поставщик с таким названием уже существует");

            _appDbContext.Suppliers.Add(supplier);
            await _appDbContext.SaveChangesAsync();
            return CreatedAtAction(nameof(GetSupplier), new { id = supplier.SupplierID }, supplier);
        }

        [HttpPut("suppliers/{id}")]
        public async Task<IActionResult> UpdateSupplier(int id, [FromBody] Supplier supplier)
        {
            if (id != supplier.SupplierID) return BadRequest("ID mismatch");

            var existing = await _appDbContext.Suppliers
                .AnyAsync(s => s.SupplierID != id && s.SupplierName == supplier.SupplierName);

            if (existing) return Conflict("Поставщик с таким названием уже существует");

            _appDbContext.Entry(supplier).State = EntityState.Modified;
            await _appDbContext.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("suppliers/{id}")]
        public async Task<IActionResult> DeleteSupplier(int id)
        {
            var supplier = await _appDbContext.Suppliers
                .Include(s => s.Products)
                .FirstOrDefaultAsync(s => s.SupplierID == id);

            if (supplier == null) return NotFound();
            if (supplier.Products.Any()) return BadRequest("Невозможно удалить поставщика с привязанными продуктами");

            _appDbContext.Suppliers.Remove(supplier);
            await _appDbContext.SaveChangesAsync();
            return NoContent();
        }
    }
}
