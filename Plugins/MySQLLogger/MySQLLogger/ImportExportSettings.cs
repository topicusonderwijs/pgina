namespace pGina.Plugin.MySqlLogger
{
    internal class ImportExportSettings
    {
        public bool EventMode { get; internal set; }
        public bool SessionMode { get; internal set; }
        public string Host { get; internal set; }
        public int Port { get; internal set; }
        public string User { get; internal set; }
        public string Password { get; internal set; }
        public string SessionTable { get; internal set; }
        public string EventTable { get; internal set; }

        public bool EvtLogon { get; internal set; }
        public bool EvtLogoff { get; internal set; }
        public bool EvtLock { get; internal set; }
        public bool EvtUnlock { get; internal set; }
        public bool EvtConsoleConnect { get; internal set; }
        public bool EvtConsoleDisconnect { get; internal set; }
        public bool EvtRemoteControl { get; internal set; }
        public bool EvtRemoteConnect { get; internal set; }
        public bool EvtRemoteDisconnect { get; internal set; }

        public bool UseModifiedName { get; internal set; }
    }
}