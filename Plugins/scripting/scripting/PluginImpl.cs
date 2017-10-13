﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using pGina.Shared.Types;
using log4net;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using pGina.Shared;

namespace pGina.Plugin.scripting
{
    public class PluginImpl : pGina.Shared.Interfaces.IPluginAuthentication, pGina.Shared.Interfaces.IPluginAuthorization, pGina.Shared.Interfaces.IPluginAuthenticationGateway, pGina.Shared.Interfaces.IPluginChangePassword, pGina.Shared.Interfaces.IPluginEventNotifications, pGina.Shared.Interfaces.IPluginConfiguration, pGina.Shared.Interfaces.IPluginImportExport
    {
        private ILog m_logger = LogManager.GetLogger("scripting");

        #region Init-plugin
        public static Guid PluginUuid
        {
            get { return new Guid("{EA94CEBF-6ED1-454A-B333-81C4FAD61FA4}"); }
        }

        public PluginImpl()
        {
            using (Process me = Process.GetCurrentProcess())
            {
                m_logger.DebugFormat("Plugin initialized on {0} in PID: {1} Session: {2}", Environment.MachineName, me.Id, me.SessionId);
            }
        }

        public string Name
        {
            get { return "scripting"; }
        }

        public string Description
        {
            get { return "run scripts in various stages"; }
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
            get { return PluginUuid; }
        }
        #endregion

        public void Starting() { }
        public void Stopping() { }

        public void Configure()
        {
            Configuration myDialog = new Configuration();
            myDialog.ShowDialog();
        }

        internal Dictionary<string, Dictionary<bool, string>> GetSettings(UserInformation userInfo)
        {
            Dictionary<string, Dictionary<bool, string>> ret = new Dictionary<string, Dictionary<bool, string>>();
            Dictionary<bool, string> lines = new Dictionary<bool, string>();

            if (ParseSettings((string[])Settings.Store.authe_sys, ref lines))
                ret.Add("authe_sys", lines);
            if (!String.IsNullOrEmpty(userInfo.script_authe_sys))
                if (ParseSettings((string[])Settings.Store.authe_sys, ref lines))
                    ret.Add("authe_sys", lines);

            if (ParseSettings((string[])Settings.Store.autho_sys, ref lines))
                ret.Add("autho_sys", lines);
            if (!String.IsNullOrEmpty(userInfo.script_autho_sys))
                if (ParseSettings((string[])Settings.Store.autho_sys, ref lines))
                    ret.Add("autho_sys", lines);

            if (ParseSettings((string[])Settings.Store.gateway_sys, ref lines))
                ret.Add("gateway_sys", lines);
            if (!String.IsNullOrEmpty(userInfo.script_gateway_sys))
                if (ParseSettings((string[])Settings.Store.gateway_sys, ref lines))
                    ret.Add("gateway_sys", lines);

            if (ParseSettings((string[])Settings.Store.notification_sys, ref lines))
                ret.Add("notification_sys", lines);
            if (!String.IsNullOrEmpty(userInfo.script_notification_sys))
                if (ParseSettings((string[])Settings.Store.notification_sys, ref lines))
                    ret.Add("notification_sys", lines);
            if (ParseSettings((string[])Settings.Store.notification_usr, ref lines))
                ret.Add("notification_usr", lines);
            if (!String.IsNullOrEmpty(userInfo.script_notification_usr))
                if (ParseSettings((string[])Settings.Store.notification_usr, ref lines))
                    ret.Add("notification_usr", lines);

            if (ParseSettings((string[])Settings.Store.changepwd_sys, ref lines))
                ret.Add("changepwd_sys", lines);
            if (!String.IsNullOrEmpty(userInfo.script_changepwd_sys))
                if (ParseSettings((string[])Settings.Store.changepwd_sys, ref lines))
                    ret.Add("changepwd_sys", lines);
            if (ParseSettings((string[])Settings.Store.changepwd_usr, ref lines))
                ret.Add("changepwd_usr", lines);
            if (!String.IsNullOrEmpty(userInfo.script_changepwd_usr))
                if (ParseSettings((string[])Settings.Store.changepwd_usr, ref lines))
                    ret.Add("changepwd_usr", lines);

            return ret;
        }

        internal bool ParseSettings(string[] setting, ref Dictionary<bool, string> lines)
        {
            lines = new Dictionary<bool, string>();
            foreach (string line in setting)
            {
                if (Regex.IsMatch(line, "(?i)(True|False)\t.*"))
                {
                    string[] split = line.Split('\t');
                    if (split.Length == 2)
                    {
                        lines.Add(Convert.ToBoolean(split[0]), split[1]);
                    }
                }
            }

            return Convert.ToBoolean(lines.Count);
        }

