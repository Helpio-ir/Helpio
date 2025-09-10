using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Helpio.Ir.API.Services
{
    public interface IOrganizationContext
    {
        int? OrganizationId { get; }
        string? OrganizationName { get; }
        int? ApiKeyId { get; }
        bool IsAuthenticated { get; }
        bool IsApiKeyAuthentication { get; }
    }

    public class OrganizationContext : IOrganizationContext
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public OrganizationContext(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public int? OrganizationId
        {
            get
            {
                var orgIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst("OrganizationId")?.Value;
                
                // Log ???? ??? ??????
                if (_httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated == true)
                {
                    Console.WriteLine($"[OrganizationContext] Reading OrganizationId claim: {orgIdClaim}");
                }
                
                return int.TryParse(orgIdClaim, out var orgId) ? orgId : null;
            }
        }

        public string? OrganizationName =>
            _httpContextAccessor.HttpContext?.User?.FindFirst("OrganizationName")?.Value;

        public int? ApiKeyId
        {
            get
            {
                var apiKeyIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst("ApiKeyId")?.Value;
                return int.TryParse(apiKeyIdClaim, out var apiKeyId) ? apiKeyId : null;
            }
        }

        public bool IsAuthenticated =>
            _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;

        public bool IsApiKeyAuthentication =>
            _httpContextAccessor.HttpContext?.User?.FindFirst("AuthenticationType")?.Value == "ApiKey";
    }
}