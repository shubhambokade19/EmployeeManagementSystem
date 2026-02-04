using Employee.Management.System.Common.Api;
using Employee.Management.System.Common.Core.Services;
using Employee.Management.System.Common.Data;
using Employee.Management.System.Common.Helpers;
using Employee.Management.System.Common.Logging;
using Employee.Management.System.Domain.Core.Repositories.App;
using Employee.Management.System.Domain.Core.Services.App;
using Employee.Management.System.Domain.Infrastructure.Search.App;
using Employee.Management.System.Domain.Models.App;
using Microsoft.AspNetCore.Http;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace Employee.Management.System.Domain.Infrastructure.Services.App
{
    public class UserService : ServiceBase<User>, IUserService
    {

        private readonly ConnectionStrings connectionStrings;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly IUserRepository userRepository;
        private readonly IUserLoginRepository userLoginRepository;
        private readonly IAuthService authService;

        public UserService(ConnectionStrings connectionStrings,
            IHttpContextAccessor httpContextAccessor,
            IUserRepository userRepository,
            IUserLoginRepository userLoginRepository,
            IAuthService authService)
        {
            this.connectionStrings = connectionStrings;
            this.httpContextAccessor = httpContextAccessor;
            this.userRepository = userRepository;
            this.userLoginRepository = userLoginRepository;
            this.authService = authService;
        }

        public override Task<bool> ActivateAsync(Session session, string[] idList)
        {
            throw new NotImplementedException();
        }

        public override Task<bool> DeleteAsync(Session session, long id)
        {
            throw new NotImplementedException();
        }

        public override Task<User> GetByIdAsync(Session session, long id, bool includeEmbeddedObjects = false)
        {
            throw new NotImplementedException();
        }

        public async override Task<IEnumerable<User>> GetBySearchAsync<T>(Session session, SearchRequest searchRequest)
        {
            var logContext = new LogContext("UserService.GetBySearchAsync");
            logContext.StartDebug();
            try
            {
                logContext.StartTrace("Validating SearchRequest ...");
                if (!UserSearch.Validate(searchRequest))
                {
                    throw new ApiException($"Unsupported search request {searchRequest?.Intent}");
                }
                logContext.StopTrace();

                logContext.StartTrace("Obtaining Sql Statement ...");
                var sqlStatement = UserSearch.GetSqlStatement(session, searchRequest);
                if (string.IsNullOrEmpty(sqlStatement)) return [];
                logContext.StopTrace();

                logContext.StartTrace("Obtaining data ...");
                await session.OpenConnection();

                var list = await userRepository.GetBySqlStatementAsync<User>(session, sqlStatement);

                session.CloseConnection();
                logContext.StopTrace();

                return list;
            }
            catch (Exception ex)
            {
                LogHelper.Error(logContext, ex);
                throw;
            }
            finally
            {
                session.CloseConnection();
                logContext.StopDebug($"{searchRequest.Intent} Completed");
            }
        }

        public override Task<bool> InactivateAsync(Session session, string[] idList)
        {
            throw new NotImplementedException();
        }

        protected async override Task<User> InsertAsync(Session session, User entity, bool returnSavedEntity = false)
        {
            var logContext = new LogContext(session, "UserService.InsertAsync");
            var methodContext = $"";
            logContext.StartDebug($"Started {methodContext}");
            try
            {
                await session.OpenConnection();
                bool startedTransaction = await session.BeginTransaction();

                // Write code for business validations ...
                var validationErrorList = await ValidateInsert(session, entity);
                if (validationErrorList.Any())
                {
                    throw new ApiException(validationErrorList);
                }

                // Get encrypted password
                entity.UserPassword = EncryptPassword(entity.UserPassword);

                // Insert the record in Users table
                entity.UserId = await userRepository.InsertAsync(session, entity);

                session.CommitTransaction(startedTransaction);
                session.CloseConnection();

                return entity;
            }
            catch (Exception ex)
            {
                session.RollbackTransaction();
                LogHelper.Error(logContext, ex);
                throw;
            }
            finally
            {
                session.CloseConnection();
                logContext.StopDebug($"Completed {methodContext}");
            }
        }

        protected override Task<User> UpdateAsync(Session session, User entity, bool returnSavedEntity = false)
        {
            throw new NotImplementedException();
        }

        public async Task<Login> LoginAsync(Login entity)
        {
            var logContext = new LogContext("UserService.LoginAsync");
            var methodContext = $"";
            logContext.StartDebug($"Started {methodContext}");
            var dbConnectionString = await connectionStrings.GetConnectionStringByKey("Default");
            var session = new Session() { ConnectionString = dbConnectionString };
            try
            {
                // Write code for business validations ...
                var validationErrorList = ValidateLogin(session, entity);
                if (validationErrorList.Any())
                {
                    throw new ApiException(validationErrorList);
                }

                await session.OpenConnection();
                bool startedTransaction = await session.BeginTransaction();

                // Get encrypted password
                string? encryptedPassword = EncryptPassword(entity.UserPassword);

                // Get User details by UserLogin and Password
                var user = await userRepository.GetUserByUserLoginAndPasswordAsync(session, entity.UserLogin, encryptedPassword);
                if (user == null)
                {
                    throw new ApiException(new List<DomainValidationResult> { new DomainValidationResult { Message = "Invalid Username and Password." } });
                }

                // Map user details into session and entity
                session.UserId = user.UserId;
                entity.UserId = session.UserId;
                session.RealName = user?.RealName ?? string.Empty;
                entity.RealName = session.RealName;
                session.UserLogin = user?.UserLogin;

                // check user is already logged in or not
                var isUserLoggedIn = await userLoginRepository.IsUserLoggedInAsync(session, session.UserId);

                //If user is already logged in then thrown api exception
                if (isUserLoggedIn)
                {
                    throw new ApiException(new List<DomainValidationResult> { new DomainValidationResult { Message = $"{session.RealName} is already logged in." } }, HttpStatusCode.InternalServerError);
                }

                var context = httpContextAccessor.HttpContext;
                var sessionId = Guid.NewGuid().ToString();

                // Insert user login details in UserLogin table
                var userLogin = new UserLogin
                {
                    SessionId = sessionId,
                    UserId = session.UserId,
                    InsertUserId = session.UserId
                };
                session.SessionId = sessionId;
                var savedUserLogin = await userLoginRepository.InsertAsync(session, userLogin);

                session.CommitTransaction(startedTransaction);
                session.CloseConnection();

                // Generate JWT token
                entity.BearerToken = await authService.GenerateJwtTokenAsync(session);

                return entity;
            }
            catch (Exception ex)
            {
                session.RollbackTransaction();
                LogHelper.Error(logContext, ex);
                throw;
            }
            finally
            {
                session.CloseConnection();
                logContext.StopDebug($"Completed {methodContext}");
            }
        }

        public async Task<bool> LogoutAsync(Logout entity)
        {
            var logContext = new LogContext("UserService.LogoutAsync");
            var methodContext = $"";
            logContext.StartDebug($"Started {methodContext}");
            var dbConnectionString = await connectionStrings.GetConnectionStringByKey("Default");
            var session = new Session() { ConnectionString = dbConnectionString };
            bool value = false;
            try
            {
                // Write code for business validations ...
                var validationErrorList = ValidateLogout(session, entity);
                if (validationErrorList.Any())
                {
                    throw new ApiException(validationErrorList);
                }

                await session.OpenConnection();
                bool startedTransaction = await session.BeginTransaction();

                if (entity.UserId == 0)
                {
                    // Get encrypted password
                    string? encryptedPassword = EncryptPassword(entity.UserPassword);

                    // Get User details by UserLogin and Password
                    var user = await userRepository.GetUserByUserLoginAndPasswordAsync(session, entity.UserLogin, encryptedPassword);
                    if (user == null)
                    {
                        throw new ApiException(new List<DomainValidationResult> { new DomainValidationResult { Message = "Invalid Username and Password." } });
                    }
                    else
                    {
                        entity.UserId = user.UserId;
                        session.UserId = entity.UserId;
                    }
                }

                // check user is already logged in or not
                var isUserLoggedIn = await userLoginRepository.IsUserLoggedInAsync(session, entity.UserId);

                //If user is already log in then log out    
                if (isUserLoggedIn)
                {
                    // Update EndTimeStamp
                    value = await userLoginRepository.UpdateUserLoginAsync(session, entity.UserId);
                }

                session.CommitTransaction(startedTransaction);
                session.CloseConnection();

                return value;
            }
            catch (Exception ex)
            {
                session.RollbackTransaction();
                LogHelper.Error(logContext, ex);
                throw;
            }
            finally
            {
                session.CloseConnection();
                logContext.StopDebug($"Completed {methodContext}");
            }
        }

        private string EncryptPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = Encoding.UTF8.GetBytes(password);
                byte[] hash = sha256.ComputeHash(bytes);
                return Convert.ToBase64String(hash);
            }
        }

        private async Task<List<DomainValidationResult>> ValidateInsert(Session session, User? entity)
        {
            DomainValidationResult result;
            List<DomainValidationResult> resultList = new List<DomainValidationResult>();

            var user = await userRepository.GetUserByUserLoginAsync(session, entity?.UserLogin);
            if (user != null)
            {
                result = new DomainValidationResult { Success = false, Message = $"{entity?.UserLogin} is already exists." };
                resultList.Add(result);
            }
            result = ValidationHelper.ValidateRequiredField("First Name", entity?.FirstName);
            if (!result.Success)
            {
                resultList.Add(result);
            }
            result = ValidationHelper.ValidateRequiredField("Last Name", entity?.LastName);
            if (!result.Success)
            {
                resultList.Add(result);
            }
            result = ValidationHelper.ValidateRequiredField("Username", entity?.UserLogin);
            if (!result.Success)
            {
                resultList.Add(result);
            }
            result = ValidationHelper.ValidateEmail(entity?.UserLogin);
            if (!result.Success)
            {
                resultList.Add(result);
            }
            result = ValidationHelper.ValidateRequiredField("Password", entity?.UserPassword);
            if (!result.Success)
            {
                resultList.Add(result);
            }

            return resultList;
        }

        private List<DomainValidationResult> ValidateLogin(Session session, Login entity)
        {
            DomainValidationResult result;
            List<DomainValidationResult> resultList = new List<DomainValidationResult>();

            result = ValidationHelper.ValidateRequiredField("UserLogin", entity.UserLogin);
            if (!result.Success)
            {
                resultList.Add(result);
            }
            result = ValidationHelper.ValidateRequiredField("UserPassword", entity.UserPassword);
            if (!result.Success)
            {
                resultList.Add(result);
            }

            return resultList;
        }

        private List<DomainValidationResult> ValidateLogout(Session session, Logout entity)
        {
            DomainValidationResult result;
            List<DomainValidationResult> resultList = new List<DomainValidationResult>();

            if (entity.UserId == 0)
            {
                result = ValidationHelper.ValidateRequiredField("UserLogin", entity.UserLogin);
                if (!result.Success)
                {
                    resultList.Add(result);
                }
                result = ValidationHelper.ValidateRequiredField("UserPassword", entity.UserPassword);
                if (!result.Success)
                {
                    resultList.Add(result);
                }
            }

            return resultList;
        }
    }
}
