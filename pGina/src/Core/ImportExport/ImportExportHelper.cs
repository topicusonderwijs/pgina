using log4net;
using pGina.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace pGina.Core.ImportExport
{
    public class ImportExportHelper
    {
        private static ILog m_logger = LogManager.GetLogger("ImportExportHelper");

        public static bool SetImportExportSettings(ImportExportSettings importExportSettings)
        {
            var importerror = false;
            var allplugins = PluginLoader.AllPlugins;
            if (importExportSettings.GeneralSettings != null)
            {
                if (File.Exists(importExportSettings.GeneralSettings.TileImage))
                {
                    Settings.Get.TileImage = importExportSettings.GeneralSettings.TileImage;
                }

                if (importExportSettings.GeneralSettings.MOTDSettings != null)
                {
                    // MOTD stuff
                    Settings.Get.EnableMotd = importExportSettings.GeneralSettings.MOTDSettings.Enable;
                    Settings.Get.Motd = importExportSettings.GeneralSettings.MOTDSettings.Text;
                }

                // Service status checkbox
                Settings.Get.ShowServiceStatusInLogonUi = importExportSettings.GeneralSettings.ShowServiceStatusInLogonUi;

                // Save unlock setting
                Settings.Get.UseOriginalUsernameInUnlockScenario = importExportSettings.GeneralSettings.UseOriginalUsernameInUnlockScenario;

                // Display last username in logon screen
                Settings.Get.LastUsernameEnable = importExportSettings.GeneralSettings.LastUsernameEnable;

                Settings.Get.PreferLocalAuthentication = importExportSettings.GeneralSettings.PreferLocalAuthentication;

                // ntp server
                string[] ntpservers = importExportSettings.GeneralSettings.NTPServers.ToArray();                
                Settings.Get.ntpservers = ntpservers;

                if (importExportSettings.GeneralSettings.EmailNotificationSettings != null)
                {
                    // email notification
                    Settings.Get.notify_smtp = importExportSettings.GeneralSettings.EmailNotificationSettings.SMTP;
                    Settings.Get.notify_email = importExportSettings.GeneralSettings.EmailNotificationSettings.Email;
                    Settings.Get.notify_user = importExportSettings.GeneralSettings.EmailNotificationSettings.User;
                    Settings.Get.SetEncryptedSetting("notify_pass", importExportSettings.GeneralSettings.EmailNotificationSettings.Password);
                    Settings.Get.notify_cred = importExportSettings.GeneralSettings.EmailNotificationSettings.UseCredentials;
                    Settings.Get.notify_ssl = importExportSettings.GeneralSettings.EmailNotificationSettings.UseSSL;
                }
            }

            if (importExportSettings.DisabledCredentialProviders != null)
            {
                var credentialProviders = CredProvFilterConfig.LoadCredProvsAndFilterSettings();
                foreach (var credentialProvider in credentialProviders)
                {
                    var importCredentialProvider = importExportSettings.DisabledCredentialProviders.FirstOrDefault(b => b.Uuid.Equals(credentialProvider.Uuid));
                    if (importCredentialProvider != null)
                    {
                        credentialProvider.FilterChangePass = importCredentialProvider.FilterChangePass;
                        credentialProvider.FilterCredUI = importCredentialProvider.FilterCredUI;
                        credentialProvider.FilterLogon = importCredentialProvider.FilterLogon;
                        credentialProvider.FilterUnlock = importCredentialProvider.FilterUnlock;
                    }
                    else
                    {
                        credentialProvider.FilterChangePass = false;
                        credentialProvider.FilterCredUI = false;
                        credentialProvider.FilterLogon = false;
                        credentialProvider.FilterUnlock = false;
                    }
                }
                CredProvFilterConfig.SaveFilterSettings(credentialProviders);
            }

            foreach (var plugin in allplugins)
            {
                if (plugin is IPluginImportExport)
                {
                    try
                    {
                        // There are plugins settings
                        if (importExportSettings.PluginSettings != null)
                        {
                            var pluginsettings = importExportSettings.PluginSettings.FirstOrDefault(b => b.Uuid == plugin.Uuid);

                            if (pluginsettings != null)
                            {
                                PluginSettings.SetMask(plugin.Uuid, pluginsettings.AuthenticateEnabled, pluginsettings.AuthorizeEnabled,
                                    pluginsettings.GatewayEnabled, pluginsettings.NotificationEnabled, pluginsettings.ChangePasswordEnabled);
                                var importplugin = (IPluginImportExport)plugin;
                                importplugin.Import(pluginsettings.Settings);
                                m_logger.ErrorFormat("Import succeeded for plugin: {0}/{1}", plugin.Uuid, plugin.Name);
                            }
                            else if (importExportSettings.DisableNonExportedPlugins)
                            {
                                // Only enable exported plugins
                                PluginSettings.SetMask(plugin.Uuid, false, false, false, false, false);
                            }
                        }
                    }
                    catch
                    {
                        importerror = true;
                        m_logger.ErrorFormat("Import malfunction for plugin: {0}/{1}", plugin.Uuid, plugin.Name);
                    }
                }
            }

            SavePluginOrder(importExportSettings.AuthenticatePluginOrder, typeof(IPluginAuthentication));
            SavePluginOrder(importExportSettings.AuthorizePluginOrder, typeof(IPluginAuthorization));
            SavePluginOrder(importExportSettings.GatewayPluginOrder, typeof(IPluginAuthenticationGateway));
            SavePluginOrder(importExportSettings.NotificationPluginOrder, typeof(IPluginEventNotifications));
            SavePluginOrder(importExportSettings.ChangePasswordPluginOrder, typeof(IPluginChangePassword));

            return importerror;
        }

        private static void SavePluginOrder(List<Guid> listOfPluginUuid, Type type)
        {
            PluginSettings.SavePluginOrder(listOfPluginUuid, type);
        }

        public static ImportExportSettings GetImportExportSettings()
        {
            var exportsettings = new ImportExportSettings { PluginSettings = new List<ImportExportPluginSetting>(), DisableNonExportedPlugins = true };

            string[] ntpservers = Settings.Get.ntpservers;

            exportsettings.GeneralSettings = new ImportExportGeneralSettings
            {
                TileImage = Settings.Get.GetSetting("TileImage", null),
                MOTDSettings = new ImportExportMOTDSettings
                {
                    Enable = Settings.Get.EnableMotd,
                    Text = Settings.Get.GetSetting("Motd")
                },
                ShowServiceStatusInLogonUi = Settings.Get.ShowServiceStatusInLogonUi,
                UseOriginalUsernameInUnlockScenario = Settings.Get.UseOriginalUsernameInUnlockScenario,
                LastUsernameEnable = Settings.Get.LastUsernameEnable,
                PreferLocalAuthentication = Settings.Get.PreferLocalAuthentication,
                NTPServers = ntpservers.ToList(),
                EmailNotificationSettings = new ImportExportEmailNotificationSettings
                {
                    SMTP = Settings.Get.GetSetting("notify_smtp"),
                    Email = Settings.Get.GetSetting("notify_email"),
                    User = Settings.Get.GetSetting("notify_user"),
                    Password = Settings.Get.GetEncryptedSetting("notify_pass"),                    
                    UseCredentials = Settings.Get.notify_cred,
                    UseSSL = Settings.Get.notify_ssl
                }
            };

            exportsettings.DisabledCredentialProviders = new List<ImportExportDisabledCredentialProvider>();
            var credentialProviders = CredProvFilterConfig.LoadCredProvsAndFilterSettings();
            foreach (var credentialProvider in credentialProviders)
            {
                if (credentialProvider.FilterEnabled())
                {
                    exportsettings.DisabledCredentialProviders.Add(new ImportExportDisabledCredentialProvider {
                        Uuid = credentialProvider.Uuid,
                        Name = credentialProvider.Name,
                        FilterChangePass = credentialProvider.FilterChangePass,
                        FilterCredUI = credentialProvider.FilterCredUI,
                        FilterLogon = credentialProvider.FilterLogon,
                        FilterUnlock = credentialProvider.FilterUnlock });
                }
            }

            exportsettings.AuthenticatePluginOrder = PluginLoader.GetOrderedPluginsOfType<IPluginAuthentication>().Select(b => b.Uuid).ToList();
            exportsettings.AuthorizePluginOrder = PluginLoader.GetOrderedPluginsOfType<IPluginAuthorization>().Select(b => b.Uuid).ToList();
            exportsettings.GatewayPluginOrder = PluginLoader.GetOrderedPluginsOfType<IPluginAuthenticationGateway>().Select(b => b.Uuid).ToList();
            exportsettings.NotificationPluginOrder = PluginLoader.GetOrderedPluginsOfType<IPluginEventNotifications>().Select(b => b.Uuid).ToList();
            exportsettings.ChangePasswordPluginOrder = PluginLoader.GetOrderedPluginsOfType<IPluginChangePassword>().Select(b => b.Uuid).ToList();

            foreach (var plugin in PluginLoader.AllPlugins)
            {
                if (plugin is IPluginImportExport)
                {
                    try
                    {
                        var exportplugin = (IPluginImportExport)plugin;

                        int pluginMask = Settings.Get.GetSetting(exportplugin.Uuid.ToString());

                        var authenticateEnabled = PluginLoader.IsEnabledFor<IPluginAuthentication>(pluginMask);
                        var authorizeEnabled = PluginLoader.IsEnabledFor<IPluginAuthorization>(pluginMask);
                        var changePasswordEnabled = PluginLoader.IsEnabledFor<IPluginChangePassword>(pluginMask);
                        var gatewayEnabled = PluginLoader.IsEnabledFor<IPluginAuthenticationGateway>(pluginMask);
                        var notificationEnabled = PluginLoader.IsEnabledFor<IPluginEventNotifications>(pluginMask);

                        // Only export enabled plugin-settings
                        if (authenticateEnabled || authorizeEnabled || changePasswordEnabled || gatewayEnabled || notificationEnabled)
                        {
                            exportsettings.PluginSettings.Add(new ImportExportPluginSetting
                            {
                                Name = exportplugin.Name,
                                Uuid = exportplugin.Uuid,
                                Settings = exportplugin.Export(),
                                AuthenticateEnabled = authenticateEnabled,
                                AuthorizeEnabled = authorizeEnabled,
                                ChangePasswordEnabled = changePasswordEnabled,
                                GatewayEnabled = gatewayEnabled,
                                NotificationEnabled = notificationEnabled
                            });
                        }
                    }
                    catch
                    {
                        m_logger.ErrorFormat("Export malfunction for plugin: {0}/{1}", plugin.Uuid, plugin.Name);
                    }
                }
            }

            return exportsettings;
        }
    }
}
