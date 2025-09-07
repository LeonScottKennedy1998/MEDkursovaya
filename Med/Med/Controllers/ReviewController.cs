using Med.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;
using System.Net;
using System.Security.Claims; 

public class ReviewController : Controller
{
    private readonly AppDbContext _context;
    private readonly ILogger<ReviewController> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly string _apiBaseUrl = "http://localhost:5072";
    public ReviewController(AppDbContext context, ILogger<ReviewController> logger, IHttpClientFactory httpClientFactory)
    {
        _context = context;
        _logger = logger;
        _httpClientFactory = httpClientFactory;
    }

    private async Task<HttpClient> GetAuthenticatedClient()
    {
        var token = Request.Cookies["jwt"];
        var client = _httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }
    [HttpPost]
    public async Task<IActionResult> AddReview(Review review)
    {

            var client = await GetAuthenticatedClient();
            var response = await client.PostAsJsonAsync($"{_apiBaseUrl}/api/Reviews", review);

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
        var client = await GetAuthenticatedClient();

        // PUT-запрос на API
        var response = await client.PutAsJsonAsync($"{_apiBaseUrl}/api/Reviews/{review.ReviewID}", review);

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

        return View("EditReview", review); // или вернём ту же страницу с формой
    }




}
