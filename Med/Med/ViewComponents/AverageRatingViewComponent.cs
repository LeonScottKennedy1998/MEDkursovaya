using Med.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace Med.ViewComponents
{
    public class AverageRatingViewComponent : ViewComponent
    {
        private readonly AppDbContext _context;

        public AverageRatingViewComponent(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync(int productId)
        {
            var reviews = await _context.Reviews.Where(r => r.ProductID == productId).ToListAsync();
            double averageRating = reviews.Any() ? reviews.Average(r => r.Rating) : 0;

            return View("Default", averageRating);
        }
    }
}
