namespace pGina.Plugin.TopicusKeyHub.LDAP.Model
{
    using System.Collections.Generic;

    public class KeyHubUserWithGroups
    {
        private IList<KeyHubGroup> keyHubGroups;

        private KeyHubUser keyHubUser;

        public KeyHubUserWithGroups(KeyHubUser keyHubUser)
        {
            this.keyHubUser = keyHubUser;
        }


        public KeyHubUser KeyHubUser
        {
            get { return this.keyHubUser; }
        }

        public void AddGroup(KeyHubGroup keyHubGroup)
        {
            if (keyHubGroup != null)
            {
                if (this.keyHubGroups == null)
                {
                    this.keyHubGroups = new List<KeyHubGroup>();
                }
                this.keyHubGroups.Add(keyHubGroup);
            }
        }
    }
}
