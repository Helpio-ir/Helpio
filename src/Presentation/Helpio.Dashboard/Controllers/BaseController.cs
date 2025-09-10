using Helpio.Dashboard.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Helpio.Dashboard.Controllers
{
    [Authorize]
    public abstract class BaseController : Controller
    {
        protected readonly ICurrentUserContext UserContext;

        protected BaseController(ICurrentUserContext userContext)
        {
            UserContext = userContext;
        }

        /// <summary>
        /// بررسی دسترسی کاربر به سازمان مشخص
        /// </summary>
        protected bool CanAccessOrganization(int organizationId)
        {
            return UserContext.CanAccessOrganization(organizationId);
        }

        /// <summary>
        /// بررسی دسترسی کاربر به تیم مشخص
        /// </summary>
        protected bool CanAccessTeam(int teamId)
        {
            return UserContext.CanAccessTeam(teamId);
        }

        /// <summary>
        /// بررسی اینکه آیا کاربر فعلی Admin است
        /// </summary>
        protected bool IsCurrentUserAdmin => UserContext.IsAdmin;

        /// <summary>
        /// بررسی اینکه آیا کاربر فعلی Manager است
        /// </summary>
        protected bool IsCurrentUserManager => UserContext.IsManager;

        /// <summary>
        /// بررسی اینکه آیا کاربر فعلی Agent است
        /// </summary>
        protected bool IsCurrentUserAgent => UserContext.IsAgent;

        /// <summary>
        /// دریافت ID سازمان کاربر فعلی
        /// </summary>
        protected int? CurrentOrganizationId => UserContext.CurrentOrganization?.Id;

        /// <summary>
        /// دریافت ID تیم کاربر فعلی
        /// </summary>
        protected int? CurrentTeamId => UserContext.CurrentTeam?.Id;

        /// <summary>
        /// بررسی دسترسی و برگشت Forbidden در صورت عدم دسترسی
        /// </summary>
        protected IActionResult CheckOrganizationAccess(int organizationId)
        {
            if (!CanAccessOrganization(organizationId))
            {
                return Forbid();
            }
            return new EmptyResult();
        }

        /// <summary>
        /// بررسی دسترسی به تیم و برگشت Forbidden در صورت عدم دسترسی
        /// </summary>
        protected IActionResult CheckTeamAccess(int teamId)
        {
            if (!CanAccessTeam(teamId))
            {
                return Forbid();
            }
            return new EmptyResult();
        }

        /// <summary>
        /// اضافه کردن اطلاعات کاربر و سازمان به ViewBag
        /// </summary>
        protected void SetUserContextToViewBag()
        {
            ViewBag.CurrentUser = UserContext;
            ViewBag.CurrentOrganization = UserContext.CurrentOrganization;
            ViewBag.CurrentTeam = UserContext.CurrentTeam;
            ViewBag.UserRoles = UserContext.UserRoles;
            ViewBag.IsAdmin = UserContext.IsAdmin;
            ViewBag.IsManager = UserContext.IsManager;
            ViewBag.IsAgent = UserContext.IsAgent;
        }

        public override void OnActionExecuting(Microsoft.AspNetCore.Mvc.Filters.ActionExecutingContext context)
        {
            // اضافه کردن اطلاعات کاربر به ViewBag در هر request
            SetUserContextToViewBag();
            base.OnActionExecuting(context);
        }
    }
}