namespace UnifiedOnboarding.Auth.Features.Me.Me;

public sealed record GetCurrentUserResponse(
    string? Id,
    string? UserName,
    string? Email,
    IReadOnlyCollection<string> Roles
    );
