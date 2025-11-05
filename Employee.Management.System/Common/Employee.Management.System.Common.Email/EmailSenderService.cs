using Employee.Management.System.Common.Api;
using Employee.Management.System.Common.Helpers;
using Employee.Management.System.Common.Logging;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;

namespace Employee.Management.System.Common.Email
{
    public class EmailSenderService : IEmailSenderService
    {
        private readonly EmailServerSettings emailServerSettings;

        public EmailSenderService(IEmailServerSettingsProvider settingsProvider)
        {
            emailServerSettings = settingsProvider.GetEmailServerSettings();
        }

        public async Task<bool> SendAsync(EmailMessage emailMessage)
        {
            var logContext = new LogContext(new Session { UserLogin = emailMessage.ToId }, "EmailSenderService.SendAsync");
            var methodContext = $"emailMessage = {emailMessage.Subject} To {emailMessage.ToId}";
            logContext.StartDebug($"Started {methodContext}");
            try
            {
                // Validate CC Email
                if (!string.IsNullOrEmpty(emailMessage.ToCC))
                {
                    emailMessage.ToCC = GetValidEmail(emailMessage.ToCC);
                }

                logContext.StartTrace("Constructing message ...");
                var mailMessage = new MailMessage(from: emailServerSettings.SenderEmailId, to: emailMessage.ToId)
                {
                    Subject = emailMessage.Subject,
                    Body = emailMessage.Body
                };
                if (!string.IsNullOrEmpty(emailMessage.ToCC))
                {
                    mailMessage.CC.Add(emailMessage.ToCC);
                }
                if (!string.IsNullOrEmpty(emailMessage.HtmlBody))
                {
                    var contentType = new ContentType("text/html");
                    AlternateView alternateView = AlternateView.CreateAlternateViewFromString(emailMessage.HtmlBody, contentType);
                    mailMessage.AlternateViews.Add(alternateView);
                    mailMessage.IsBodyHtml = true;
                    mailMessage.Body = emailMessage.HtmlBody;
                    if (!string.IsNullOrEmpty(emailServerSettings.ConfigSet))
                    {
                        mailMessage.Headers.Add("X-SES-CONFIGURATION-SET", emailServerSettings.ConfigSet);
                    }
                    if (emailMessage.LinkedResources != null)
                        alternateView.LinkedResources.Add(emailMessage.LinkedResources);
                }
                logContext.StopTrace("Constructing message ...Completed");

                using (var smtpClient = new SmtpClient(!string.IsNullOrEmpty(emailMessage.SmtpServer) ? emailMessage.SmtpServer : emailServerSettings.SmtpServer, emailMessage.SmtpPort != 0 ? emailMessage.SmtpPort : emailServerSettings.SmtpPort))
                {
                    logContext.StartTrace("Sending message through SMTP Server ...");

                    smtpClient.UseDefaultCredentials = false;
                    smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
                    if (emailMessage.EnableSsl)
                        smtpClient.EnableSsl = true;
                    smtpClient.Credentials = new NetworkCredential(!string.IsNullOrEmpty(emailMessage.FromId) ? emailMessage.FromId : emailServerSettings.SenderEmailId, !string.IsNullOrEmpty(emailMessage.Password) ? emailMessage.Password : emailServerSettings.SmtpPassword);

                    smtpClient.Send(mailMessage);
                    logContext.StopTrace("Sending message through SMTP Server ...Completed");
                }
                return await Task.FromResult(true);
            }
            catch (Exception ex)
            {
                LogHelper.Error(logContext, ex);
                throw;
            }
            finally
            {
                logContext.StopDebug($"Completed {methodContext}");
            }
        }

        private string GetValidEmail(string emailList)
        {
            var logContext = new LogContext("EmailSenderService.GetValidEmail");
            logContext.StartTrace($"Email list: {emailList}");
            // Comma separated email addresses
            string[] emails = emailList.Split(',');

            // Validated email addresses
            var validEmails = new List<string>();
            foreach (string email in emails)
            {
                var validationResult = ValidationHelper.ValidateEmail(email);
                if (validationResult.Success)
                {
                    validEmails.Add(email);
                }
                else
                {
                    logContext.StartTrace($"Invalid Email: {email}");
                    logContext.StopTrace($"Invalid Email: {email} - Completed");
                }
            }

            string result = string.Join(",", validEmails);
            logContext.StopTrace($"Valid Email: {result} - Completed");
            return result;
        }
    }
}
