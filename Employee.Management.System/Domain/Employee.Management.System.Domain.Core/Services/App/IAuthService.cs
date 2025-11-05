using Employee.Management.System.Common.Api;
using System.Security.Claims;

namespace Employee.Management.System.Domain.Core.Services.App
{
    public interface IAuthService
    {
        Task<string?> GenerateJwtTokenAsync(Session session);
        Task<ClaimsPrincipal> GetJwtTokenDataAsync(Session session);
    }
}
