using Employee.Management.System.ApiModels.App;
using Employee.Management.System.Common.Logging;
using Employee.Management.System.Domain.Core.Services.App;

namespace Employee.Management.System.Api.ApiServices.App
{
    public class LogoutApiService : ILogoutApiService
    {
        private readonly IUserService userService;

        public LogoutApiService(IUserService userService)
        {
            this.userService = userService;
        }

        public async Task<bool> LogoutAsync(LogoutApiModel model)
        {
            var logContext = new LogContext("LogoutApiService.LogoutAsync");
            var methodContext = $"";
            logContext.StartDebug($"Started {methodContext}");
            try
            {
                // Use domain services to accomplish the work ...
                var logout = await userService.LogoutAsync(model).ConfigureAwait(false);

                // Construct response object ...
                return logout;
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
