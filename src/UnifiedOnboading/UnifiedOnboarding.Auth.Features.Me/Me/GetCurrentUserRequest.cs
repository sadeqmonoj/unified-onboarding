using Platform.BuildingBlocks.CustomMediator;
using Platform.BuildingBlocks.Results;

namespace UnifiedOnboarding.Auth.Features.Me.Me;

public sealed record GetCurrentUserRequest : IRequest<Result<GetCurrentUserResponse>>;
