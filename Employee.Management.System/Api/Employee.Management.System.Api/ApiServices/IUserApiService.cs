using Employee.Management.System.ApiModels;
using Employee.Management.System.Common.Api;
using Employee.Management.System.Domain.Models.App;

namespace Employee.Management.System.Api.ApiServices
{
    public interface IUserApiService : IApiService
    {
        Task<User> AddAsync(Session session, UserApiModel model);
        Task<List<User>> SearchAsync(Session session, SearchRequest searchRequest);
    }
}
