using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Platform.BuildingBlocks.Abstractions;

namespace Platform.Infrastructure;

public sealed class CurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUser(IHttpContextAccessor httpContextAccessor) => _httpContextAccessor = httpContextAccessor;

    private ClaimsPrincipal? Principal => _httpContextAccessor.HttpContext?.User;
    public string? UserId =>
        Principal?.FindFirstValue("sub") ??
        Principal?.FindFirstValue(ClaimTypes.NameIdentifier);

    public string? UserName =>
        Principal?.Identity?.Name ??
        Principal?.FindFirstValue("name") ??
        Principal?.FindFirstValue(ClaimTypes.Name);

    public string? Email =>
        Principal?.FindFirstValue("email") ??
        Principal?.FindFirstValue(ClaimTypes.Email);
    public IEnumerable<string> Roles =>
        Principal?.FindAll("role").Select(c => c.Value) ??
        Principal?.FindAll(ClaimTypes.Role).Select(c => c.Value) ??
        Enumerable.Empty<string>();

    public bool IsInRole(string role) =>
        Roles.Contains(role, StringComparer.OrdinalIgnoreCase);
}
