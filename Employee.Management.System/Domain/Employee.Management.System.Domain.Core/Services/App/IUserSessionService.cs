using Employee.Management.System.Common.Api;

namespace Employee.Management.System.Domain.Core.Services.App
{
    public interface IUserSessionService
    {
        string GetConnectionString();
        Task<Session> GetValidUserSessionAsync(Session session);
    }
}
