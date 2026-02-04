using Platform.BuildingBlocks.CustomMediator;
using Platform.BuildingBlocks.Results;
using UnifiedOnboarding.Registration.Features.OTP.Abstractions;

namespace UnifiedOnboarding.Registration.Features.OTP.SendOtp;

public sealed class SendOtpHandler : IRequestHandler<SendOtpRequest, Result<SendOtpResponse>>
{
    private readonly IOtpApi _otpApi;

    public SendOtpHandler(IOtpApi otpApi) => _otpApi = otpApi;

    public async Task<Result<SendOtpResponse>> Handle(SendOtpRequest request, CancellationToken cancellationToken)
    {
        SendOtpResultDto result = await _otpApi.SendOtpAsync(request.MobileNumber, cancellationToken);

        if (!result.IsSent)
        {
            return Result<SendOtpResponse>.Fail(
                Error.Validation(result.Message)
            );
        }
        return Result<SendOtpResponse>.Success(
            new SendOtpResponse(result.IsSent, result.Message)
        );
    }
}
