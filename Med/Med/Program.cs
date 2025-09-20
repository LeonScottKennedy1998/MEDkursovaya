using Microsoft.EntityFrameworkCore;
using Med.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Localization;
using System.Globalization;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Localization;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Med.Services;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
string connection = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connection));
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
.AddCookie(options =>
{
    options.LoginPath = "/AccountAdminManager/SignIn";
})
.AddJwtBearer("Bearer", options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = "MedAPI",
        ValidAudience = "MedClient",
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes("super-mega-duper-puper-secretkey12345ya-idu-iskat"))
    };
});
builder.Services.AddDistributedMemoryCache();
builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient();
builder.Services.AddScoped<IApiService, ApiService>();
builder.Services.AddSession();
builder.Services.Configure<ApiSettings>(builder.Configuration.GetSection("ApiSettings"));
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var supportedCultures = new List<CultureInfo>
    {
        new CultureInfo("ru-RU")
    };

    options.DefaultRequestCulture = new RequestCulture("ru-RU");
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.Use(async (context, next) =>
{
    if (context.Request.Path.StartsWithSegments("/media/catalog"))
    {
        var fileName = context.Request.Path.Value!.Replace("/media/catalog/", "");
        var apiBaseUrl = "http://localhost:5072";
        var token = context.Request.Cookies["jwt"];

        using var client = new HttpClient();
        if (!string.IsNullOrEmpty(token))
        {
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

        var apiUrl = $"{apiBaseUrl}/media/catalog/{fileName}";
        try
        {
            var response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var contentType = response.Content.Headers.ContentType?.ToString() ?? "application/octet-stream";
                context.Response.ContentType = contentType;
                await response.Content.CopyToAsync(context.Response.Body);
                return;
            }
            else
            {
                context.Response.StatusCode = (int)response.StatusCode;
                return;
            }
        }
        catch
        {
            context.Response.StatusCode = 500;
            return;
        }
    }

    await next();
});
app.UseRouting();



app.UseAuthentication();
app.UseAuthorization();

app.UseSession();

app.UseRequestLocalization(app.Services.GetRequiredService<IOptions<RequestLocalizationOptions>>().Value);

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
