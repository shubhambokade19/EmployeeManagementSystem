namespace Employee.Management.System.Common.Email
{
    public class EmailServerSettings
    {
        public string SmtpServer { get; set; } = string.Empty;
        public string SmtpUser { get; set; } = string.Empty;
        public string SmtpPassword { get; set; } = string.Empty;
        public int SmtpPort { get; set; }
        public string SenderEmailId { get; set; } = string.Empty;
        public string ConfigSet { get; set; } = string.Empty;
    }
}
