using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows.Forms;
using System.Linq;
using System.Text.RegularExpressions;
using log4net;

using pGina.Plugin.TopicusKeyHub.Settings.Model;
using pGina.Plugin.TopicusKeyHub.Settings;
using pGina.Plugin.TopicusKeyHub.LDAP.Model;
using pGina.Plugin.TopicusKeyHub.LDAP;

namespace pGina.Plugin.TopicusKeyHub
{
    public partial class Configuration : Form
    {
        private readonly TopicusKeyHubSettings topicusKeyHubSettings;
        private readonly GroupConfigurationHelper groupConfigurationHelper;
        private BackgroundWorker backgroundWorkerLoadGroups;

        private readonly SettingsProvider settingsProvider =
            SettingsProvider.GetInstance(TopicusKeyHubPlugin.TopicusKeyHubUuid);
        private readonly ILog logger = LogManager.GetLogger("ConfigurationKeyHub");
        private List<GroupConfigurationHelper.GatewayRule> gatewayrules = new List<GroupConfigurationHelper.GatewayRule>();
        private List<KeyHubGroup> keyHubGroups = new List<KeyHubGroup>();

        public Configuration()
        {
            this.topicusKeyHubSettings = this.settingsProvider.GetSettings();
            this.groupConfigurationHelper = new GroupConfigurationHelper();
            this.InitializeComponent();
            this.InitSettings();
        }

        private void InitSettings()
        {
            // LDAP server Connection settings
            var connectionSettings = this.topicusKeyHubSettings.GetConnectionSettings;

            var ldapHosts = connectionSettings.LdapHosts;
            string hosts = "";
            for (int i = 0; i < ldapHosts.Count(); i++)
            {
                string host = ldapHosts[i];
                if (i < ldapHosts.Count() - 1) hosts += host + " ";
                else hosts += host;
            }
            this.ldapHostTextBox.Text = hosts;
            this.ldapPortTextBox.Text = connectionSettings.LdapPort.ToString();
            this.timeoutTextBox.Text = connectionSettings.LdapTimeout.ToString();
            this.validateServerCertCheckBox.Checked = connectionSettings.RequireCert;
            this.validateServerDNSCheckBox.Checked = connectionSettings.DNSCheck;
            this.sslCertFileConnectionTextBox.Text = connectionSettings.ServerCertFile;
            this.tbBindDnTextBox.Text = connectionSettings.BindDN;
            this.tbBindPassTextBox.Text = connectionSettings.BindPw;
            this.certSubjectBindTextBox.Text = connectionSettings.CertSubjectBind;
            this.useWindowsStoreBindcheckBox.Checked = connectionSettings.UseWindowsStoreBind;
            this.useWindowsStoreConnectionCheckBox.Checked = connectionSettings.UseWindowsStoreConnection;
            this.useWindowsStoreBindcheckBox.CheckedChanged += this.useWindowsStoreBindcheckBox_CheckedChanged;
            this.useWindowsStoreConnectionCheckBox.CheckedChanged += this.useWindowsStoreConnectioncheckBox_CheckedChanged;
            this.SetDNSCheckCheckBox();
            this.SetsslCertFileTextBox();
            this.SetBindChoice();

            // Groups 
            var groupsettings = this.topicusKeyHubSettings.GetGroupSettings;
            this.SetColumnHeaderGroups(this.lvKeyHubGroupsNotSelected);
            this.SetColumnHeaderGroups(this.lvKeyHubGroupsSelected);
            this.cbDynamic.Checked = groupsettings.Dynamic;            
            this.lvKeyHubGroupsNotSelected.Sorting = SortOrder.Ascending;
            this.lvKeyHubGroupsSelected.Sorting = SortOrder.Ascending;
            this.cbDynamic.CheckedChanged += this.cbDynamic_CheckedChanged;

            // Gateway and Authorization Keyhubgroups background worker
            this.LoadMachineGroups();
            this.lvGatewayRules.View = View.Details;
            this.lvGatewayRules.Columns.Add(new ColumnHeader());
            this.lvGatewayRules.Columns[0].Text = "Gateway rules";
            this.lvGatewayRules.Columns[0].Width = 430;
            this.backgroundWorkerLoadGroups = new BackgroundWorker();
            this.backgroundWorkerLoadGroups.DoWork += (sender, args) =>
            {
                this.keyHubGroups = this.groupConfigurationHelper.GetKeyHubGroups(connectionSettings, groupsettings.Dynamic);
            };
            this.backgroundWorkerLoadGroups.RunWorkerCompleted += (sender, args)
                =>
            {                
                this.RefresfKeyhubGroups(true);
                // if groups are loaded, gateway can be showed
                // Gateway
                this.FillGatewayrules(this.topicusKeyHubSettings.GetGatewaySettings.Rules);
                this.ReloadDisplayGatewayRules();
            };
            this.backgroundWorkerLoadGroups.RunWorkerAsync();
        }


