using Employee.Management.System.Api.ApiServices.App;
using Employee.Management.System.ApiModels.App;
using Employee.Management.System.Common.Api;
using Employee.Management.System.Common.Logging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Employee.Management.System.Api.Controllers.App
{
    [AllowAnonymous]
    [Route("api/app/[controller]")]
    [ApiController]
    public class LoginController : ApiBaseController
    {
        private readonly ILoginApiService loginApiService;

        public LoginController(ILoginApiService loginApiService)
        {
            this.loginApiService = loginApiService;
        }

        [HttpPost("", Name = "Login")]
        public async Task<ActionResult> Login([FromBody] LoginApiModel model)
        {
            var logContext = new LogContext("LoginController.Login");
            logContext.StartInfo();
            try
            {
                var result = await loginApiService.LoginAsync(model).ConfigureAwait(false);
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
