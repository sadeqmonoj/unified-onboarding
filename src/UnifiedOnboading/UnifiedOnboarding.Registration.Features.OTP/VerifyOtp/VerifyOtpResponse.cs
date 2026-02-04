namespace UnifiedOnboarding.Registration.Features.OTP.VerifyOtp;

public sealed record VerifyOtpResponse(bool IsVerified, string Message, string AccessToken, string GrantType);
