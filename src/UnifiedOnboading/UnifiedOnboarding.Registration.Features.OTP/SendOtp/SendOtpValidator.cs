using FluentValidation;

namespace UnifiedOnboarding.Registration.Features.OTP.SendOtp;

public sealed class SendOtpValidator : AbstractValidator<SendOtpRequest>
{
    public SendOtpValidator() => RuleFor(x => x.MobileNumber)
        .NotEmpty()
        .WithMessage("Mobile Number is required");
}
