using System;
using System.Collections.Generic;

namespace pGina.Core.ImportExport
{
    public class ImportExportSettings
    {
        public string Version { get; set; }

        public ImportExportGeneralSettings GeneralSettings { get; set; }

        public List<ImportExportPluginSetting> PluginSettings { get; set; }

        public bool DisableNonExportedPlugins { get; set; }

        public List<Guid> AuthenticatePluginOrder { get; set; }

        public List<Guid> AuthorizePluginOrder { get; set; }

        public List<Guid> ChangePasswordPluginOrder { get; set; }

        public List<Guid> GatewayPluginOrder { get; set; }

        public List<Guid> NotificationPluginOrder { get; set; }

        public List<ImportExportDisabledCredentialProvider> DisabledCredentialProviders { get; set; }
    }
}
