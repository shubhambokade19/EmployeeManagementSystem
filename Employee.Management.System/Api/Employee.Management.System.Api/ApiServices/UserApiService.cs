using Employee.Management.System.ApiModels;
using Employee.Management.System.Common.Api;
using Employee.Management.System.Common.Logging;
using Employee.Management.System.Domain.Core.Services.App;
using Employee.Management.System.Domain.Models.App;

namespace Employee.Management.System.Api.ApiServices
{
    public class UserApiService : IUserApiService
    {
        private readonly IUserService userService;

        public UserApiService(IUserService userService)
        {
            this.userService = userService;
        }

        public async Task<User> AddAsync(Session session, UserApiModel model)
        {
            var logContext = new LogContext("UserApiService.AddAsync");
            var methodContext = $"";
            logContext.StartDebug($"Started {methodContext}");
            try
            {
                // Use domain services to accomplish the work ...
                var savedUser = await userService.SaveAsync(session, model).ConfigureAwait(false);

                // Construct response object ...
                return savedUser;
            }
            catch (Exception ex)
            {
                LogHelper.Error(logContext, ex);
                throw;
            }
            finally
            {
                logContext.StopDebug($"Completed {methodContext}");
            }
        }
    }
}
