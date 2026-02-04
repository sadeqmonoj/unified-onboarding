namespace UnifiedOnboarding.Registration.Features.OTP.Abstractions;

public interface IOtpApi
{
    Task<SendOtpResultDto> SendOtpAsync(string MobileNumber, CancellationToken cancellationToken);

    Task<VerifyOtpResultDto> VerifyOtpAsync(VerifyOtpRequestDto requestDto, CancellationToken cancellationToken);

}

public sealed record SendOtpResultDto(bool IsSent, string Message);

public sealed record VerifyOtpRequestDto(string MobileNumber, string Otp);

public sealed record VerifyOtpResultDto(bool IsVerified, string Message, string AccessToken, string GrantType);


