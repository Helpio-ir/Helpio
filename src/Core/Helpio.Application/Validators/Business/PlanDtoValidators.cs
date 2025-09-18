using FluentValidation;
using Helpio.Ir.Application.DTOs.Business;
using Helpio.Ir.Domain.Entities.Business;

namespace Helpio.Ir.Application.Validators.Business
{
    public class CreatePlanDtoValidator : AbstractValidator<CreatePlanDto>
    {
        public CreatePlanDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("نام پلن ضروری است")
                .MaximumLength(100).WithMessage("نام پلن نباید بیش از ۱۰۰ کاراکتر باشد");

            RuleFor(x => x.Type)
                .IsInEnum().WithMessage("نوع پلن نامعتبر است");

            RuleFor(x => x.Price)
                .GreaterThanOrEqualTo(0).WithMessage("قیمت نمی‌تواند منفی باشد");

            RuleFor(x => x.Currency)
                .NotEmpty().WithMessage("نوع ارز ضروری است")
                .MaximumLength(3).WithMessage("کد ارز نباید بیش از ۳ کاراکتر باشد");

            RuleFor(x => x.BillingCycleDays)
                .GreaterThan(0).WithMessage("دوره صورتحساب باید بیش از ۰ روز باشد")
                .LessThanOrEqualTo(365).WithMessage("دوره صورتحساب نباید بیش از ۳۶۵ روز باشد");

            RuleFor(x => x.MonthlyTicketLimit)
                .Must(x => x == -1 || x > 0).WithMessage("محدودیت تیکت باید -۱ (نامحدود) یا عددی مثبت باشد");

            RuleFor(x => x.DisplayOrder)
                .GreaterThanOrEqualTo(0).WithMessage("ترتیب نمایش نمی‌تواند منفی باشد");

            // Business rules
            RuleFor(x => x)
                .Must(HaveConsistentFeatures)
                .WithMessage("ویژگی‌های پلن با نوع آن سازگار نیست");
        }

        private bool HaveConsistentFeatures(CreatePlanDto dto)
        {
            // Freemium plans should have basic features only
            if (dto.Type == PlanType.Freemium)
            {
                return dto.Price == 0 &&
                       !dto.HasPrioritySupport &&
                       !dto.Has24x7Support &&
                       !dto.HasCustomBranding &&
                       !dto.HasAdvancedReporting &&
                       !dto.HasCustomIntegrations;
            }

            // Enterprise plans should have all features
            if (dto.Type == PlanType.Enterprise)
            {
                return dto.MonthlyTicketLimit == -1; // Unlimited
            }

            return true;
        }
    }

    public class UpdatePlanDtoValidator : AbstractValidator<UpdatePlanDto>
    {
        public UpdatePlanDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("نام پلن ضروری است")
                .MaximumLength(100).WithMessage("نام پلن نباید بیش از ۱۰۰ کاراکتر باشد");

            RuleFor(x => x.Price)
                .GreaterThanOrEqualTo(0).WithMessage("قیمت نمی‌تواند منفی باشد");

            RuleFor(x => x.Currency)
                .NotEmpty().WithMessage("نوع ارز ضروری است")
                .MaximumLength(3).WithMessage("کد ارز نباید بیش از ۳ کاراکتر باشد");

            RuleFor(x => x.BillingCycleDays)
                .GreaterThan(0).WithMessage("دوره صورتحساب باید بیش از ۰ روز باشد")
                .LessThanOrEqualTo(365).WithMessage("دوره صورتحساب نباید بیش از ۳۶۵ روز باشد");

            RuleFor(x => x.MonthlyTicketLimit)
                .Must(x => x == -1 || x > 0).WithMessage("محدودیت تیکت باید -۱ (نامحدود) یا عددی مثبت باشد");

            RuleFor(x => x.DisplayOrder)
                .GreaterThanOrEqualTo(0).WithMessage("ترتیب نمایش نمی‌تواند منفی باشد");
        }
    }
}