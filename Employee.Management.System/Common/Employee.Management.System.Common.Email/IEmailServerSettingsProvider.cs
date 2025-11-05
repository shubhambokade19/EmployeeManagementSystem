namespace Employee.Management.System.Common.Email
{
    public interface IEmailServerSettingsProvider
    {
        EmailServerSettings GetEmailServerSettings();
    }
}
