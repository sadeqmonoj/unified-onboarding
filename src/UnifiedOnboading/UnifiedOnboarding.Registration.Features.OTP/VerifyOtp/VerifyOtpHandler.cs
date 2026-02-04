using Platform.BuildingBlocks.CustomMediator;
using Platform.BuildingBlocks.Results;
using UnifiedOnboarding.Registration.Features.OTP.Abstractions;

namespace UnifiedOnboarding.Registration.Features.OTP.VerifyOtp;

public sealed class VerifyOtpHandler : IRequestHandler<VerifyOtpRequest, Result<VerifyOtpResponse>>
{
    private readonly IOtpApi _otpApi;
    public VerifyOtpHandler(IOtpApi otpApi) => _otpApi = otpApi;

    public async Task<Result<VerifyOtpResponse>> Handle(VerifyOtpRequest request, CancellationToken cancellationToken)
    {
        var verifyOtpRequestDto = new VerifyOtpRequestDto(request.MobileNumber, request.EnteredOtp);
        VerifyOtpResultDto result = await _otpApi.VerifyOtpAsync(verifyOtpRequestDto, cancellationToken);

        if (!result.IsVerified)
        {
            return Result<VerifyOtpResponse>.Fail(
                Error.Server("Server", result.Message)
            );
        }
        return Result<VerifyOtpResponse>.Success(
            new VerifyOtpResponse(
                result.IsVerified,
                result.Message,
                result.AccessToken,
                result.GrantType
            )
        );
    }
}
