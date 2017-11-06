using System;
using System.Diagnostics;
using System.Linq;
using log4net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using pGina.Shared.Interfaces;
using pGina.Shared.Types;
using System.Collections.Generic;

using pGina.Plugin.TopicusKeyHub.LDAP;
using pGina.Plugin.TopicusKeyHub.LDAP.Model;
using pGina.Plugin.TopicusKeyHub.Settings;
using pGina.Plugin.TopicusKeyHub.Settings.ImportExport;
using pGina.Plugin.TopicusKeyHub.Settings.Model;

namespace pGina.Plugin.TopicusKeyHub
{
    public class TopicusKeyHubPlugin : IStatefulPlugin, IPluginConfiguration, IPluginAuthentication, IPluginAuthorization, IPluginImportExport, IPluginAuthenticationGateway
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

        public BooleanResult AuthenticatedUserGateway(SessionProperties properties)
        {
            this.logger.Debug("LDAP Plugin Gateway");
            var addedGroups = new List<string>();

            var ldapServer = properties.GetTrackedSingle<LdapServer>();

            // If the server is unavailable, we stop here!
            if (ldapServer == null)
            {
                this.logger.ErrorFormat("AuthenticatedUserGateway: Internal error, LdapServer object not available.");
                return new BooleanResult
                {
                    Success = false,
                    Message = "Topicus KeyHub LDAP server not available"
                };
            }
            try
            {
                var userInfo = properties.GetTrackedSingle<UserInformation>();
                var context = properties.GetTrackedSingle<KeyHubNamingContexts>();
                if (context == null)
                {
                    this.logger.Debug("No KeyHubNamingContexts");
                    return new BooleanResult { Message = "No KeyHubNamingContexts", Success = false };
                }
                var user = properties.GetTrackedSingle<KeyHubUser>();
                if (user == null)
                {
                    this.logger.Debug("No User");
                    return new BooleanResult { Message = "No KeyHubUser", Success = false };
                }
                var groupsettings = properties.GetTrackedSingle<GroupSettings>();
                if (groupsettings == null)
                {
                    this.logger.Debug("No GroupSettings");
                    return new BooleanResult { Message = "No GroupSettings", Success = false };
                }
                var groups = ldapServer.GetGroups(context.DistributionName).ToList();
                var rules = this.settings.GetGatewaySettings.Rules;
                foreach (var rule in rules)
                {
                    var rulevalues = rule.Split('*');
                    // Group still exists and user is in the group.
                    if (groups.FirstOrDefault(b => b.DistinguishedName == rulevalues[0]) != null &&
                        ldapServer.UserIsInGroup(user, groups.FirstOrDefault(b => b.DistinguishedName == rulevalues[0])))
                    {

                        this.logger.InfoFormat("Adding user {0} to local group {1}, due to rule \"{2}\"",
                            userInfo.Username, rulevalues[1], rule);
                        addedGroups.Add(rulevalues[1]);
                        userInfo.AddGroup(new GroupInformation {Name = rulevalues[1]});
                    }
                }
            }
            catch (Exception e)
            {
                this.logger.ErrorFormat("Error during gateway: {0}", e);

                // Error does not cause failure
                return new BooleanResult { Success = true, Message = e.Message };
            }

            string message;
            if (addedGroups.Count > 0)
            {
                message = string.Format("Added to groups: {0}", string.Join(", ", addedGroups));
            }
            else
            {
                message = "No groups added.";
            }

            return new BooleanResult { Success = true, Message = message };
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

                // Remove domain name from user
                userInfo.Username = userInfo.Username.RemoveDomainFromUsername();

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

        public void BeginChain(SessionProperties props)
        {
            this.logger.Debug("BeginChain");
            try
            {
                var connectionSettings = SettingsProvider.GetInstance(TopicusKeyHubUuid).GetSettings().GetConnectionSettings;
                var ldapServer = new LdapServer(connectionSettings);
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
