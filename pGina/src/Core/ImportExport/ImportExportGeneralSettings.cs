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
}
