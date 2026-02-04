using Platform.BuildingBlocks.Abstractions;
using Platform.BuildingBlocks.CustomMediator;
using Platform.BuildingBlocks.Results;

namespace UnifiedOnboarding.Auth.Features.Me.Me;

public sealed class GetCurrentUserHandler : IRequestHandler<GetCurrentUserRequest, Result<GetCurrentUserResponse>>
{
    private readonly ICurrentUser _currentUser;

    public GetCurrentUserHandler(ICurrentUser currentUser) => _currentUser = currentUser;

    public async Task<Result<GetCurrentUserResponse>> Handle(GetCurrentUserRequest request, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId is null)
        {
            return Result<GetCurrentUserResponse>.Fail(
               Error.Unauthorized("User is not Authenticated.")
           );
        }

        return Result<GetCurrentUserResponse>.Success(
            new GetCurrentUserResponse(
            _currentUser.UserId,
            _currentUser.UserName,
            _currentUser.Email,
            _currentUser.Roles.ToArray()
            )
        );
    }
}
