namespace pGina.Plugin.TopicusKeyHub
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Windows.Forms;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Threading;
    using log4net;
    using LDAP;
    using LDAP.Model;
    using Settings;
    using Settings.Model;

    public partial class Configuration : Form
    {
        private readonly TopicusKeyHubSettings topicusKeyHubSettings;

        private readonly SettingsProvider settingsProvider =
            SettingsProvider.GetInstance(TopicusKeyHubPlugin.TopicusKeyHubUuid);

        private readonly ILog logger = LogManager.GetLogger("ConfigurationKeyHub");
        private List<GatewayRule> gatewayrules = new List<GatewayRule>();
        private List<KeyHubGroup> keyHubGroups;

        public Configuration()
        {
            this.topicusKeyHubSettings = this.settingsProvider.GetSettings();
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
            this.sslCertFileTextBox.Text = connectionSettings.ServerCertFile;
            this.searchDnTextBox.Text = connectionSettings.SearchDN;
            this.searchPassTextBox.Text = connectionSettings.SearchPW;

            // Groups 
            var groupsettings = this.topicusKeyHubSettings.GetGroupSettings;
            this.SetColumnHeaderGroups(this.lvKeyHubGroupsNotSelected);
            this.SetColumnHeaderGroups(this.lvKeyHubGroupsSelected);
            this.cbDynamic.Checked = groupsettings.Dynamic;
            var loadGroupsBackground = new Thread(() =>
            {
                try
                {
                    this.LoadKeyHubGroupsTask(true);
                    // if groups are loaded, gateway can be showed
                    // Gateway
                    this.FillGatewayrules(this.topicusKeyHubSettings.GetGatewaySettings.Rules);
                    this.ReloadDisplayGatewayRules();
                }
                catch (Exception e)
                {
                    this.logger.InfoFormat("Load groups in background failt: {0}", e);
                }
            });
            loadGroupsBackground.Start();
            this.lvKeyHubGroupsNotSelected.Sorting = SortOrder.Ascending;
            this.lvKeyHubGroupsSelected.Sorting = SortOrder.Ascending;
            this.cbDynamic.CheckedChanged += this.cbDynamic_CheckedChanged;            
        }


        private void FillGatewayrules(string[] gatewayrulessettings)
        {
            this.gatewayrules = new List<GatewayRule>();
            if (this.keyHubGroups != null)
            {
                foreach (var gatewayrulessetting in gatewayrulessettings)
                {
                    var splitRule = gatewayrulessetting.Split('*');
                    var keyHubGroup = this.keyHubGroups.SingleOrDefault(b => b.CommonName == splitRule[0]);
                    this.gatewayrules.Add(new GatewayRule
                    {
                        LocalMachineGroupName = splitRule[1],
                        KeyHubGroupCommonName = splitRule[0],
                        KeyHubGroupDistinguishedName = keyHubGroup == null ? null : keyHubGroup.DistinguishedName,
                        ErrorRemove = keyHubGroup == null
                    });
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
                if (rule.ErrorRemove)
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
            foreach (ListView item in this.lvGatewayRules.Items)
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
            // LDAP server Connection settings
            this.topicusKeyHubSettings.SetConnectionSettings(new ConnectionSettings(
                Regex.Split(this.ldapHostTextBox.Text.Trim(), @"\s+"),
                int.Parse(this.ldapPortTextBox.Text),
                int.Parse(this.timeoutTextBox.Text),
                this.validateServerCertCheckBox.Checked,
                this.sslCertFileTextBox.Text,
                this.searchDnTextBox.Text,
                this.searchPassTextBox.Text));
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
                this.sslCertFileTextBox.Text.Trim().Length > 0 &&
                !(File.Exists(this.sslCertFileTextBox.Text.Trim())))
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
            this.searchPassTextBox.UseSystemPasswordChar = !this.showPwCB.Checked;
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
                this.sslCertFileTextBox.Text = fileName;
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
                    ldap.BindForSearch();
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

        private void LoadKeyHubGroupsTask(bool init)
        {
            Cursor.Current = Cursors.WaitCursor;
            using (var ldap = new LdapServer(this.topicusKeyHubSettings.GetConnectionSettings))
            {
                ldap.BindForSearch();
                var contexts = ldap.GetTopNamingContexts();
                var groups = ldap.GetGroups(contexts.Single(b => b.Dynamic == this.cbDynamic.Checked).DistributionName)
                    .OrderBy(c => c.CommonName);
                this.keyHubGroups = groups.ToList();
                var index = 0;
                if (init)
                {
                    this.cbKeyhubGroups.Items.Clear();

                    this.lvKeyHubGroupsNotSelected.Items.Clear();
                    this.lvKeyHubGroupsSelected.Items.Clear();
                    // Add all groups in selection Authorization
                    var groupsettings = this.topicusKeyHubSettings.GetGroupSettings;
                    foreach (var keyHubGroup in groups.Where(b => groupsettings.Groups.Contains(b.DistinguishedName)))
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

                foreach (var keyHubGroup in groups)
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

                        this.cbKeyhubGroups.Items.Add(new ComboBoxItem(keyHubGroup.CommonName,
                            keyHubGroup.DistinguishedName));
                    }
                }
                this.RemoveNotExistingGroupsFromListView(groups, this.lvKeyHubGroupsNotSelected);
                this.RemoveNotExistingGroupsFromListView(groups, this.lvKeyHubGroupsSelected);
            }
        }

        public class ComboBoxItem
        {
            public ComboBoxItem(string text, string value)
            {
                this.Text = text;
                this.Value = value;
            }

            public string Text { get; set; }
            public object Value { get; set; }

            public override string ToString()
            {
                return Text;
            }
        }


        private void LoadKeyHubGroups()
        {
            try
            {
                this.LoadKeyHubGroupsTask(false);
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

        private void btAdd_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in this.lvKeyHubGroupsNotSelected.Items)
            {
                if (item.Selected)
                {
                    this.lvKeyHubGroupsNotSelected.Items.Remove(item);
                    this.lvKeyHubGroupsSelected.Items.Add(item);
                }
            }
        }

        private void btRemove_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in this.lvKeyHubGroupsSelected.Items)
            {
                if (item.Selected)
                {
                    this.lvKeyHubGroupsSelected.Items.Remove(item);
                    this.lvKeyHubGroupsNotSelected.Items.Add(item);
                }
            }
        }

        private void cbDynamic_CheckedChanged(object sender, EventArgs e)
        {
            this.LoadKeyHubGroups();
        }

        private void lvGroupsNotSelected_DoubleClicked(object sender, EventArgs e)
        {
            if (this.lvKeyHubGroupsNotSelected.SelectedItems.Count == 1)
            {
                var item = this.lvKeyHubGroupsNotSelected.SelectedItems[0];
                this.lvKeyHubGroupsNotSelected.Items.Remove(item);
                this.lvKeyHubGroupsSelected.Items.Add(item);
            }
        }

        private void lvGroupsSelected_DoubleClicked(object sender, EventArgs e)
        {
            if (this.lvKeyHubGroupsSelected.SelectedItems.Count == 1)
            {
                var item = this.lvKeyHubGroupsSelected.SelectedItems[0];
                this.lvKeyHubGroupsSelected.Items.Remove(item);
                this.lvKeyHubGroupsNotSelected.Items.Add(item);
            }
        }

        private void lbLoadgroups_Click(object sender, EventArgs e)
        {
            this.LoadKeyHubGroups();
            this.LoadMachineGroups();
            this.ReloadDisplayGatewayRules();
        }

        private void LoadMachineGroups()
        {
            this.cbLocalMachineGroups.Items.Clear();
            var localMachineGroups = Abstractions.Windows.Group.GetLocalMachineGroups();
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
                this.gatewayrules.Add(new GatewayRule
                {
                    KeyHubGroupCommonName = keyHubgroep.Text,
                    KeyHubGroupDistinguishedName = (string)keyHubgroep.Value,
                    LocalMachineGroupName = (string)localgroup.Value
                });
            }
        }

        private class GatewayRule
        {
            public string KeyHubGroupDistinguishedName { get; set; }

            public string KeyHubGroupCommonName { get; set; }        

            public string LocalMachineGroupName { get; set; }

            public bool ErrorRemove { get; set; }
        }
    }
}
