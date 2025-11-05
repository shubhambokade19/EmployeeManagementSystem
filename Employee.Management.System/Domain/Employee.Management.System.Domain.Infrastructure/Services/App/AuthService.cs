using Employee.Management.System.Common.Api;
using Employee.Management.System.Common.Logging;
using Employee.Management.System.Domain.Core.Services.App;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Employee.Management.System.Domain.Infrastructure.Services.App
{
    public class AuthService : IAuthService
    {
        private readonly IConfiguration configuration;
        private readonly IHttpContextAccessor httpContextAccessor;
        public AuthService(IConfiguration configuration,
            IHttpContextAccessor httpContextAccessor)
        {
            this.configuration = configuration;
            this.httpContextAccessor = httpContextAccessor;
        }

        public async Task<string?> GenerateJwtTokenAsync(Session session)
        {
            var logContext = new LogContext("AuthService.GenerateJwtTokenAsync");
            logContext.StartDebug("Started");
            try
            {
                var domain = configuration["Authentication:Domain"];
                var audience = configuration["Authentication:Audience"];
                var secretKey = configuration["Authentication:SecretKey"] ?? string.Empty;
                var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
                var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, session.UserLogin ?? string.Empty),
                    new Claim("SessionId", session?.SessionId ?? string.Empty),
                    new Claim("UserId", session?.UserId.ToString() ?? string.Empty),
                    new Claim("UserLogin", session?.UserLogin ?? string.Empty)
                };

                var tokeOptions = new JwtSecurityToken(
                    issuer: domain,
                    audience: audience,
                    claims: claims,
                    signingCredentials: credentials
                );

                var tokenString = new JwtSecurityTokenHandler().WriteToken(tokeOptions);
                return await Task.FromResult(tokenString);
            }
            catch (Exception ex)
            {
                LogHelper.Error(logContext, ex);
                throw;
            }
            finally
            {
                logContext.StopDebug("Completed");
            }
        }

        public async Task<ClaimsPrincipal> GetJwtTokenDataAsync(Session session)
        {
            var logContext = new LogContext("AuthService.GetJwtTokenDataAsync");
            logContext.StartDebug("Started");
            try
            {
                // Retrieve the Authorization header from the HTTP request
                string authorizationHeader = httpContextAccessor?.HttpContext?.Request?.Headers["Authorization"].FirstOrDefault() ?? string.Empty;

                // Ensure the Authorization header exists and starts with "Bearer "
                if (authorizationHeader == null || !authorizationHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                {
                    throw new ArgumentException("Bearer token not found in Authorization header.");
                }

                // Extract the JWT token from the Authorization header
                string encodedJwt = authorizationHeader.Substring("Bearer ".Length).Trim();

                // Retrieve the secret key from configuration and convert to byte array
                var secretKey = Encoding.UTF8.GetBytes(configuration["Authentication:SecretKey"] ?? string.Empty);

                // Initialize a JWT token handler and set validation parameters
                var tokenHandler = new JwtSecurityTokenHandler();
                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = false,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = configuration["Authentication:Domain"],
                    ValidAudience = configuration["Authentication:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(secretKey),
                    ClockSkew = TimeSpan.Zero
                };

                // Validate the JWT token and retrieve the principal
                SecurityToken validatedToken;
                var principal = tokenHandler.ValidateToken(encodedJwt, validationParameters, out validatedToken);

                // Check if principal.Claims is null
                if (principal == null)
                {
                    throw new InvalidOperationException("Unauthorized");
                }

                // If principal.Claims is not null, return it
                return await Task.FromResult(principal);
            }
            catch (Exception ex)
            {
                LogHelper.Error(logContext, ex);
                throw;
            }
            finally
            {
                logContext.StopDebug("Completed");
            }
        }
    }
}
