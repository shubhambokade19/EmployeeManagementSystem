using Employee.Management.System.Api.ApiServices.App;
using Employee.Management.System.ApiModels.App;
using Employee.Management.System.Common.Api;
using Employee.Management.System.Common.Logging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Employee.Management.System.Api.Controllers.App
{
    [Authorize]
    [Route("api/app/[controller]")]
    [ApiController]
    public class LogoutController : ApiBaseController
    {
        private readonly ILogoutApiService logoutApiService;

        public LogoutController(ILogoutApiService logoutApiService)
        {
            this.logoutApiService = logoutApiService;
        }

        [HttpPost("", Name = "Logout")]
        public async Task<ActionResult> Logout([FromBody] LogoutApiModel model)
        {
            var logContext = new LogContext("LogoutController.Logout");
            logContext.StartInfo();
            try
            {
                var result = await logoutApiService.LogoutAsync(model).ConfigureAwait(false);
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
