using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace pGina.Configuration
{
    public class pGinaImportExportPluginSetting
    {
        public string Name { get; set; }
        public JToken Settings { get; set; }
        public Guid Uuid { get; set; }
    }
}
