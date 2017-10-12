using System;

namespace pGina.Core.ImportExport
{
    public class ImportExportDisabledCredentialProvider
    {
        public string Name { get; set; }
        public Guid Uuid { get; set; }

        public bool FilterLogon { get; set; }
        public bool FilterUnlock { get; set; }
        public bool FilterChangePass { get; set; }
        public bool FilterCredUI { get; set; }
    }
}
