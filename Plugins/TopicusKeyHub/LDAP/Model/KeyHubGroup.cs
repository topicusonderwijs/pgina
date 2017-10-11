namespace pGina.Plugin.TopicusKeyHub.LDAP.Model
{
    public class KeyHubGroup
    {
        private readonly string distinguishedName;
        private readonly string commonName;

        public KeyHubGroup(string distinguishedName, string commonName)
        {
            this.distinguishedName = distinguishedName;
            this.commonName = commonName;
        }

        public string DistinguishedName
        {
            get { return this.distinguishedName; }
        }

        public string CommonName
        {
            get { return this.commonName; }
        }
    }
}
