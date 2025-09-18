using System.ComponentModel.DataAnnotations;

namespace Helpio.web.Models;

public class RegisterModel
{
    [Required(ErrorMessage = "نام کامل ضروری است")]
    [Display(Name = "نام کامل")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "نام شرکت ضروری است")]
    [Display(Name = "نام شرکت/استارتاپ")]
    public string Company { get; set; } = string.Empty;

    [Required(ErrorMessage = "ایمیل ضروری است")]
    [EmailAddress(ErrorMessage = "فرمت ایمیل نامعتبر است")]
    [Display(Name = "ایمیل")]
    public string Email { get; set; } = string.Empty;

    [Phone(ErrorMessage = "فرمت شماره تلفن نامعتبر است")]
    [Display(Name = "شماره تلفن")]
    public string? Phone { get; set; }

    [Required(ErrorMessage = "رمز عبور ضروری است")]
    [MinLength(6, ErrorMessage = "رمز عبور باید حداقل ۶ کاراکتر باشد")]
    [Display(Name = "رمز عبور")]
    public string Password { get; set; } = string.Empty;
}

public class LoginModel
{
    [Required(ErrorMessage = "ایمیل یا شماره تلفن ضروری است")]
    [Display(Name = "ایمیل یا شماره تلفن")]
    public string EmailOrPhone { get; set; } = string.Empty;

    [Required(ErrorMessage = "نوع ورود انتخاب کنید")]
    [Display(Name = "نوع ورود")]
    public string LoginType { get; set; } = "password"; // "password" or "otp"

    [Display(Name = "رمز عبور")]
    public string? Password { get; set; }

    [Display(Name = "مرا به خاطر بسپار")]
    public bool RememberMe { get; set; } = false;
}

public class OtpVerificationModel
{
    [Required(ErrorMessage = "ایمیل یا شماره تلفن ضروری است")]
    public string EmailOrPhone { get; set; } = string.Empty;

    [Required(ErrorMessage = "کد تأیید ضروری است")]
    [StringLength(6, MinimumLength = 4, ErrorMessage = "کد تأیید باید بین ۴ تا ۶ رقم باشد")]
    public string OtpCode { get; set; } = string.Empty;
}