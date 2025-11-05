using Employee.Management.System.Common.Api;
using Employee.Management.System.Common.Data;
using Employee.Management.System.Common.Logging;
using Employee.Management.System.Domain.Core.Repositories.App;
using Employee.Management.System.Domain.Core.Services.App;
using Microsoft.Extensions.Configuration;
using System.Net;

namespace Employee.Management.System.Domain.Infrastructure.Services.App
{
    public class UserSessionService : IUserSessionService
    {
        private readonly ConnectionStrings connectionStrings;
        private readonly IUserLoginRepository userLoginRepository;
        private readonly IConfiguration configuration;
        private readonly IAuthService authService;
        private readonly IUserRepository userRepository;

        public UserSessionService(ConnectionStrings connectionStrings,
            IUserLoginRepository userLoginRepository,
            IConfiguration configuration,
            IAuthService authService,
            IUserRepository userRepository)
        {
            this.connectionStrings = connectionStrings;
            this.userLoginRepository = userLoginRepository;
            this.configuration = configuration;
            this.authService = authService;
            this.userRepository = userRepository;
        }

        public string GetConnectionString()
        {
            return connectionStrings.GetConnectionStringByKey("Default").Result;
        }

        public async Task<Session> GetValidUserSessionAsync(Session session)
        {
            var logContext = new LogContext("UserSessionService.GetValidUserSessionByIdAsync");
            var methodContext = $"";
            logContext.StartDebug($"Started {methodContext}");
            try
            {
                // Retrieve JWT token from request headers
                var jwtTokenData = await authService.GetJwtTokenDataAsync(session);

                // Extract claims values
                string? sessionId = jwtTokenData.Claims.FirstOrDefault(c => c.Type == "SessionId")?.Value;
                session.SessionId = sessionId;
                long userId = Convert.ToInt64(jwtTokenData.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value);
                session.UserId = userId;
                string? userLogin = jwtTokenData.Claims.FirstOrDefault(c => c.Type == "UserLogin")?.Value;
                session.UserLogin = userLogin;

                // Get default connection string and database 
                session.ConnectionString = GetConnectionString();
                session.DatabaseName = await connectionStrings.GetParameterValueFromConnectionString(session.ConnectionString, "Database");

                await session.OpenConnection();

                // Get User login data for mapping into session
                var userLoginDetails = await userLoginRepository.GetUserBySessionIdAndUserIdAsync(session, sessionId, session.UserId);

                if (userLoginDetails == null)
                {
                    throw new ApiException(new List<DomainValidationResult> { new DomainValidationResult { Message = "Unauthorized access. Please Log out and Log in again." } }, HttpStatusCode.Unauthorized);
                }

                // Map RealName into session
                session.RealName = userLoginDetails.RealName;

                session.CloseConnection();
            }
            finally
            {
                session.CloseConnection();
                logContext.StopDebug($"Completed {methodContext}");
            }
            return session;
        }
    }
}
