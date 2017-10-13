namespace pGina.Plugin.TopicusKeyHub.Settings.Model
{
    using Shared;

    public class TopicusKeyHubSettings
    {
        // The dynamic settings are from pGina. The magic is happing only here.
        private static TopicusKeyHubSettings _instance;
        private readonly dynamic settings;

        private TopicusKeyHubSettings(ref dynamic settings)
        {
            this.settings = settings;
        }

        public static TopicusKeyHubSettings GetInstance(ref dynamic settings)
        {
            if (_instance == null)
            {
                _instance = new TopicusKeyHubSettings(ref settings);
            }
            return _instance;
        }

        public ConnectionSettings GetConnectionSettings
        {
            get
            {
                int ldapTimeout = this.settings.LdapTimeout;
                bool requireCert = this.settings.RequireCert;
                string[] ldapHost = this.settings.LdapHost;
                int ldapPort = this.settings.LdapPort;
                string searchDN = this.settings.SearchDN;
                string searchPw = this.settings.GetEncryptedSetting("SearchPW");
                string serverCertFile = this.settings.ServerCertFile;
                return new ConnectionSettings(ldapHost, ldapPort, ldapTimeout, requireCert, serverCertFile, searchDN,
                    searchPw);
            }
        }

        public void SetConnectionSettings(ConnectionSettings connectionSettings)
        {
            this.settings.LdapTimeout = connectionSettings.LdapTimeout;
            this.settings.LdapHost = connectionSettings.LdapHosts.EmptyStringArrayIfNull();
            this.settings.LdapPort = connectionSettings.LdapPort;
            this.settings.RequireCert = connectionSettings.RequireCert;
            this.settings.ServerCertFile = connectionSettings.ServerCertFile.EmptyStringIfNull();
            this.settings.SearchDN = connectionSettings.SearchDN.EmptyStringIfNull();
            this.settings.SetEncryptedSetting("SearchPW",connectionSettings.SearchPW.EmptyStringIfNull());
        }

        public GroupSettings GetGroupSettings
        {
            get
            {
                bool dynamic = this.settings.Dynamic;
                string[] groups = this.settings.Groups;
                return new GroupSettings(groups, dynamic);
            }
        }

        public void SetGroupsSettings(GroupSettings groupSettings)
        {
            this.settings.Groups = groupSettings.Groups.EmptyStringArrayIfNull();
            this.settings.Dynamic = groupSettings.Dynamic;
        }
    }
}
