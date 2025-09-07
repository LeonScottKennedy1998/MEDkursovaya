using Microsoft.AspNetCore.Mvc;

namespace MedAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MediaController : ControllerBase
    {
        [HttpPost("upload")]
        public async Task<IActionResult> UploadImage(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("Файл не выбран");

            var fileName = Path.GetFileName(file.FileName);
            var savePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/media/catalog", fileName);

            using (var stream = new FileStream(savePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var imageUrl = $"/media/catalog/{fileName}";
            return Ok(new { Url = imageUrl });
        }
    }
}
