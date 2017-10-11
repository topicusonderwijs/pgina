namespace pGina.Plugin.TopicusKeyHub.Settings
{
    using System;
    using Model;
    using Shared.Settings;

    internal class SettingsProvider
    {
        private static SettingsProvider _instance;
        private dynamic settings;

        private SettingsProvider(Guid settingsGuid)
        {
            this.settings = new pGinaDynamicSettings(settingsGuid);
            // ConnectionSettings Set default values for settings (if not already set)
            this.settings.SetDefault("LdapHost", new string[] { "ldap.keyhub.com" });
            this.settings.SetDefault("LdapPort", 389);
            this.settings.SetDefault("LdapTimeout", 10);
            this.settings.SetDefault("RequireCert", true);
            this.settings.SetDefault("ServerCertFile", "");
            this.settings.SetDefault("SearchDN", "");
            this.settings.SetDefaultEncryptedSetting("SearchPW", "");
            // Groups Set default values for settings (if not already set)
            this.settings.SetDefault("Groups", new string[] {});
            this.settings.SetDefault("Dynamic", true);
        }

        public static SettingsProvider GetInstance(Guid settingsGuid)
        {
            if (_instance == null)
            {
                _instance = new SettingsProvider(settingsGuid);
            }
            return _instance;
        }

        public TopicusKeyHubSettings GetSettings()
        {
            return TopicusKeyHubSettings.GetInstance(ref this.settings);
        }
    }
}
