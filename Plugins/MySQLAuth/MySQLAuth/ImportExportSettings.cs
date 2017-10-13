namespace pGina.Plugin.MySQLAuth
{
    internal class ImportExportSettings
    {
        //Connection
        public string Host { get; internal set; }
        public int Port { get; internal set; }
        public bool UseSsl { get; internal set; }
        public string User { get; internal set; }
        public string Password { get; internal set; }
        public string Database { get; internal set; }

        // User table
        public string Table { get; internal set; }
        public int HashEncoding { get; internal set; }
        public string UsernameColumn { get; internal set; }
        public string HashMethodColumn { get; internal set; }
        public string PasswordColumn { get; internal set; }
        public string UserTablePrimaryKeyColumn { get; internal set; }

        // Group table
        public string GroupTableName { get; internal set; }
        public string GroupNameColumn { get; internal set; }
        public string GroupTablePrimaryKeyColumn { get; internal set; }

        // User-Group table
        public string UserGroupTableName { get; internal set; }
        public string UserForeignKeyColumn { get; internal set; }
        public string GroupForeignKeyColumn { get; internal set; }

        // Authz Settings
        public string[] GroupAuthzRules { get; internal set; }
        public bool AuthzRequireMySqlAuth { get; internal set; }

        // Gateway settings
        public string[] GroupGatewayRules { get; internal set; }
        public bool PreventLogonOnServerError { get; internal set; }
    }
}