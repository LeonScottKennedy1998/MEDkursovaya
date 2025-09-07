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

    public class UsersController : Controller
    {
        private readonly AppDbContext _appDbContext;

        public UsersController(AppDbContext context)
        {
            _appDbContext = context;
        }

        [HttpGet("users")]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers() =>
            Ok(await _appDbContext.Users.ToListAsync());

        [HttpGet("users/{id}")]
        public async Task<ActionResult<User>> GetUser(int id)
        {
            var user = await _appDbContext.Users.FindAsync(id);
            return user == null ? NotFound() : Ok(user);
        }

        [HttpPost("users")]
        public async Task<ActionResult<User>> CreateUser([FromBody] User user)
        {
            if (await _appDbContext.Users.AnyAsync(u => u.Email == user.Email))
                return Conflict("Этот email уже используется");

            if (await _appDbContext.Users.AnyAsync(u => u.Username == user.Username))
                return Conflict("Этот логин уже используется");

            _appDbContext.Users.Add(user);
            await _appDbContext.SaveChangesAsync();
            return CreatedAtAction(nameof(GetUser), new { id = user.UserID }, user);
        }

        [HttpPut("users/{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] User user)
        {
            if (id != user.UserID) return BadRequest();

            _appDbContext.Entry(user).State = EntityState.Modified;
            await _appDbContext.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("users/{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _appDbContext.Users
                .Include(u => u.Orders)
                .ThenInclude(o => o.OrderDetails)
                .FirstOrDefaultAsync(u => u.UserID == id);

            if (user == null) return NotFound();

            foreach (var order in user.Orders)
                _appDbContext.OrderDetails.RemoveRange(order.OrderDetails);

            _appDbContext.Orders.RemoveRange(user.Orders);
            _appDbContext.Users.Remove(user);
            await _appDbContext.SaveChangesAsync();
            return NoContent();
        }

    }
}
