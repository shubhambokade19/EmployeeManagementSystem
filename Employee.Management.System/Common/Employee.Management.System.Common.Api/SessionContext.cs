using System.Text;

namespace Employee.Management.System.Common.Api
{
    public class SessionContext
    {
        public long SessionId { get; set; }
        public string AuthToken { get; set; }

        public string GetEncodedSessionContext()
        {
            var contextString = new StringBuilder();

            contextString.Append(SessionId);
            contextString.Append(";");
            contextString.Append(AuthToken);

            return Convert.ToBase64String(Encoding.UTF8.GetBytes(contextString.ToString()));
        }

        public SessionContext(string encodedSessionContext)
        {
            var decodedString = Encoding.UTF8.GetString(Convert.FromBase64String(encodedSessionContext));
            var properties = decodedString.Split(';');
            if (properties.Length != 2)
                throw new ArgumentOutOfRangeException($"Invalid SessionContext. '{properties.Length}/2");

            this.SessionId = Convert.ToInt64(properties[0]);
            this.AuthToken = properties[1];
        }
    }
}