        internal bool Run(int sessionId, string cmd, UserInformation userInfo, bool pwd, bool sys)
        {
            string expand_cmd = "";
            string expand_cmd_out = "";
            foreach (string c in cmd.Split(' '))
            {
                string ce = c;
                string cp = c;
                if (!Regex.IsMatch(c, @"(?i)%\S+%") && Regex.IsMatch(c, @"(?i)%\S+"))
                {
                    ce = ce.Replace("%u", userInfo.Username);
                    cp = cp.Replace("%u", userInfo.Username);
                    ce = ce.Replace("%o", userInfo.OriginalUsername);
                    cp = cp.Replace("%o", userInfo.OriginalUsername);
                    if (pwd)
                    {
                        ce = ce.Replace("%p", userInfo.Password);
                        cp = cp.Replace("%p", "*****");
                        ce = ce.Replace("%b", userInfo.oldPassword);
                        cp = cp.Replace("%b", "*****");
                    }
                    ce = ce.Replace("%s", userInfo.SID.Value);
                    cp = cp.Replace("%s", userInfo.SID.Value);
                    ce = ce.Replace("%e", userInfo.PasswordEXP.ToString());
                    cp = cp.Replace("%e", userInfo.PasswordEXP.ToString());
                    ce = ce.Replace("%i", userInfo.SessionID.ToString());
                    cp = cp.Replace("%i", userInfo.SessionID.ToString());
                    expand_cmd += ce + " ";
                    expand_cmd_out += cp + " ";
                }
                else
                {
                    expand_cmd += c + " ";
                    expand_cmd_out += cp + " ";
                }
            }
            expand_cmd = expand_cmd.Trim();
            expand_cmd_out = expand_cmd_out.Trim();
            m_logger.InfoFormat("execute {0}", expand_cmd_out);

            if (sys)
                return Abstractions.WindowsApi.pInvokes.StartProcessInSessionWait(sessionId, expand_cmd);
            else
                return Abstractions.WindowsApi.pInvokes.StartUserProcessInSessionWait(sessionId, expand_cmd);
        }

        public BooleanResult AuthenticateUser(SessionProperties properties)
        {
            UserInformation userInfo = properties.GetTrackedSingle<UserInformation>();
            Dictionary<string, Dictionary<bool, string>> settings = GetSettings(userInfo);
            Dictionary<bool, string> authe_sys = new Dictionary<bool, string>();
            try { authe_sys = settings["authe_sys"]; }
            catch { }

            foreach (KeyValuePair<bool, string> line in authe_sys)
            {
                if (!Run(userInfo.SessionID, line.Value, userInfo, line.Key, true))
                    return new BooleanResult { Success = false, Message = String.Format("failed to run:{0}", line.Value) };
            }

            // return false if no other plugin succeeded
            BooleanResult ret = new BooleanResult() { Success = false, Message = this.Name + " plugin can't authenticate a user on its own" };
            /*PluginActivityInformation pluginInfo = properties.GetTrackedSingle<PluginActivityInformation>();
            foreach (Guid uuid in pluginInfo.GetAuthenticationPlugins())
            {
                if (pluginInfo.GetAuthenticationResult(uuid).Success)
                {
                    return new BooleanResult() { Success = true };
                }
                else
                {
                    ret.Message = pluginInfo.GetAuthenticationResult(uuid).Message;
                }
            }*/

            return ret;
        }

        public BooleanResult AuthorizeUser(SessionProperties properties)
        {
            UserInformation userInfo = properties.GetTrackedSingle<UserInformation>();
            Dictionary<string, Dictionary<bool, string>> settings = GetSettings(userInfo);
            Dictionary<bool, string> autho_sys = new Dictionary<bool, string>();
            try { autho_sys = settings["autho_sys"]; }
            catch { }

            foreach (KeyValuePair<bool, string> line in autho_sys)
            {
                if (!Run(userInfo.SessionID, line.Value, userInfo, line.Key, true))
                    return new BooleanResult { Success = false, Message = String.Format("failed to run:{0}", line.Value) };
            }

            // return false if no other plugin succeeded
            BooleanResult ret = new BooleanResult() { Success = false, Message = this.Name + " plugin can't authorize a user on its own" };
            PluginActivityInformation pluginInfo = properties.GetTrackedSingle<PluginActivityInformation>();
            foreach (Guid uuid in pluginInfo.GetAuthorizationPlugins())
            {
                if (pluginInfo.GetAuthorizationResult(uuid).Success)
                {
                    return new BooleanResult() { Success = true };
                }
                else
                {
                    ret.Message = pluginInfo.GetAuthorizationResult(uuid).Message;
                }
            }

            return ret;
        }

