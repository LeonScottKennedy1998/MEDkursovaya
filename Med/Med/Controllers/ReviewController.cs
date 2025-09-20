using Med.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;
using System.Net;
using System.Security.Claims;
using Med.Services;

public class ReviewController : Controller
{
    private readonly AppDbContext _context;
    private readonly ILogger<ReviewController> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IApiService _apiService;
    public ReviewController(AppDbContext context, ILogger<ReviewController> logger, IHttpClientFactory httpClientFactory, IApiService apiService)
    {
        _context = context;
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _apiService = apiService;
    }

    [HttpPost]
    public async Task<IActionResult> AddReview(Review review)
    {

            var client = await _apiService.CreateAuthenticatedClient(HttpContext);
            var response = await client.PostAsJsonAsync($"{_apiService}/api/Reviews", review);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("Catalog", "Home");
            }
            else if (response.StatusCode == HttpStatusCode.Unauthorized)
            {

                return RedirectToAction("SignIn", "AccountAdminManager");
            }
            else if (response.StatusCode == HttpStatusCode.BadRequest)
            {

                var validationErrors = await response.Content
                    .ReadFromJsonAsync<Dictionary<string, string[]>>();
                foreach (var field in validationErrors.Keys)
                {
                    foreach (var msg in validationErrors[field])
                    {
                        ModelState.AddModelError(field, msg);
                    }
                }
                return View(review);
            }


            return StatusCode((int)response.StatusCode);
        
    }

    [HttpPost]
    public async Task<IActionResult> EditReview(Review review)
    {
        var client = await _apiService.CreateAuthenticatedClient(HttpContext);

        var response = await client.PutAsJsonAsync($"{_apiService.BaseUrl}/api/Reviews/{review.ReviewID}", review);

        if (response.IsSuccessStatusCode)
        {
            return RedirectToAction("Reviews", "Home", new { productId = review.ProductID });
        }

        var validationErrors = await response.Content.ReadFromJsonAsync<Dictionary<string, string[]>>();
        foreach (var field in validationErrors.Keys)
        {
            foreach (var msg in validationErrors[field])
            {
                ModelState.AddModelError(field, msg);
            }
        }

        return View("EditReview", review);
    }




}
