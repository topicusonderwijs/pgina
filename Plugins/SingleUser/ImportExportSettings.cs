namespace pGina.Plugin.SingleUser
{
    internal class ImportExportSettings
    {
        public string Username { get; internal set; }

        public string Domain { get; internal set; }

        public string Password { get; internal set; }

        public bool RequirePlugins { get; internal set; }

        public bool RequireAllPlugins { get; internal set; }

        public string[] RequiredPluginList { get; internal set; }
    }
}