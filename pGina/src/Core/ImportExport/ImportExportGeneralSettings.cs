using System.Collections.Generic;

namespace pGina.Core.ImportExport
{
    public class ImportExportGeneralSettings
    {
        public bool ShowServiceStatusInLogonUi { get; set; }

        public bool UseOriginalUsernameInUnlockScenario { get; set; }

        public bool LastUsernameEnable { get; set; }

        public bool PreferLocalAuthentication { get; set; }

        public List<string> NTPServers { get; set; }

        public string TileImage { get; set; }

        public ImportExportMOTDSettings MOTDSettings { get; set; }

        public ImportExportEmailNotificationSettings EmailNotificationSettings { get; set; }
    }

    public class ImportExportMOTDSettings
    {
        public bool Enable { get; set; }

        public string Text { get; set; }
    }

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
