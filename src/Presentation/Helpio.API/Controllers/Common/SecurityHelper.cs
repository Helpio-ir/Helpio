using Microsoft.AspNetCore.Mvc;
using Helpio.Ir.API.Services;

namespace Helpio.Ir.API.Controllers.Common
{
    /// <summary>
    /// Helper class for common security checks in controllers
    /// </summary>
    public static class SecurityHelper
    {
        /// <summary>
        /// Check if user is authenticated and has valid organization context
        /// </summary>
        public static ActionResult? CheckAuthentication(IOrganizationContext organizationContext)
        {
            if (!organizationContext.IsAuthenticated)
            {
                return new UnauthorizedObjectResult("User must be authenticated");
            }

            if (!organizationContext.OrganizationId.HasValue)
            {
                return new BadRequestObjectResult("Organization context not found");
            }

            return null; // No error
        }

        /// <summary>
        /// Check if user has access to organization-specific resource
        /// </summary>
        public static ActionResult? CheckOrganizationAccess(IOrganizationContext organizationContext, int resourceOrganizationId, string resourceType = "resource")
        {
            var authCheck = CheckAuthentication(organizationContext);
            if (authCheck != null)
                return authCheck;

            if (organizationContext.OrganizationId.Value != resourceOrganizationId)
            {
                return new ObjectResult($"Access denied to other organization's {resourceType}")
                {
                    StatusCode = 403
                };
            }

            return null; // No error
        }

        /// <summary>
        /// Check if unauthenticated user can access public resource
        /// </summary>
        public static ActionResult? CheckPublicAccess(IOrganizationContext organizationContext, bool isPublic, string resourceType = "resource")
        {
            // If user is authenticated, they passed organization checks already
            if (organizationContext.IsAuthenticated)
                return null;

            // If user is not authenticated and resource is not public, deny access
            if (!isPublic)
            {
                return new NotFoundResult(); // Don't reveal that resource exists
            }

            return null; // No error
        }
    }
}