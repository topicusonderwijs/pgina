namespace pGina.Plugin.TopicusKeyHub.LDAP.Model
{
    public class KeyHubNamingContexts
    {
        private readonly bool dynamic;
        private readonly string distributionName;

        public KeyHubNamingContexts(bool dynamic, string distributionName)
        {
            this.dynamic = dynamic;
            this.distributionName = distributionName;
        }

        public bool Dynamic {
            get { return this.dynamic; }
        }

        public string DistributionName
        {
            get { return this.distributionName; }
        }
    }
}
