namespace MedAPI.Middleware
{
    public class JwtValidationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<JwtValidationMiddleware> _logger;

        public JwtValidationMiddleware(RequestDelegate next, ILogger<JwtValidationMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var path = context.Request.Path.Value?.ToLower();

            // Пропускаем login и register
            if (path!.Contains("/api/auth/login") || path.Contains("/api/auth/register") || path.Contains("/api/auth/forgot-password") || path.Contains("/api/auth/reset-password"))
            {
                await _next(context);
                return;
            }

            var jwt = context.Request.Cookies["jwt"] ??
          context.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

            if (string.IsNullOrEmpty(jwt))
            {
                _logger.LogInformation("JWT missing");
                if (!context.Request.Path.StartsWithSegments("/api"))
                {
                    context.Response.Redirect("/AccountAdminManager/SignIn");
                    return;
                }
                else
                {
                    context.Response.StatusCode = 401;
                    await context.Response.WriteAsync("Unauthorized");
                    return;
                }
            }

            await _next(context);
        }
    }
}

