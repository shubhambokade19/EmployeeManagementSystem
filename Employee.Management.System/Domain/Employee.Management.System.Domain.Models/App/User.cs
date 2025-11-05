using Employee.Management.System.Common.Core.Models;

namespace Employee.Management.System.Domain.Models.App
{
    public class User : ModelBase
    {
        protected override string? GetLabel()
        {
            if (string.IsNullOrEmpty(UserLogin))
                return string.Empty;
            else
                return UserLogin;
        }
        protected override long GetValue()
        {
            return UserId;
        }
        public long UserId { get; set; }
        public string? UserLogin { get; set; }
        public string UserPassword { get; set; } = string.Empty;
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        private string? _realName;
        public string? RealName
        {
            get => string.IsNullOrWhiteSpace(_realName)
                ? $"{FirstName} {LastName}".Trim()
                : _realName;
            set => _realName = value;
        }
    }
}
