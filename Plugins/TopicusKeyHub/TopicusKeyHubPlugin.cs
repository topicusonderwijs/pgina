namespace pGina.Plugin.TopicusKeyHub
{
    using System;
    using System.Diagnostics;
    using System.Linq;
    using log4net;
    using LDAP;
    using LDAP.Model;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using Settings;
    using Settings.ImportExport;
    using Settings.Model;
    using Shared.Interfaces;
    using Shared.Types;

    public class TopicusKeyHubPlugin : IStatefulPlugin, IPluginConfiguration, IPluginAuthentication, IPluginChangePassword, IPluginAuthorization, IPluginImportExport
    {
        private readonly ILog logger = LogManager.GetLogger("TopicusKeyHubPlugin");
        public static Guid TopicusKeyHubUuid = new Guid("{EF869D73-8C63-4A93-B952-B94E52BAFB13}");
        private readonly TopicusKeyHubSettings settings;

        public TopicusKeyHubPlugin()
        {
            using(var me = Process.GetCurrentProcess())
            {
                this.logger.DebugFormat("Plugin initialized on {0} in PID: {1} Session: {2}", Environment.MachineName, me.Id, me.SessionId);
                this.settings = SettingsProvider.GetInstance(TopicusKeyHubUuid).GetSettings();
            }
        }

        public string Name
        {
            get { return "Topicus KeyHub"; }
        }

        public string Description
        {
            get { return "Use Topicus KeyHub as a data source for authentication and/or group authorization."; }
        }

        public string Version
        {
            get
            {
                return System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
            }
        }

        public Guid Uuid
        {
            get { return TopicusKeyHubUuid; }
        }

        public BooleanResult AuthorizeUser(SessionProperties properties)
        {
            try
            {
                this.logger.DebugFormat("AuthorizeUser({0})", properties.Id.ToString());
                var context = properties.GetTrackedSingle<KeyHubNamingContexts>();
                if (context == null)
                {
                    this.logger.Debug("No KeyHubNamingContexts");
                    return new BooleanResult {Message = "No KeyHubNamingContexts", Success = false};
                }
                var user = properties.GetTrackedSingle<KeyHubUser>();
                if (user == null)
                {
                    this.logger.Debug("No User");
                    return new BooleanResult {Message = "No KeyHubUser", Success = false};
                }
                var groupsettings = properties.GetTrackedSingle<GroupSettings>();
                if (groupsettings == null)
                {
                    this.logger.Debug("No GroupSettings");
                    return new BooleanResult {Message = "No GroupSettings", Success = false};
                }

                var ldapServer = properties.GetTrackedSingle<LdapServer>();
                var groups = ldapServer.GetGroups(context.DistributionName);
                foreach (var keyHubGroup in groups.Where(b => groupsettings.Groups.Contains(b.DistinguishedName)))
                {
                    if (ldapServer.UserIsInGroup(user, keyHubGroup))
                    {
                        this.logger.DebugFormat("Match User {0} and Group {1}", user.CommonName, keyHubGroup.CommonName);
                        return new BooleanResult { Success = true };
                    }
                }
                return new BooleanResult { Message = "No Group match Found", Success = false };
            }
            catch (Exception e)
            {
                this.logger.ErrorFormat("AuthenticateUser exception: {0}", e);
                throw; // Allow pGina service to catch and handle exception
            }
        }

        BooleanResult IPluginAuthentication.AuthenticateUser(SessionProperties properties)
        {
            try
            {
                this.logger.DebugFormat("AuthenticateUser({0})", properties.Id.ToString());

                // Get user info
                var userInfo = properties.GetTrackedSingle<UserInformation>();
                var ldapServer = properties.GetTrackedSingle<LdapServer>();

                this.logger.DebugFormat("Found username: {0}", userInfo.Username);
                var groupsettings = this.settings.GetGroupSettings;
                var dynamic = groupsettings.Dynamic;                
                properties.AddTrackedSingle<GroupSettings>(groupsettings);
                var contexts = ldapServer.GetTopNamingContexts();
                var context = contexts.Single(b => b.Dynamic.Equals(dynamic));
                properties.AddTrackedSingle<KeyHubNamingContexts>(context);
                var user = ldapServer.GetUser(context.DistributionName, userInfo.Username);
                if (user != null)
                {
                    if (ldapServer.PasswordCheck(user, userInfo.Password))
                    {
                        userInfo.Fullname = user.CommonName;
                        userInfo.Email = user.Mail;
                        properties.AddTrackedSingle<KeyHubUser>(user);
                        this.logger.InfoFormat("Authenticated user: {0}", userInfo.Username);
                        return new BooleanResult { Success = true };
                    }
                    this.logger.ErrorFormat("Wrong passowrd: {0}", userInfo.Username);
                }
                else
                {
                    this.logger.ErrorFormat("Username does not exist: {0}", userInfo.Username);
                }
                this.logger.ErrorFormat("Failed to authenticate user: {0}", userInfo.Username);
                return new BooleanResult
                {
                    Success = false,
                    Message = string.Format("Failed to authenticate user: {0}", userInfo.Username)
                };
            }
            catch (Exception e)
            {
                this.logger.ErrorFormat("AuthenticateUser exception: {0}", e);
                throw;  // Allow pGina service to catch and handle exception
            }
        }

        public void Configure()
        {
            Configuration conf = new Configuration();
            conf.ShowDialog();
        }

        public void Starting() { }
        public void Stopping() { }
        public BooleanResult ChangePassword(SessionProperties props, ChangePasswordPluginActivityInfo pluginInfo)
        {
            return new BooleanResult
            {
                Success = false,
                Message = "Password change not possible"
            };
        }


        public void BeginChain(SessionProperties props)
        {
            this.logger.Debug("BeginChain");
            try
            {
                var connectionSettings = SettingsProvider.GetInstance(TopicusKeyHubUuid).GetSettings().GetConnectionSettings;
                var ldapServer = new LdapServer(connectionSettings);
                ldapServer.BindForSearch();
                props.AddTrackedSingle<LdapServer>(ldapServer);
            }
            catch (Exception e)
            {
                this.logger.ErrorFormat("Failed to create LdapServer: {0}", e);
                props.AddTrackedSingle<LdapServer>(null);
            }
        }

        public void EndChain(SessionProperties props)
        {
            this.logger.Debug("EndChain");
            var serv = props.GetTrackedSingle<LdapServer>();
            if (serv != null)
            {
                serv.Dispose();
            }
        }

        public void Import(JToken pluginSettings)
        {
            var jsonSerializerSettings = new JsonSerializerSettings();
            jsonSerializerSettings.Converters.Add(new TopicusKeyHubSettingsConverter());
            JsonConvert.DeserializeObject<TopicusKeyHubSettings>(pluginSettings.ToString(), jsonSerializerSettings);            
        }

        public JToken Export()
        {
            return JToken.FromObject(this.settings);
        }
    }
}
