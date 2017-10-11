namespace pGina.Plugin.TopicusKeyHub.Settings.Model
{
    public class GroupSettings
    {
        private readonly bool dynamic;
        private readonly string[] groups;

        public GroupSettings(string[] groups, bool dynamic)
        {
            this.groups = groups;
            this.dynamic = dynamic;
        }

        public bool Dynamic
        {
            get { return this.dynamic; }
        }

        public string[] Groups
        {
            get { return this.groups; }
        }      
    }
}
