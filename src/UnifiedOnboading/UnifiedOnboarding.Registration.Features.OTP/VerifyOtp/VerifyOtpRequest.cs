using Platform.BuildingBlocks.CustomMediator;
using Platform.BuildingBlocks.Results;

namespace UnifiedOnboarding.Registration.Features.OTP.VerifyOtp;

public sealed record VerifyOtpRequest(string MobileNumber, string EnteredOtp) : IRequest<Result<VerifyOtpResponse>>;
