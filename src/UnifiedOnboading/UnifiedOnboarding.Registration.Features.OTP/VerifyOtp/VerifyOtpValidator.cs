using FluentValidation;

namespace UnifiedOnboarding.Registration.Features.OTP.VerifyOtp;

public sealed class VerifyOtpValidator : AbstractValidator<VerifyOtpRequest>
{
    public VerifyOtpValidator()
    {
        RuleFor(x => x.MobileNumber)
            .NotEmpty()
            .WithMessage("Mobile Number is required");

        RuleFor(x => x.EnteredOtp)
            .NotEmpty()
            .WithMessage("OTP Is Required");
    }


}
