namespace pGina.Plugin.pgSMB2
{
    internal class ImportExportSettings
    {
        public string SMBshare { get; internal set; }
        public string RoamingSource { get; internal set; }
        public string Filename { get; internal set; }
        public string TempComp { get; internal set; }
        public int ConnectRetry { get; internal set; }
        public string Compressor { get; internal set; }
        public string CompressCLI { get; internal set; }
        public string UncompressCLI { get; internal set; }

        public string HomeDir { get; internal set; }
        public string HomeDirDrive { get; internal set; }
        public string ScriptPath { get; internal set; }
        public int MaxStore { get; internal set; }
        public string MaxStoreExclude { get; internal set; }
        public string[] MaxStoreText { get; internal set; }
        public string ACE { get; internal set; }
    }
}