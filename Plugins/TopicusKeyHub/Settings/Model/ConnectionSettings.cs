namespace pGina.Plugin.TopicusKeyHub.Settings.Model
{
    public class ConnectionSettings
    {
        private readonly int ldapTimeout;
        private readonly bool requireCert;
        private readonly string serverCertFile;
        private readonly string[] ldapHosts;
        private readonly int ldapPort;
        private readonly string searchDN;
        private readonly string searchPW;
        private readonly bool dnsCheck;

        public ConnectionSettings(string[] ldapHosts, int ldapPort,int ldapTimeout, bool requireCert, string serverCertFile, string searchDn, string searchPw, bool dnsCheck)
        {
            this.ldapTimeout = ldapTimeout;
            this.requireCert = requireCert;
            this.serverCertFile = serverCertFile;
            this.ldapHosts = ldapHosts;
            this.ldapPort = ldapPort;
            this.searchDN = searchDn;
            this.searchPW = searchPw;
            this.dnsCheck = dnsCheck;
        }

        public int LdapTimeout {
            get { return this.ldapTimeout; }
        }

        public bool RequireCert
        {
            get { return this.requireCert; }
        }

        public string ServerCertFile
        {
            get { return this.serverCertFile; }
        }

        public string[] LdapHosts
        {
            get { return this.ldapHosts; }
        }

        public int LdapPort
        {
            get { return this.ldapPort; }
        }

        public string SearchDN
        {
            get { return this.searchDN; }
        }

        public string SearchPW
        {
            get { return this.searchPW; }
        }

        public bool DNSCheck { get { return this.dnsCheck; } }
    }
}
