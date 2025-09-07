using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MedAPI.Models;

namespace MedAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Админ")]
    public class RolesController : Controller
    {
        private readonly AppDbContext _appDbContext;

        public RolesController(AppDbContext context)
        {
            _appDbContext = context;
        }

        [HttpGet("roles")]
        public async Task<ActionResult<IEnumerable<Role>>> GetRoles() =>
            Ok(await _appDbContext.Roles.Include(r => r.Users).ToListAsync());

        [HttpGet("roles/{id}")]
        public async Task<ActionResult<Role>> GetRole(int id)
        {
            var role = await _appDbContext.Roles
                .Include(r => r.Users)
                .FirstOrDefaultAsync(r => r.RoleID == id);

            return role == null ? NotFound() : Ok(role);
        }

        [HttpPost("roles")]
        public async Task<ActionResult<Role>> CreateRole([FromBody] Role role)
        {
            if (await _appDbContext.Roles.AnyAsync(r => r.RoleName == role.RoleName))
                return Conflict("Роль с таким названием уже существует");

            _appDbContext.Roles.Add(role);
            await _appDbContext.SaveChangesAsync();
            return CreatedAtAction(nameof(GetRole), new { id = role.RoleID }, role);
        }

        [HttpPut("roles/{id}")]
        public async Task<IActionResult> UpdateRole(int id, [FromBody] Role role)
        {
            if (id != role.RoleID) return BadRequest("ID mismatch");

            var existing = await _appDbContext.Roles
                .AnyAsync(r => r.RoleID != id && r.RoleName == role.RoleName);

            if (existing) return Conflict("Роль с таким названием уже существует");

            _appDbContext.Entry(role).State = EntityState.Modified;
            await _appDbContext.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("roles/{id}")]
        public async Task<IActionResult> DeleteRole(int id)
        {
            var role = await _appDbContext.Roles
                .Include(r => r.Users)
                .FirstOrDefaultAsync(r => r.RoleID == id);

            if (role == null) return NotFound();
            if (role.Users.Any()) return BadRequest("Невозможно удалить роль с привязанными пользователями");

            _appDbContext.Roles.Remove(role);
            await _appDbContext.SaveChangesAsync();
            return NoContent();
        }
    }
}