        private void FillGatewayrules(string[] gatewayrulessettings)
        {
            this.gatewayrules = new List<GroupConfigurationHelper.GatewayRule>();
            if (this.keyHubGroups != null)
            {
                foreach (var gatewayrulessetting in gatewayrulessettings)
                {
                    
                    var splitRule = GroupConfigurationHelper.GetGatewayRule(gatewayrulessetting, this.keyHubGroups);
                    this.gatewayrules.Add(splitRule);
                }
            }
        }

        private void ReloadDisplayGatewayRules()
        {
            this.lvGatewayRules.Items.Clear();
            foreach (var rule in this.gatewayrules.OrderBy(b => b.KeyHubGroupCommonName).ThenBy(b => b.LocalMachineGroupName))
            {
                var message = "";
                var name = string.Format("{0}*{1}", rule.KeyHubGroupDistinguishedName, rule.LocalMachineGroupName);
                if (string.IsNullOrEmpty(rule.KeyHubGroupDistinguishedName))
                {
                    message = " (a error occurred, rule will be delete)";
                    name = "delete";
                }
                this.lvGatewayRules.Items.Add(new ListViewItem
                {
                    Text = string.Format("{0} => {1}{2}", rule.KeyHubGroupCommonName, rule.LocalMachineGroupName, message),
                    Name = name
                });
            }
        }

        private void UpdateGatewaySettings()
        {
            var newgatewaysettings = new List<string>();
            foreach (ListViewItem item in this.lvGatewayRules.Items)
            {
                if (item.Name != "delete")
                {
                    newgatewaysettings.Add(item.Name);
                }
            }
            this.topicusKeyHubSettings.SetGatewaySettings(new GatewaySettings(newgatewaysettings.ToArray()));
        }

        private void UpdateSettings()
        {
            this.UpdateConnectionSettings();
            this.UpdateGroupSettings();
            this.UpdateGatewaySettings();
        }

        private void UpdateGroupSettings()
        {
            var selectedgroups = new List<string>();
            foreach (ListViewItem item in this.lvKeyHubGroupsSelected.Items)
            {
                selectedgroups.Add(item.Name);
            }
            this.topicusKeyHubSettings.SetGroupsSettings(new GroupSettings(selectedgroups.ToArray(),
                this.cbDynamic.Checked));
        }

        private void UpdateConnectionSettings()
        {            
            if (this.useWindowsStoreBindcheckBox.Checked)
            {
                // make username and password empty if WindowsStore is used
                this.tbBindPassTextBox.Text = string.Empty;
                this.tbBindDnTextBox.Text = string.Empty;
            }

            // LDAP server Connection settings
            this.topicusKeyHubSettings.SetConnectionSettings(new ConnectionSettings(
                Regex.Split(this.ldapHostTextBox.Text.Trim(), @"\s+"),
                int.Parse(this.ldapPortTextBox.Text),
                int.Parse(this.timeoutTextBox.Text),
                this.validateServerCertCheckBox.Checked,
                this.sslCertFileConnectionTextBox.Text,
                this.tbBindDnTextBox.Text,
                this.tbBindPassTextBox.Text,
                this.certSubjectBindTextBox.Text,
                this.validateServerDNSCheckBox.Checked,
                this.useWindowsStoreBindcheckBox.Checked,
                this.useWindowsStoreConnectionCheckBox.Checked));
        }

