using Employee.Management.System.Common.Api;
using Employee.Management.System.Common.Helpers;
using System.Text;

namespace Employee.Management.System.Domain.Infrastructure.Search.App
{
    public class UserSearch
    {
        // Field below should be the primary key of the entity being searched ...
        private static string IdField = "u.UserId";
        // Define fields that can be chosen in the "Fields" in the search request for the given entity ...
        private static Dictionary<string, string> OutputFields = new Dictionary<string, string>
        {
            { "UserLogin", "u.UserLogin" },
            { "UserPassword", "u.UserPassword" },
            { "FirstName", "u.FirstName" },
            { "LastName", "u.LastName" },
            { "RealName", "u.RealName" },
            { "Active", "u.Active" },
            { "InsertUserId", "u.InsertUserId" },
            { "InsertTimestamp", "u.InsertTimestamp" },
            { "UpdateUserId", "u.UpdateUserId" },
            { "UpdateTimestamp", "u.UpdateTimestamp" }
        };
        // Define fields that can be used to sepcify the criterion in the search request for the given entity ...
        private static Dictionary<string, string> CriterionFields = new Dictionary<string, string>
        {
            { "UserId", "u.UserId" },
            { "Active", "u.Active" }
        };
        public static bool Validate(SearchRequest searchRequest)
        {
            return true;
        }
        public static string GetSqlStatement(Session session, SearchRequest searchRequest)
        {
            var sqlStatement = SearchHelper.GetSqlStatementForSearch(searchRequest, IdField, OutputFields, CriterionFields, GetFromClause(session));
            return sqlStatement;
        }
        private static string GetFromClause(Session session)
        {
            StringBuilder fromClause = new StringBuilder(1024);
            fromClause.Append($" FROM users AS u");
            return fromClause.ToString();
        }
    }
}
