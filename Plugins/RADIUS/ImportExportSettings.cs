namespace pGina.Plugin.RADIUS
{
    internal class ImportExportSettings
    {
        public bool EnableAuth { get; internal set; }
        public bool EnableAcct { get; internal set; }
        public string Server { get; internal set; }
        public int AuthPort { get; internal set; }
        public int AcctPort { get; internal set; }
        public string SharedSecret { get; internal set; }
        public int Timeout { get; internal set; }
        public int Retry { get; internal set; }

        public bool SendNASIPAddress { get; internal set; }
        public bool SendNASIdentifier { get; internal set; }
        public string NASIdentifier { get; internal set; }
        public bool SendCalledStationID { get; internal set; }
        public string CalledStationID { get; internal set; }

        public bool AcctingForAllUsers { get; internal set; }
        public bool SendInterimUpdates { get; internal set; }
        public bool ForceInterimUpdates { get; internal set; }
        public int InterimUpdateTime { get; internal set; }

        public bool AllowSessionTimeout { get; internal set; }
        public bool WisprSessionTerminate { get; internal set; }

        public bool UseModifiedName { get; internal set; }
        public string IPSuggestion { get; internal set; }
    }
}