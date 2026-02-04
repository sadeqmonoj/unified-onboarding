using FluentValidation;

namespace UnifiedOnboarding.Registration.Features.OTP.SendOtp;

public sealed class SendOtpValidator : AbstractValidator<SendOtpRequest>
{
    public SendOtpValidator() => RuleFor(x => x.MobileNumber)
        .NotEmpty()
        .WithMessage("Mobile Number is required")
            // Length validation
            .Length(10, 15)
            .WithMessage("Mobile number must be 10-15 digits")

            // Format validation - E.164 international format
            .Matches(@"^\+?[1-9]\d{1,14}$")
            .WithMessage("Invalid mobile number format. Use international format: +8801712345678")

            // Or for Bangladesh-specific:
            // .Matches(@"^(\+?88)?01[3-9]\d{8}$")
            // .WithMessage("Invalid Bangladesh mobile number")

            

            // Security validation
            .Must(NotContainDangerousCharacters)
            .WithMessage("Invalid characters in phone number");

    

    private bool NotContainDangerousCharacters(string phone)
    {
        // Prevent injection attempts
        string[] dangerous = new[] { "<", ">", "'", "\"", ";", "--", "/*", "*/" };
        return !dangerous.Any(phone.Contains);
    }
}
