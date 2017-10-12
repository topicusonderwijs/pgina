using System.Collections.Generic;

namespace pGina.Core.ImportExport
{
    public class ImportExportReport
    {
        public List<ImportExportReportRow> Rows { get; set; }
    }

    public class ImportExportReportRow
    {
        public ImportExportReportMessageLevel MessageLevel { get; set; }

        public string Message { get; set; }
    }

    public enum ImportExportReportMessageLevel
    {
        Error = 0,
        Warning = 1,
        Info = 2
    }
}
