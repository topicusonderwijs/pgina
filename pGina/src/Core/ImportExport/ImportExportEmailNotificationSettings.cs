namespace pGina.Core.ImportExport
{
    public class ImportExportEmailNotificationSettings
    {
        public string SMTP { get; set; }

        public string Email { get; set; }

        public string User { get; set; }

        public string Password { get; set; }

        public bool UseCredentials { get; set; }

        public bool UseSSL { get; set; }
    }
}
