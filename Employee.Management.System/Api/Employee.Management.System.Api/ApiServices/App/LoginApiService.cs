using Employee.Management.System.ApiModels.App;
using Employee.Management.System.Common.Logging;
using Employee.Management.System.Domain.Core.Services.App;
using Employee.Management.System.Domain.Models.App;

namespace Employee.Management.System.Api.ApiServices.App
{
    public class LoginApiService : ILoginApiService
    {
        private readonly IUserService userService;

        public LoginApiService(IUserService userService)
        {
            this.userService = userService;
        }

        public async Task<Login> LoginAsync(LoginApiModel model)
        {
            var logContext = new LogContext("LoginApiService.LoginAsync");
            var methodContext = $"";
            logContext.StartDebug($"Started {methodContext}");
            try
            {
                // Use domain services to accomplish the work ...
                var login = await userService.LoginAsync(model).ConfigureAwait(false);

                // Construct response object ...
                return login;
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
