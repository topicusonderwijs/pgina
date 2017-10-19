using pGina.Shared;

namespace pGina.Plugin.TopicusKeyHub.Settings.Model
{
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
                string bindDN = this.settings.BindDN;
                string bindPw = this.settings.GetEncryptedSetting("BindPW");
                string serverCertFile = this.settings.ServerCertFile;
                bool dnsCheck = this.settings.DNSCheck;
                string certSubjectBind = this.settings.CertSubjectBind;
                bool useWindowsStoreBind = this.settings.UseWindowsStoreBind;
                bool useWindowsStoreConnection = this.settings.UseWindowsStoreConnection;            
                return new ConnectionSettings(ldapHost, ldapPort, ldapTimeout, requireCert, serverCertFile, bindDN,
                    bindPw, certSubjectBind, dnsCheck, useWindowsStoreBind, useWindowsStoreConnection);
            }
        }

        public void SetConnectionSettings(ConnectionSettings connectionSettings)
        {
            this.settings.LdapTimeout = connectionSettings.LdapTimeout;
            this.settings.LdapHost = connectionSettings.LdapHosts.EmptyStringArrayIfNull();
            this.settings.LdapPort = connectionSettings.LdapPort;
            this.settings.RequireCert = connectionSettings.RequireCert;
            this.settings.DNSCheck = connectionSettings.DNSCheck;
            this.settings.ServerCertFile = connectionSettings.ServerCertFile.EmptyStringIfNull();
            this.settings.BindDN = connectionSettings.BindDN.EmptyStringIfNull();
            this.settings.UseWindowsStoreBind = connectionSettings.UseWindowsStoreBind;
            this.settings.UseWindowsStoreConnection = connectionSettings.UseWindowsStoreConnection;
            this.settings.CertSubjectBind = connectionSettings.CertSubjectBind;
            this.settings.SetEncryptedSetting("BindPW", connectionSettings.BindPw.EmptyStringIfNull());
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

        public GatewaySettings GetGatewaySettings
        {
            get
            {
                string[] rules = this.settings.GatewayRules;
                return new GatewaySettings(rules);
            }
        }

        public void SetGatewaySettings(GatewaySettings gatewaySettings)
        {
            this.settings.GatewayRules = gatewaySettings.Rules.EmptyStringArrayIfNull();            
        }
    }
}
