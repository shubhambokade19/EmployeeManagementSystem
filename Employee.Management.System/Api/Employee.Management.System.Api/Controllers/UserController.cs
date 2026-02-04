using Employee.Management.System.Api.ApiServices;
using Employee.Management.System.ApiModels;
using Employee.Management.System.Common.Api;
using Employee.Management.System.Common.Data;
using Employee.Management.System.Common.Logging;
using Employee.Management.System.Domain.Core.Services.App;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Employee.Management.System.Api.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ApiBaseController
    {
        private readonly IUserApiService userApiService;

        public UserController(ConnectionStrings connectionStrings, IUserSessionService userSessionService,
            IUserApiService userApiService) : base(userSessionService, connectionStrings)
        {
            this.userApiService = userApiService;
        }

        [HttpPost("", Name = "AddUser")]
        public async Task<ActionResult> AddUser([FromBody] UserApiModel model)
        {
            var logContext = new LogContext("UserController.AddUser");
            logContext.StartInfo();
            try
            {
                using var session = await GetSessionAsync().ConfigureAwait(false);
                var result = await userApiService.AddAsync(session, model).ConfigureAwait(false);
                return Created("AddUser", result);
            }
            catch (ApiException ex)
            {
                LogHelper.Error(logContext, ex);
                return StatusCode((int)ex.StatusCode, ex.ErrorList);
            }
            catch (Exception ex)
            {
                LogHelper.Error(logContext, ex);
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
            finally
            {
                logContext.StopInfo();
            }
        }

        [HttpPost("search", Name = "SearchUser")]
        public async Task<ActionResult> SearchUser([FromBody] SearchRequest searchRequest)
        {
            var logContext = new LogContext("UserController.SearchUser");
            logContext.StartInfo();
            try
            {
                using var session = await GetSessionAsync().ConfigureAwait(false);
                var result = await userApiService.SearchAsync(session, searchRequest).ConfigureAwait(false);
                return Ok(result);
            }
            catch (ApiException ex)
            {
                LogHelper.Error(logContext, ex);
                return StatusCode((int)ex.StatusCode, ex.ErrorList);
            }
            catch (Exception ex)
            {
                LogHelper.Error(logContext, ex);
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
            finally
            {
                logContext.StopInfo();
            }
        }
    }
}
