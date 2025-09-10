using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Helpio.Ir.Application.Services.Core;
using System.Security.Claims;

namespace Helpio.Ir.API.Middleware
{
    public class ApiKeyAuthenticationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ApiKeyAuthenticationMiddleware> _logger;
        private const string ApiKeyHeaderName = "X-API-Key";

        public ApiKeyAuthenticationMiddleware(RequestDelegate next, ILogger<ApiKeyAuthenticationMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, IApiKeyService apiKeyService)
        {
            // Skip authentication for health checks and swagger
            if (ShouldSkipAuthentication(context.Request.Path))
            {
                await _next(context);
                return;
            }

            // Extract API key from header
            if (!context.Request.Headers.TryGetValue(ApiKeyHeaderName, out var extractedApiKey))
            {
                await HandleUnauthorizedAsync(context, "API Key missing");
                return;
            }

            var apiKey = extractedApiKey.ToString();
            if (string.IsNullOrEmpty(apiKey))
            {
                await HandleUnauthorizedAsync(context, "API Key is empty");
                return;
            }

            // Get client IP
            var clientIp = GetClientIP(context);

            // Validate API key
            var isValid = await apiKeyService.ValidateApiKeyAsync(apiKey, clientIp);
            if (!isValid)
            {
                await HandleUnauthorizedAsync(context, "Invalid API Key");
                return;
            }

            // Get API key details for context
            var apiKeyDetails = await apiKeyService.GetByKeyValueAsync(apiKey);
            if (apiKeyDetails?.Organization != null)
            {
                // Add organization context to user claims
                var claims = new List<Claim>
                {
                    new(ClaimTypes.NameIdentifier, "api-user"),
                    new("OrganizationId", apiKeyDetails.OrganizationId.ToString()),
                    new("OrganizationName", apiKeyDetails.Organization.Name ?? "Unknown"),
                    new("ApiKeyId", apiKeyDetails.Id.ToString()),
                    new("AuthenticationType", "ApiKey")
                };

                var identity = new ClaimsIdentity(claims, "ApiKey");
                context.User = new ClaimsPrincipal(identity);

                _logger.LogInformation("API Key authenticated for Organization: {OrganizationId}", 
                    apiKeyDetails.OrganizationId);
            }

            await _next(context);
        }

        private static bool ShouldSkipAuthentication(PathString path)
        {
            var pathValue = path.Value?.ToLower();
            
            return pathValue switch
            {
                _ when pathValue.StartsWith("/health") => true,
                _ when pathValue.StartsWith("/swagger") => true,
                _ when pathValue.StartsWith("/api/docs") => true,
                _ when pathValue == "/" => true,
                _ => false
            };
        }

        private static string? GetClientIP(HttpContext context)
        {
            // Try to get real IP from headers (in case of proxy/load balancer)
            var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrEmpty(forwardedFor))
            {
                return forwardedFor.Split(',')[0].Trim();
            }

            var realIp = context.Request.Headers["X-Real-IP"].FirstOrDefault();
            if (!string.IsNullOrEmpty(realIp))
            {
                return realIp;
            }

            // Fallback to connection remote IP
            return context.Connection.RemoteIpAddress?.ToString();
        }

        private async Task HandleUnauthorizedAsync(HttpContext context, string message)
        {
            _logger.LogWarning("API Key authentication failed: {Message} for {Path}", 
                message, context.Request.Path);

            context.Response.StatusCode = 401;
            context.Response.ContentType = "application/json";

            var response = new
            {
                error = "Unauthorized",
                message = message,
                timestamp = DateTime.UtcNow
            };

            await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(response));
        }
    }
}