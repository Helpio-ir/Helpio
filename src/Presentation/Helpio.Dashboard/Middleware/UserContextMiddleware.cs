using Helpio.Dashboard.Services;

namespace Helpio.Dashboard.Middleware
{
    public class UserContextMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<UserContextMiddleware> _logger;

        public UserContextMiddleware(RequestDelegate next, ILogger<UserContextMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, ICurrentUserContext userContext)
        {
            // فقط برای کاربران احراز هویت شده اطلاعات را بارگذاری می‌کنیم
            if (context.User.Identity?.IsAuthenticated == true)
            {
                try
                {
                    await userContext.LoadUserContextAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error loading user context");
                }
            }

            await _next(context);
        }
    }

    public static class UserContextMiddlewareExtensions
    {
        public static IApplicationBuilder UseUserContext(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<UserContextMiddleware>();
        }
    }
}