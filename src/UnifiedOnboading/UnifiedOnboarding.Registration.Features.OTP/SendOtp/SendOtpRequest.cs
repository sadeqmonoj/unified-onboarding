using Platform.BuildingBlocks.CustomMediator;
using Platform.BuildingBlocks.Results;

namespace UnifiedOnboarding.Registration.Features.OTP.SendOtp;

public sealed record SendOtpRequest(string MobileNumber) : IRequest<Result<SendOtpResponse>>;
