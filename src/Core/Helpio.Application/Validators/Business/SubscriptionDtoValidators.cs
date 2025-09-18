using FluentValidation;
using Helpio.Ir.Application.DTOs.Business;

namespace Helpio.Ir.Application.Validators.Business
{
    public class UpdateSubscriptionDtoValidator : AbstractValidator<UpdateSubscriptionDto>
    {
        public UpdateSubscriptionDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("نام اشتراک الزامی است")
                .Length(2, 100).WithMessage("نام اشتراک باید بین 2 تا 100 کاراکتر باشد");

            RuleFor(x => x.Description)
                .MaximumLength(1000).WithMessage("توضیحات حداکثر 1000 کاراکتر باشد")
                .When(x => !string.IsNullOrEmpty(x.Description));

            RuleFor(x => x.EndDate)
                .GreaterThan(DateTime.UtcNow).WithMessage("تاریخ انقضا باید از امروز بیشتر باشد")
                .When(x => x.EndDate.HasValue);

            RuleFor(x => x.CustomPrice)
                .GreaterThan(0).WithMessage("قیمت باید بیشتر از صفر باشد")
                .LessThan(100000000).WithMessage("قیمت حداکثر 100,000,000 تومان")
                .When(x => x.CustomPrice.HasValue);

            RuleFor(x => x.CustomMonthlyTicketLimit)
                .GreaterThan(0).WithMessage("حد تیکت ماهانه باید بیشتر از صفر باشد")
                .LessThanOrEqualTo(100000).WithMessage("حد تیکت ماهانه حداکثر 100,000 عدد")
                .When(x => x.CustomMonthlyTicketLimit.HasValue);

            RuleFor(x => x.Status)
                .IsInEnum().WithMessage("وضعیت اشتراک معتبر نیست");

            RuleFor(x => x.IsActive)
                .NotNull().WithMessage("وضعیت فعال بودن الزامی است");
        }
    }
}