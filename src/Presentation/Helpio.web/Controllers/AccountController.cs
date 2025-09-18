using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Helpio.web.Models;
using Helpio.Ir.Domain.Entities.Core;
using Helpio.Ir.Domain.Entities.Business;
using Helpio.Ir.Application.Services.Core;
using Helpio.Ir.Application.Services.Business;
using Helpio.Ir.Application.DTOs.Core;
using Helpio.Ir.Application.DTOs.Business;

namespace Helpio.web.Controllers;

public class AccountController : Controller
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly ILogger<AccountController> _logger;
    private readonly IOrganizationService _organizationService;
    private readonly ISubscriptionService _subscriptionService;
    private readonly IPlanService _planService;
    private readonly RoleManager<IdentityRole<int>> _roleManager;

    public AccountController(
        UserManager<User> userManager,
        SignInManager<User> signInManager,
        ILogger<AccountController> logger,
        IOrganizationService organizationService,
        ISubscriptionService subscriptionService,
        IPlanService planService,
        RoleManager<IdentityRole<int>> roleManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _logger = logger;
        _organizationService = organizationService;
        _subscriptionService = subscriptionService;
        _planService = planService;
        _roleManager = roleManager;
    }

    [HttpPost]
    public async Task<IActionResult> Register([FromBody] RegisterModel model)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToArray();
                
                return Json(new { 
                    success = false, 
                    message = "لطفاً تمام فیلدهای ضروری را پر کنید",
                    errors = errors
                });
            }

            // بررسی وجود کاربر با این ایمیل
            var existingUser = await _userManager.FindByEmailAsync(model.Email);
            if (existingUser != null)
            {
                return Json(new { 
                    success = false, 
                    message = "کاربری با این ایمیل قبلاً ثبت‌نام کرده است" 
                });
            }

            // جدا کردن نام از نام کامل
            var nameParts = model.Name.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var firstName = nameParts.FirstOrDefault() ?? "";
            var lastName = nameParts.Length > 1 ? string.Join(" ", nameParts.Skip(1)) : "";

            // ایجاد کاربر جدید
            var user = new User
            {
                UserName = model.Email,
                Email = model.Email,
                FirstName = firstName,
                LastName = lastName,
                PhoneNumber = model.Phone,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                EmailConfirmed = true // فعلاً true می‌گذاریم، بعداً confirmation اضافه می‌کنیم
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                // 1. ایجاد workspace/organization برای شرکت
                var organization = await CreateOrganizationForUserAsync(model.Company, user.Id);
                
                // 2. اعمال محدودیت‌های پلن فریمیوم (۵۰ تیکت در ماه)
                await CreateFreemiumSubscriptionAsync(organization.Id);

                // تخصیص نقش Agent به کاربر (نقش پیش‌فرض)
                await EnsureRoleExistsAsync("Agent");
                await _userManager.AddToRoleAsync(user, "Agent");

                // ورود خودکار کاربر
                await _signInManager.SignInAsync(user, isPersistent: false);

                _logger.LogInformation("Freemium signup successful: {Email}, Company: {Company}, UserId: {UserId}, OrganizationId: {OrganizationId}", 
                    model.Email, model.Company, user.Id, organization.Id);

                // 3. ارسال ایمیل خوش‌آمدگویی
                await SendWelcomeEmailAsync(user, model.Company);

                return Json(new { 
                    success = true, 
                    message = "ثبت‌نام موفقیت‌آمیز! حساب فریمیوم شما فعال شد. ۵۰ تیکت رایگان در اختیار دارید.",
                    redirectUrl = "https://dashboard.helpio.ir"
                });
            }
            else
            {
                var errors = string.Join(", ", result.Errors.Select(e => TranslateIdentityError(e.Code)));
                _logger.LogWarning("Registration failed for {Email}: {Errors}", model.Email, errors);
                
                return Json(new { 
                    success = false, 
                    message = $"خطا در ثبت‌نام: {errors}"
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "خطا در ثبت‌نام کاربر {Email}", model?.Email);
            return Json(new { 
                success = false, 
                message = "خطایی در ثبت‌نام رخ داده است. لطفاً دوباره تلاش کنید." 
            });
        }
    }

    private async Task<OrganizationDto> CreateOrganizationForUserAsync(string companyName, int userId)
    {
        var createOrgDto = new CreateOrganizationDto
        {
            Name = companyName,
            Description = $"سازمان ایجاد شده برای {companyName}",
            Email = null, // این بعداً توسط کاربر تنظیم می‌شود
            PhoneNumber = null,
            Address = null
        };

        var organization = await _organizationService.CreateAsync(createOrgDto);
        
        _logger.LogInformation("Organization created for user {UserId}: {OrganizationName} (ID: {OrganizationId})", 
            userId, organization.Name, organization.Id);

        return organization;
    }

    private async Task CreateFreemiumSubscriptionAsync(int organizationId)
    {
        // Get the default freemium plan
        var freemiumPlan = await _planService.GetDefaultFreemiumPlanAsync();
        if (freemiumPlan == null)
        {
            _logger.LogError("Default freemium plan not found! Creating subscription with hardcoded values.");
            // Fallback - this shouldn't happen in production
            throw new InvalidOperationException("Default freemium plan not found in database");
        }

        var createSubscriptionDto = new CreateSubscriptionDto
        {
            Name = "اشتراک فریمیوم",
            Description = "اشتراک رایگان با محدودیت ۵۰ تیکت در ماه",
            OrganizationId = organizationId,
            PlanId = freemiumPlan.Id,
            StartDate = DateTime.UtcNow,
            EndDate = null // فریمیوم بدون تاریخ انقضا
        };

        var subscription = await _subscriptionService.CreateAsync(createSubscriptionDto);
        
        _logger.LogInformation("Freemium subscription created for organization {OrganizationId}: {SubscriptionId} using plan {PlanId}", 
            organizationId, subscription.Id, freemiumPlan.Id);
    }

    private async Task SendWelcomeEmailAsync(User user, string companyName)
    {
        try
        {
            // TODO: پیاده‌سازی ارسال ایمیل خوش‌آمدگویی
            // فعلاً فقط لاگ می‌کنیم
            _logger.LogInformation("Welcome email should be sent to {Email} for company {Company}", 
                user.Email, companyName);

            // در آینده اینجا کد ارسال ایمیل اضافه می‌شود:
            // var emailService = ... // Inject IEmailService
            // await emailService.SendWelcomeEmailAsync(user.Email, user.FirstName, companyName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "خطا در ارسال ایمیل خوش‌آمدگویی به {Email}", user.Email);
            // عدم ارسال ایمیل نباید مانع ثبت‌نام شود
        }
    }

    private async Task EnsureRoleExistsAsync(string roleName)
    {
        if (!await _roleManager.RoleExistsAsync(roleName))
        {
            await _roleManager.CreateAsync(new IdentityRole<int>(roleName));
            _logger.LogInformation("Role {RoleName} created", roleName);
        }
    }

    [HttpPost]
    public async Task<IActionResult> Login([FromBody] LoginModel model)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "لطفاً تمام فیلدهای ضروری را پر کنید" });
            }

            if (model.LoginType == "otp")
            {
                // شبیه‌سازی ارسال OTP
                _logger.LogInformation("OTP request for: {EmailOrPhone}", model.EmailOrPhone);
                return Json(new { 
                    success = true, 
                    message = "کد تأیید به شماره/ایمیل شما ارسال شد",
                    requiresOtp = true
                });
            }
            else
            {
                // ورود با رمز عبور
                var user = await _userManager.FindByEmailAsync(model.EmailOrPhone);
                if (user == null)
                {
                    // بررسی با شماره تلفن
                    user = _userManager.Users.FirstOrDefault(u => u.PhoneNumber == model.EmailOrPhone);
                }

                if (user == null || !user.IsActive)
                {
                    return Json(new { 
                        success = false, 
                        message = "ایمیل/شماره تلفن یا رمز عبور نادرست است" 
                    });
                }

                var result = await _signInManager.PasswordSignInAsync(
                    user, 
                    model.Password!, 
                    model.RememberMe, 
                    lockoutOnFailure: false);

                if (result.Succeeded)
                {
                    _logger.LogInformation("User {Email} logged in successfully", user.Email);
                    
                    return Json(new { 
                        success = true, 
                        message = "ورود موفقیت‌آمیز!",
                        redirectUrl = "https://dashboard.helpio.ir"
                    });
                }
                else
                {
                    _logger.LogWarning("Failed login attempt for {EmailOrPhone}", model.EmailOrPhone);
                    return Json(new { 
                        success = false, 
                        message = "ایمیل/شماره تلفن یا رمز عبور نادرست است" 
                    });
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "خطا در ورود کاربر");
            return Json(new { success = false, message = "خطایی در ورود رخ داده است. لطفاً دوباره تلاش کنید." });
        }
    }

    [HttpPost]
    public async Task<IActionResult> VerifyOtp([FromBody] OtpVerificationModel model)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "کد تأیید نامعتبر است" });
            }

            // شبیه‌سازی تأیید OTP
            // TODO: Verify OTP against stored value
            
            _logger.LogInformation("OTP verification for: {EmailOrPhone}, Code: {OtpCode}", model.EmailOrPhone, model.OtpCode);

            // فرض می‌کنیم OTP درست است
            var user = await _userManager.FindByEmailAsync(model.EmailOrPhone);
            if (user == null)
            {
                user = _userManager.Users.FirstOrDefault(u => u.PhoneNumber == model.EmailOrPhone);
            }

            if (user != null && user.IsActive)
            {
                await _signInManager.SignInAsync(user, isPersistent: false);
                
                return Json(new { 
                    success = true, 
                    message = "ورود موفقیت‌آمیز!",
                    redirectUrl = "https://dashboard.helpio.ir"
                });
            }

            return Json(new { 
                success = false, 
                message = "کد تأیید نامعتبر است" 
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "خطا در تأیید کد OTP");
            return Json(new { success = false, message = "خطایی در تأیید کد رخ داده است. لطفاً دوباره تلاش کنید." });
        }
    }

    private static string TranslateIdentityError(string errorCode)
    {
        return errorCode switch
        {
            "DuplicateUserName" => "این نام کاربری قبلاً استفاده شده است",
            "DuplicateEmail" => "این ایمیل قبلاً ثبت شده است",
            "InvalidEmail" => "فرمت ایمیل نادرست است",
            "PasswordTooShort" => "رمز عبور باید حداقل ۶ کاراکتر باشد",
            "PasswordRequiresNonAlphanumeric" => "رمز عبور باید حداقل یک کاراکتر خاص داشته باشد",
            "PasswordRequiresDigit" => "رمز عبور باید حداقل یک رقم داشته باشد",
            "PasswordRequiresLower" => "رمز عبور باید حداقل یک حرف کوچک داشته باشد",
            "PasswordRequiresUpper" => "رمز عبور باید حداقل یک حرف بزرگ داشته باشد",
            _ => "خطای نامشخص"
        };
    }
}