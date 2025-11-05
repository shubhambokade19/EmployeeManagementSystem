using Employee.Management.System.Common.Api;
using Employee.Management.System.Common.Data;
using Employee.Management.System.Domain.Core.Services.App;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Employee.Management.System.Api.Controllers
{
    public class ApiBaseController : ControllerBase, IApiController
    {
        protected readonly IUserSessionService? userSessionService = null;
        protected readonly ConnectionStrings? connectionStrings = null;
        protected readonly IAuthorizationService? authorizationService = null;

        public ApiBaseController() : base()
        {
        }
        public ApiBaseController(IUserSessionService userSessionService, ConnectionStrings connectionStrings) : base()
        {
            this.userSessionService = userSessionService;
            this.connectionStrings = connectionStrings;
        }

        public ApiBaseController(IUserSessionService userSessionService, ConnectionStrings connectionStrings, IAuthorizationService authorizationService) : base()
        {
            this.userSessionService = userSessionService;
            this.connectionStrings = connectionStrings;
            this.authorizationService = authorizationService;
        }

        protected ObjectResult InternalServerError(Exception ex)
        {
            if (ex is ApiUnauthorizedAccessException)
            {
                return StatusCode(StatusCodes.Status401Unauthorized, new { Message = ex.Message });
            }
            return StatusCode(StatusCodes.Status500InternalServerError, new { Message = ex.Message });
        }

        protected ObjectResult BadRequest(Exception ex)
        {
            return StatusCode(StatusCodes.Status400BadRequest, new { Message = ex.Message });
        }

        protected async Task<Session> GetSessionAsync()
        {
            if (connectionStrings == null)
            {
                throw new InvalidOperationException("ConnectionStrings cannot be null.");
            }

            var dbConnectionString = await connectionStrings.GetConnectionStringByKey("Default");
            var session = new Session() { ConnectionString = dbConnectionString };

            if (userSessionService == null)
            {
                throw new InvalidOperationException("UserSessionService cannot be null.");
            }

            // Get session data
            session = await userSessionService.GetValidUserSessionAsync(session).ConfigureAwait(false);

            return session;
        }
        protected async Task<Session> GetSessionAsync(string[] claimList)
        {
            var session = await GetSessionAsync();
            return session;
        }
    }
}