        public BooleanResult AuthenticatedUserGateway(SessionProperties properties)
        {
            UserInformation userInfo = properties.GetTrackedSingle<UserInformation>();
            Dictionary<string, Dictionary<bool, string>> settings = GetSettings(userInfo);
            Dictionary<bool, string> gateway_sys = new Dictionary<bool, string>();
            try { gateway_sys = settings["gateway_sys"]; }
            catch { }

            foreach (KeyValuePair<bool, string> line in gateway_sys)
            {
                if (!Run(userInfo.SessionID, line.Value, userInfo, line.Key, true))
                    return new BooleanResult { Success = false, Message = String.Format("failed to run:{0}", line.Value) };
            }

            return new BooleanResult() { Success = true };
        }

        public void SessionChange(int SessionId, System.ServiceProcess.SessionChangeReason Reason, SessionProperties properties)
        {
            if (properties == null)
                return;

            if (Reason == System.ServiceProcess.SessionChangeReason.SessionLogon)
            {
                UserInformation userInfo = properties.GetTrackedSingle<UserInformation>();
                if (userInfo.Description.Contains("pGina created"))
                {
                    Dictionary<string, Dictionary<bool, string>> settings = GetSettings(userInfo);
                    Dictionary<bool, string> notification_sys = new Dictionary<bool, string>();
                    try { notification_sys = settings["notification_sys"]; }
                    catch { }
                    Dictionary<bool, string> notification_usr = new Dictionary<bool, string>();
                    try { notification_usr = settings["notification_usr"]; }
                    catch { }


                    foreach (KeyValuePair<bool, string> line in notification_sys)
                    {
                        if (!Run(userInfo.SessionID, line.Value, userInfo, line.Key, true))
                            m_logger.InfoFormat("failed to run:{0}", line.Value);
                    }

                    foreach (KeyValuePair<bool, string> line in notification_usr)
                    {
                        if (!Run(userInfo.SessionID, line.Value, userInfo, line.Key, false))
                            m_logger.InfoFormat("failed to run:{0}", line.Value);
                    }
                }
            }
        }

        public BooleanResult ChangePassword(SessionProperties properties, ChangePasswordPluginActivityInfo pluginInfo)
        {
            UserInformation userInfo = properties.GetTrackedSingle<UserInformation>();
            Dictionary<string, Dictionary<bool, string>> settings = GetSettings(userInfo);
            Dictionary<bool, string> changepwd_sys = new Dictionary<bool, string>();
            try { changepwd_sys = settings["changepwd_sys"]; }
            catch { }
            Dictionary<bool, string> changepwd_usr = new Dictionary<bool, string>();
            try { changepwd_usr = settings["changepwd_usr"]; }
            catch { }

            foreach (KeyValuePair<bool, string> line in changepwd_sys)
            {
                if (!Run(userInfo.SessionID, line.Value, userInfo, line.Key, true))
                    return new BooleanResult { Success = false, Message = String.Format("failed to run:{0}", line.Value) };
            }
            foreach (KeyValuePair<bool, string> line in changepwd_usr)
            {
                if (!Run(userInfo.SessionID, line.Value, userInfo, line.Key, false))
                    return new BooleanResult { Success = false, Message = String.Format("failed to run:{0}", line.Value) };
            }

            // the change password plugin chain will end as soon as one plugin failed
            // no special treatment needed
            return new BooleanResult { Success = true };
        }

        public void Import(JToken pluginSettings)
        {
            var importSettings = pluginSettings.ToObject<ImportExportSettings>();
            Settings.Store.authe_sys = importSettings.AutheSys.EmptyStringArrayIfNull();
            Settings.Store.autho_sys = importSettings.Authosys.EmptyStringArrayIfNull();
            Settings.Store.gateway_sys = importSettings.GatewaySys.EmptyStringArrayIfNull();
            Settings.Store.notification_sys = importSettings.NotificationSys.EmptyStringArrayIfNull();
            Settings.Store.notification_usr = importSettings.NotificationUsr.EmptyStringArrayIfNull();
            Settings.Store.changepwd_sys = importSettings.ChangepwdSys.EmptyStringArrayIfNull();
            Settings.Store.changepwd_usr = importSettings.ChangepwdUsr.EmptyStringArrayIfNull();
        }

        public JToken Export()
        {
            var exportsettings = new ImportExportSettings
            {
                AutheSys = Settings.Store.authe_sys,
                Authosys = Settings.Store.autho_sys,
                GatewaySys = Settings.Store.gateway_sys,
                NotificationSys = Settings.Store.notification_sys,
                NotificationUsr = Settings.Store.notification_usr,
                ChangepwdSys = Settings.Store.changepwd_sys,
                ChangepwdUsr = Settings.Store.changepwd_usr,
            };
            return JToken.FromObject(exportsettings);
        }
    }
}
