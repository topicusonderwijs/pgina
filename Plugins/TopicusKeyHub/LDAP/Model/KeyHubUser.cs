namespace pGina.Plugin.TopicusKeyHub.LDAP.Model
{
    using System.Collections.Generic;

    public class KeyHubUser
    {
        private readonly string distinguishedName;
        private readonly string commonName;
        private readonly string displayName;
        private readonly IEnumerable<string> memberOflist;

        public KeyHubUser(string distinguishedName, string commonName, string displayName, IEnumerable<string> memberOflist)
        {
            this.distinguishedName = distinguishedName;
            this.commonName = commonName;
            this.displayName = displayName;
            this.memberOflist = memberOflist;
        }

        public string DistinguishedName
        {
            get { return this.distinguishedName; }
        }

        public string CommonName
        {
            get { return this.commonName; }
        }

        public string DisplayName
        {
            get { return this.displayName; }
        }

        public IEnumerable<string> MemberOflist
        {
            get { return this.memberOflist;  }
        }
    }
}
