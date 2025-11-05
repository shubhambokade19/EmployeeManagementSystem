using Employee.Management.System.Common.Core.Models;

namespace Employee.Management.System.Domain.Models.App
{
    public class UserLogin : ModelBase
    {
        protected override string? GetLabel()
        {
            return string.Empty;
        }
        protected override long GetValue()
        {
            return UserLoginId;
        }
        public long UserLoginId { get; set; }
        public string SessionId { get; set; } = string.Empty;
        public long UserId { get; set; }
        public DateTime StartTimeStamp { get; set; } = DateTime.Now;
        public DateTime? EndTimeStamp { get; set; } = null;
    }
}
