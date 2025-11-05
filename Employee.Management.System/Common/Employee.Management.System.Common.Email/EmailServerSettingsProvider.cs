using Microsoft.Extensions.Configuration;

namespace Employee.Management.System.Common.Email
{
    public class EmailServerSettingsProvider : IEmailServerSettingsProvider
    {
        private readonly EmailServerSettings emailServerSettings;

        public EmailServerSettingsProvider(IConfiguration _configuration)
        {
            var settings = _configuration.GetSection("EmailServerSettings");
            emailServerSettings = new EmailServerSettings
            {
                SmtpServer = settings["SmtpServer"] ?? string.Empty,
                SmtpPassword = settings["SenderPassword"] ?? string.Empty,
                SenderEmailId = settings["SenderEmail"] ?? string.Empty,
            };
            int.TryParse(settings["SmtpPort"], out int smtpPort);
            emailServerSettings.SmtpPort = smtpPort;
        }

        public EmailServerSettings GetEmailServerSettings()
        {
            return emailServerSettings;
        }
    }
}
