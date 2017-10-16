namespace pGina.Plugin.TopicusKeyHub.Settings.Model
{
    public class GatewaySettings
    {        
        private readonly string[] rules;

        public GatewaySettings(string[] rules)
        {
            this.rules = rules;            
        }

        public string[] Rules
        {
            get { return this.rules; }
        }
    }
}
