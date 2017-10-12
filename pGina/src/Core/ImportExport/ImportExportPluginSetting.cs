using Newtonsoft.Json.Linq;
using System;

namespace pGina.Core.ImportExport
{
    public class ImportExportPluginSetting
    {
        public Guid Uuid { get; set; }

        public string Name { get; set; }

        public JToken Settings { get; set; }

        public bool AuthenticateEnabled { get; set; }

        public bool AuthorizeEnabled { get; set; }

        public bool GatewayEnabled { get; set; }

        public bool NotificationEnabled { get; set; }

        public bool ChangePasswordEnabled { get; set; }               
    }
}
