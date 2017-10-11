namespace pGina.Plugin.TopicusKeyHub
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Windows.Forms;
    using System.Linq;
    using System.Text.RegularExpressions;
    using log4net;
    using LDAP;
    using LDAP.Model;
    using Newtonsoft.Json;
    using Settings;
    using Settings.ImportExport;
    using Settings.Model;

    public partial class Configuration : Form
    {
        private TopicusKeyHubSettings topicusKeyHubSettings;
        private readonly SettingsProvider settingsProvider = SettingsProvider.GetInstance(TopicusKeyHubPlugin.TopicusKeyHubUuid);
        private readonly ILog logger = LogManager.GetLogger("ConfigurationKeyHub");

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
            this.SetColumnHeaderGroups(this.lvGroupsNotSelected);
            this.SetColumnHeaderGroups(this.lvGroupsSelected);            
            this.cbDynamic.Checked = groupsettings.Dynamic;
            this.LoadGroups(true);
            this.lvGroupsNotSelected.Sorting = SortOrder.Ascending;
            this.lvGroupsSelected.Sorting = SortOrder.Ascending;

            // Gateway
        }

        private void UpdateSettings()
        {
            this.UpdateConnectionSettings();
            this.UpdateGroupSettings();
        }

        private void UpdateGroupSettings()
        {
            var selectedgroups = new List<string>();
            foreach (ListViewItem item in this.lvGroupsSelected.Items)
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
                if (timeout <= 0) throw new FormatException();
            }
            catch (FormatException)
            {
                MessageBox.Show("The timout be a positive integer > 0.");
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

        private void btnExport_Click(object sender, EventArgs e)
        {
            var json = JsonConvert.SerializeObject(this.topicusKeyHubSettings);
            var save = new SaveFileDialog
            {
                FileName = "TopicusKeyHubSettings.json",
                Filter = "JSON File | *.json"
            };
            if (save.ShowDialog() == DialogResult.OK)
            {
                var writer = new StreamWriter(save.OpenFile());
                writer.WriteLine(json);
                writer.Dispose();
                writer.Close();
            }
        }

        private void btnImport_Click(object sender, EventArgs e)
        {
            var openFileDialog1 = new OpenFileDialog {Filter = "JSON File | *.json"};
            var settingsstring = "";
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                System.IO.StreamReader sr = new
                    System.IO.StreamReader(openFileDialog1.FileName);
                settingsstring = sr.ReadToEnd();
                sr.Close();
                JsonSerializerSettings settings = new JsonSerializerSettings();
                settings.Converters.Add(new TopicusKeyHubSettingsConverter());
                var json = JsonConvert.DeserializeObject<TopicusKeyHubSettings>(settingsstring, settings);
                this.topicusKeyHubSettings = json;
                this.InitSettings();
            }
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
                    ldap.Close();
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


        private void LoadGroups(bool init)
        {
            try
            {
                Cursor.Current = Cursors.WaitCursor;
                using (var ldap = new LdapServer(this.topicusKeyHubSettings.GetConnectionSettings))
                {
                    ldap.BindForSearch();
                    var contexts = ldap.GetTopNamingContexts();
                    var groups = ldap.GetGroups(contexts.Single(b => b.Dynamic == this.cbDynamic.Checked).DistributionName).OrderBy(c => c.CommonName);
                    var index = 0;
                    if (init)
                    {
                        this.lvGroupsNotSelected.Items.Clear();
                        this.lvGroupsSelected.Items.Clear();
                        // Add all groups in selection
                        var groupsettings = this.topicusKeyHubSettings.GetGroupSettings;
                        foreach (var keyHubGroup in groups.Where(b => groupsettings.Groups.Contains(b.DistinguishedName)))
                        {
                            this.logger.DebugFormat("init item {0}", keyHubGroup.CommonName);
                            var item = new ListViewItem(keyHubGroup.CommonName, index)
                            {
                                Name = keyHubGroup.DistinguishedName
                            };
                            this.lvGroupsSelected.Items.Add(item);
                            index++;
                        }
                    }
                    index = 0;
                    foreach (var keyHubGroup in groups)
                    {
                        // Only add groups with are not in al ListView yet.
                        bool add = !(this.GroupInListView(keyHubGroup, this.lvGroupsNotSelected) != null ||
                                     this.GroupInListView(keyHubGroup, this.lvGroupsSelected) != null);
                        if (add)
                        {
                            this.logger.DebugFormat("add item {0}", keyHubGroup.CommonName);
                            var item = new ListViewItem(keyHubGroup.CommonName, index)
                            {
                                Name = keyHubGroup.DistinguishedName
                            };
                            this.lvGroupsNotSelected.Items.Add(item);
                            index++;
                        }
                    }
                    this.RemoveNotExistingGroupsFromListView(groups, this.lvGroupsNotSelected);
                    this.RemoveNotExistingGroupsFromListView(groups, this.lvGroupsSelected);
                    ldap.Close();
                }
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
            this.LoadGroups(false);
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
            foreach (ListViewItem item in this.lvGroupsNotSelected.Items)
            {
                if (item.Selected)
                {                    
                    this.lvGroupsNotSelected.Items.Remove(item);
                    this.lvGroupsSelected.Items.Add(item);
                }
            }
        }

        private void btRemove_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in this.lvGroupsSelected.Items)
            {
                if (item.Selected)
                {
                    this.lvGroupsSelected.Items.Remove(item);
                    this.lvGroupsNotSelected.Items.Add(item);                    
                }
            }
        }

        private void cbDynamic_CheckedChanged(object sender, EventArgs e)
        {
            this.LoadGroups(false);
        }

        private void lvGroupsNotSelected_DoubleClicked(object sender, EventArgs e)
        {
            if (this.lvGroupsNotSelected.SelectedItems.Count == 1)
            {
                var item = this.lvGroupsNotSelected.SelectedItems[0];
                this.lvGroupsNotSelected.Items.Remove(item);
                this.lvGroupsSelected.Items.Add(item);                
            }            
        }

        private void lvGroupsSelected_DoubleClicked(object sender, EventArgs e)
        {
            if (this.lvGroupsSelected.SelectedItems.Count == 1)
            {
                var item = this.lvGroupsSelected.SelectedItems[0];
                this.lvGroupsSelected.Items.Remove(item);
                this.lvGroupsNotSelected.Items.Add(item);
            }
        }
    }
}