        private bool ValidateInput()
        {
            if (this.ldapHostTextBox.Text.Trim().Length == 0)
            {
                MessageBox.Show("Please provide at least one LDAP host.");
                return false;
            }

            try
            {
                int port = Convert.ToInt32(this.ldapPortTextBox.Text.Trim());
                if (port <= 0) throw new FormatException();
            }
            catch (FormatException)
            {
                MessageBox.Show("The LDAP port number must be a positive integer > 0.");
                return false;
            }

            try
            {
                int timeout = Convert.ToInt32(this.timeoutTextBox.Text.Trim());
                if (timeout <= 0 || timeout > 10) throw new FormatException();
            }
            catch (FormatException)
            {
                MessageBox.Show("The timout be a positive 10 > 0.");
                return false;
            }

            if (this.validateServerCertCheckBox.CheckState == CheckState.Checked &&
                this.sslCertFileConnectionTextBox.Text.Trim().Length > 0 &&
                !(File.Exists(this.sslCertFileConnectionTextBox.Text.Trim())))
            {
                MessageBox.Show("SSL certificate file does not exist." + "Please select a valid certificate file.");
                return false;
            }
            return true;
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            if (this.ValidateInput())
            {
                this.UpdateSettings();
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void showPwCB_CheckedChanged(object sender, EventArgs e)
        {
            this.tbBindPassTextBox.UseSystemPasswordChar = !this.showPwCB.Checked;
        }

        private void sslCertFileBrowseButton_Click(object sender, EventArgs e)
        {
            DialogResult result;
            string fileName;
            using (OpenFileDialog dlg = new OpenFileDialog())
            {
                result = dlg.ShowDialog();
                fileName = dlg.FileName;
            }

            if (result == DialogResult.OK)
            {
                this.sslCertFileConnectionTextBox.Text = fileName;
            }
        }

        private void btnTestConnection_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            try
            {
                this.UpdateConnectionSettings();
                using (var ldap = new LdapServer(this.topicusKeyHubSettings.GetConnectionSettings))
                {
                }
                MessageBox.Show("Connection OK");
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }

        private void SetColumnHeaderGroups(ListView listView)
        {
            listView.View = View.Details;
            listView.Columns.Add(new ColumnHeader());
            listView.Columns[0].Text = "Groups";
            listView.Columns[0].Width = 240;
        }

        private void RefresfKeyhubGroups(bool init)
        {
            var index = 0;
            if (init)
            {
                this.lvKeyHubGroupsNotSelected.Items.Clear();
                this.lvKeyHubGroupsSelected.Items.Clear();
                // Add all groups in selection Authorization
                var groupsettings = this.topicusKeyHubSettings.GetGroupSettings;
                foreach (var keyHubGroup in this.keyHubGroups.Where(b => groupsettings.Groups.Contains(b.DistinguishedName)))
                {
                    this.logger.DebugFormat("init item {0}", keyHubGroup.CommonName);
                    var item = new ListViewItem(keyHubGroup.CommonName, index)
                    {
                        Name = keyHubGroup.DistinguishedName
                    };
                    this.lvKeyHubGroupsSelected.Items.Add(item);
                    index++;
                }
            }

            index = 0;
            foreach (var keyHubGroup in this.keyHubGroups)
            {
                // Only add groups with are not in al ListView yet.
                bool add = !(this.GroupInListView(keyHubGroup, this.lvKeyHubGroupsNotSelected) != null ||
                             this.GroupInListView(keyHubGroup, this.lvKeyHubGroupsSelected) != null);
                if (add)
                {
                    this.logger.DebugFormat("add item {0}", keyHubGroup.CommonName);
                    var item = new ListViewItem(keyHubGroup.CommonName, index)
                    {
                        Name = keyHubGroup.DistinguishedName
                    };

                    this.lvKeyHubGroupsNotSelected.Items.Add(item);
                    index++;
                }

            }
            this.RemoveNotExistingGroupsFromListView(this.keyHubGroups, this.lvKeyHubGroupsNotSelected);
            this.RemoveNotExistingGroupsFromListView(this.keyHubGroups, this.lvKeyHubGroupsSelected);
            this.RefresfKeyhubGroupsGatewayRules();
        }

        public void RefresfKeyhubGroupsGatewayRules()
        {
            this.cbKeyhubGroups.Items.Clear();
            foreach (var keyHubGroup in this.keyHubGroups)
            {
                if (this.GroupInListView(keyHubGroup, this.lvKeyHubGroupsSelected) != null)
                {
                    // Only Authorized users in gateway selectbox.
                    this.cbKeyhubGroups.Items.Add(new ComboBoxItem(keyHubGroup.CommonName,
                        keyHubGroup.DistinguishedName));
                }
            }
            this.ReloadDisplayGatewayRules();
        }

        private void LoadKeyHubGroups()
        {
            Cursor.Current = Cursors.WaitCursor;
            try
            {
                this.keyHubGroups = this.groupConfigurationHelper.GetKeyHubGroups(this.topicusKeyHubSettings.GetConnectionSettings, this.cbDynamic.Checked);
                this.RefresfKeyhubGroups(false);
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }

        private void btLoadGroups_Click(object sender, EventArgs e)
        {
            this.LoadKeyHubGroups();
        }

        private void RemoveNotExistingGroupsFromListView(IEnumerable<KeyHubGroup> keyHubGroups, ListView listView)
        {
            foreach (ListViewItem item in listView.Items)
            {
                if (keyHubGroups.FirstOrDefault(b => b.CommonName == item.Text) == null)
                {
                    listView.Items.Remove(item);
                }
            }
        }

        private ListViewItem GroupInListView(KeyHubGroup keyHubGroup, ListView listView)
        {
            foreach (ListViewItem item in listView.Items)
            {
                if (item.Name.Equals(keyHubGroup.DistinguishedName))
                {
                    return item;
                }
            }
            return null;
        }

        private void cbDynamic_CheckedChanged(object sender, EventArgs e)
        {
            this.LoadKeyHubGroups();
            this.gatewayrules.Clear();
        }

        private void btAdd_Click(object sender, EventArgs e)
        {
            this.AddAuthorizedItems();
        }

        private void btRemove_Click(object sender, EventArgs e)
        {
            this.RemoveAuthorizedItems();
        }

        private void lvGroupsNotSelected_DoubleClicked(object sender, EventArgs e)
        {
            if (this.lvKeyHubGroupsNotSelected.SelectedItems.Count == 1)
            {
                this.AddAuthorizedItems();
            }
        }

        private void lvGroupsSelected_DoubleClicked(object sender, EventArgs e)
        {
            if (this.lvKeyHubGroupsSelected.SelectedItems.Count == 1)
            {
                this.RemoveAuthorizedItems();
            }
        }

        private void AddAuthorizedItems()
        {
            foreach (ListViewItem item in this.lvKeyHubGroupsNotSelected.Items)
            {
                if (item.Selected)
                {
                    this.lvKeyHubGroupsNotSelected.Items.Remove(item);
                    this.lvKeyHubGroupsSelected.Items.Add(item);
                }
            }
            this.RefresfKeyhubGroupsGatewayRules();
        }

        private void RemoveAuthorizedItems()
        {
            foreach (ListViewItem item in this.lvKeyHubGroupsSelected.Items)
            {
                if (item.Selected)
                {
                    this.lvKeyHubGroupsSelected.Items.Remove(item);
                    this.lvKeyHubGroupsNotSelected.Items.Add(item);
                }
            }
            this.RefresfKeyhubGroupsGatewayRules();
        }

        private void lbLoadgroups_Click(object sender, EventArgs e)
        {
            this.LoadKeyHubGroups();
            this.LoadMachineGroups();
        }

        private void LoadMachineGroups()
        {
            this.cbLocalMachineGroups.Items.Clear();
            var localMachineGroups = GroupConfigurationHelper.GetLocalMachineGroups();
            foreach (var localMachineGroup in localMachineGroups)
            {
                this.cbLocalMachineGroups.Items.Add(new ComboBoxItem(localMachineGroup, localMachineGroup));
            }
        }

        private void btAddGatewayRule_Click(object sender, EventArgs e)
        {
            var localgroup = (ComboBoxItem) this.cbLocalMachineGroups.SelectedItem;
            var keyHubgroep = (ComboBoxItem) this.cbKeyhubGroups.SelectedItem;
            if (localgroup != null && keyHubgroep != null)
            {
                var item = new GroupConfigurationHelper.GatewayRule
                {
                    KeyHubGroupCommonName = keyHubgroep.Text,
                    KeyHubGroupDistinguishedName = (string) keyHubgroep.Value,
                    LocalMachineGroupName = (string) localgroup.Value
                };
                if (this.gatewayrules.Any(b =>
                    b.LocalMachineGroupName == item.LocalMachineGroupName &&
                    b.KeyHubGroupDistinguishedName == item.KeyHubGroupDistinguishedName))
                {
                    MessageBox.Show("Gateway rule already exists");
                }
                else
                {
                    this.gatewayrules.Add(item);
                }
            }
            this.ReloadDisplayGatewayRules();
        }

        private void btDeleteGatewayRule_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem selectedItem in this.lvGatewayRules.SelectedItems)
            {
                var splitrule = GroupConfigurationHelper.GetGatewayRule(selectedItem.Name, this.keyHubGroups);
                var rule = this.gatewayrules.FirstOrDefault(b =>
                    b.KeyHubGroupDistinguishedName == splitrule.KeyHubGroupDistinguishedName && b.LocalMachineGroupName == splitrule.LocalMachineGroupName);
                if (rule != null)
                {
                    this.gatewayrules.Remove(rule);
                }
            }
            this.ReloadDisplayGatewayRules();
        }

        private void SetDNSCheckCheckBox()
        {
            this.validateServerDNSCheckBox.Enabled = this.validateServerCertCheckBox.Checked;
            if (!this.validateServerCertCheckBox.Checked)
            {
                this.validateServerDNSCheckBox.Checked = false;
            }
        }

        private void validateServerCertCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            this.SetDNSCheckCheckBox();
        }

