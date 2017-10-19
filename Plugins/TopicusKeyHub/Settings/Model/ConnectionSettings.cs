namespace pGina.Plugin.TopicusKeyHub.Settings.Model
{
    public class ConnectionSettings
    {
        private readonly int ldapTimeout;
        private readonly bool requireCert;
        private readonly string serverCertFile;
        private readonly string[] ldapHosts;
        private readonly int ldapPort;
        private readonly string bindDN;
        private readonly string bindPW;
        private readonly bool dnsCheck;
        private readonly bool useWindowsStoreBind;
        private readonly bool useWindowsStoreConnection;
        private readonly string certSubjectBind;

        public ConnectionSettings(string[] ldapHosts, int ldapPort,int ldapTimeout, bool requireCert, string serverCertFile, string bindDN, string bindPw, string certSubjectBind, bool dnsCheck, bool useWindowsStoreBind, bool useWindowsStoreConnection)
        {
            this.ldapTimeout = ldapTimeout;
            this.requireCert = requireCert;
            this.serverCertFile = serverCertFile;
            this.ldapHosts = ldapHosts;
            this.ldapPort = ldapPort;
            this.bindDN = bindDN;
            this.bindPW = bindPw;
            this.dnsCheck = dnsCheck;
            this.useWindowsStoreBind = useWindowsStoreBind;
            this.certSubjectBind = certSubjectBind;
            this.useWindowsStoreConnection = useWindowsStoreConnection;
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

        public string BindDN
        {
            get { return this.bindDN; }
        }

        public string BindPw
        {
            get { return this.bindPW; }
        }

        public string CertSubjectBind
        {
            get { return this.certSubjectBind; }
        }

        public bool DNSCheck
        {
            get { return this.dnsCheck; }
        }

        public bool UseWindowsStoreBind
        {
            get { return this.useWindowsStoreBind; }
        }

        public bool UseWindowsStoreConnection
        {
            get { return this.useWindowsStoreConnection; }
        }
    }
}
