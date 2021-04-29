namespace Discounting.API.Common.Options
{
    public class SmtpSettingsOptions
    {
        public string Server { get; set; }
        public int Port { get; set; }
        public string SenderName { get; set; }
        public string SenderEmail { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public bool NeedAuth { get; set; }
        public int SecureSocketOption { get; set; }
    }
}