        private void SetsslCertFileTextBox()
        {
            this.sslCertFileConnectionTextBox.Visible = !this.useWindowsStoreConnectionCheckBox.Checked;
            this.lblCertificateFileConnection.Visible = !this.useWindowsStoreConnectionCheckBox.Checked;  
            this.sslCertFileBrowseButton.Visible = !this.useWindowsStoreConnectionCheckBox.Checked;
        }

        private void SetBindChoice()
        {
            this.tbBindDnTextBox.Visible = !this.useWindowsStoreBindcheckBox.Checked;
            this.tbBindPassTextBox.Visible = !this.useWindowsStoreBindcheckBox.Checked;            
            this.lblBindDN.Visible = !this.useWindowsStoreBindcheckBox.Checked;
            this.lblBindPassword.Visible = !this.useWindowsStoreBindcheckBox.Checked;
            this.showPwCB.Visible = !this.useWindowsStoreBindcheckBox.Checked;

            this.lblBindCertificateStore.Visible = this.useWindowsStoreBindcheckBox.Checked;
        }

        private void useWindowsStoreConnectioncheckBox_CheckedChanged(object sender, EventArgs e)
        {
            this.SetsslCertFileTextBox();
        }

        private void useWindowsStoreBindcheckBox_CheckedChanged(object sender, EventArgs e)
        {
            this.SetBindChoice();
        }
    }
}
