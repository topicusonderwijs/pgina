using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace pGina.Configuration
{
    public class pGinaImportExportPluginSetting
    {
        public bool AuthenticateEnabled { get; set; }
        public bool AuthorizeEnabled { get; set; }
        public bool ChangePasswordEnabled { get; set; }
        public bool GatewayEnabled { get; set; }
        public string Name { get; set; }
        public bool NotificationEnabled { get; set; }
        public JToken Settings { get; set; }
        public Guid Uuid { get; set; }
    }
}
