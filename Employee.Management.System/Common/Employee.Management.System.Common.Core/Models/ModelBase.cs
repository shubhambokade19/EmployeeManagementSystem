using Employee.Management.System.Common.Api;
using System.Xml.Serialization;

namespace Employee.Management.System.Common.Core.Models
{
    public abstract class ModelBase
    {
        public long Value => GetValue();
        public string? Label => GetLabel();

        public int Active { get; set; } = 2;
        public long InsertUserId { get; set; }
        public long UpdateUserId { get; set; }
        public DateTime InsertTimestamp { get; set; }
        public DateTime? UpdateTimestamp { get; set; }

        public void SetDataContextFields(Session session)
        {
            InsertUserId = session.InsertUserId;
            InsertTimestamp = session.InsertTimestamp;
            UpdateUserId = session.UpdateUserId;
            UpdateTimestamp = session.UpdateTimestamp;
        }

        protected abstract long GetValue();

        protected abstract string? GetLabel();
    }
}
