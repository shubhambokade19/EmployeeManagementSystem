using System.Net.Mail;

namespace Employee.Management.System.Common.Email
{
    public class EmailMessage
    {
        public string FromId { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string ToId { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public string HtmlBody { get; set; } = string.Empty;
        public LinkedResource? LinkedResources { get; set; }
        public string ToCC { get; set; } = string.Empty;
        public string SmtpServer { get; set; } = string.Empty;
        public int SmtpPort { get; set; } = 0;
        public bool EnableSsl { get; set; } = false;
    }
}
