namespace Employee.Management.System.Common.Email
{
    public interface IEmailSenderService
    {
        Task<bool> SendAsync(EmailMessage emailMessage);
    }
}
