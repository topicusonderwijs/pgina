namespace pGina.Plugin.TopicusKeyHub
{
    partial class Configuration
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (this.components != null))
            {
                this.components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Configuration));
            this.btnOk = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.tcPluginSettings = new System.Windows.Forms.TabControl();
            this.tpLDAPServer = new System.Windows.Forms.TabPage();
            this.btnTestConnection = new System.Windows.Forms.Button();
            this.ldapServerGroupBox = new System.Windows.Forms.GroupBox();
            this.showPwCB = new System.Windows.Forms.CheckBox();
            this.timeoutTextBox = new System.Windows.Forms.TextBox();
            this.timeoutLabel = new System.Windows.Forms.Label();
            this.searchPassTextBox = new System.Windows.Forms.TextBox();
            this.sslCertFileBrowseButton = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.searchDnTextBox = new System.Windows.Forms.TextBox();
            this.sslCertFileTextBox = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.validateServerCertCheckBox = new System.Windows.Forms.CheckBox();
            this.ldapPortTextBox = new System.Windows.Forms.TextBox();
            this.ldapHostTextBox = new System.Windows.Forms.TextBox();
            this.ldapPortLabel = new System.Windows.Forms.Label();
            this.ldapHostDescriptionLabel = new System.Windows.Forms.Label();
            this.tpGroups = new System.Windows.Forms.TabPage();
            this.lblSelected = new System.Windows.Forms.Label();
            this.lblNotSelected = new System.Windows.Forms.Label();
            this.btLoadGroups = new System.Windows.Forms.Button();
            this.btRemove = new System.Windows.Forms.Button();
            this.btAdd = new System.Windows.Forms.Button();
            this.lvGroupsSelected = new System.Windows.Forms.ListView();
            this.lvGroupsNotSelected = new System.Windows.Forms.ListView();
            this.cbDynamic = new System.Windows.Forms.CheckBox();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.btnClose = new System.Windows.Forms.Button();
            this.tcPluginSettings.SuspendLayout();
            this.tpLDAPServer.SuspendLayout();
            this.ldapServerGroupBox.SuspendLayout();
            this.tpGroups.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnOk
            // 
            this.btnOk.Location = new System.Drawing.Point(471, 253);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(75, 23);
            this.btnOk.TabIndex = 4;
            this.btnOk.Text = "Save";
            this.btnOk.UseVisualStyleBackColor = true;
            this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(633, 253);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 3;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // tcPluginSettings
            // 
            this.tcPluginSettings.Controls.Add(this.tpLDAPServer);
            this.tcPluginSettings.Controls.Add(this.tpGroups);
            this.tcPluginSettings.Controls.Add(this.tabPage3);
            this.tcPluginSettings.Location = new System.Drawing.Point(12, 12);
            this.tcPluginSettings.Name = "tcPluginSettings";
            this.tcPluginSettings.SelectedIndex = 0;
            this.tcPluginSettings.Size = new System.Drawing.Size(698, 235);
            this.tcPluginSettings.TabIndex = 9;
            // 
            // tpLDAPServer
            // 
            this.tpLDAPServer.Controls.Add(this.btnTestConnection);
            this.tpLDAPServer.Controls.Add(this.ldapServerGroupBox);
            this.tpLDAPServer.Location = new System.Drawing.Point(4, 22);
            this.tpLDAPServer.Name = "tpLDAPServer";
            this.tpLDAPServer.Padding = new System.Windows.Forms.Padding(3);
            this.tpLDAPServer.Size = new System.Drawing.Size(690, 209);
            this.tpLDAPServer.TabIndex = 0;
            this.tpLDAPServer.Text = "LDAP Server";
            this.tpLDAPServer.UseVisualStyleBackColor = true;
            // 
            // btnTestConnection
            // 
            this.btnTestConnection.Location = new System.Drawing.Point(607, 163);
            this.btnTestConnection.Name = "btnTestConnection";
            this.btnTestConnection.Size = new System.Drawing.Size(75, 23);
            this.btnTestConnection.TabIndex = 11;
            this.btnTestConnection.Text = "Test";
            this.btnTestConnection.UseVisualStyleBackColor = true;
            this.btnTestConnection.Click += new System.EventHandler(this.btnTestConnection_Click);
            // 
            // ldapServerGroupBox
            // 
            this.ldapServerGroupBox.Controls.Add(this.showPwCB);
            this.ldapServerGroupBox.Controls.Add(this.timeoutTextBox);
            this.ldapServerGroupBox.Controls.Add(this.timeoutLabel);
            this.ldapServerGroupBox.Controls.Add(this.searchPassTextBox);
            this.ldapServerGroupBox.Controls.Add(this.sslCertFileBrowseButton);
            this.ldapServerGroupBox.Controls.Add(this.label3);
            this.ldapServerGroupBox.Controls.Add(this.searchDnTextBox);
            this.ldapServerGroupBox.Controls.Add(this.sslCertFileTextBox);
            this.ldapServerGroupBox.Controls.Add(this.label2);
            this.ldapServerGroupBox.Controls.Add(this.label1);
            this.ldapServerGroupBox.Controls.Add(this.validateServerCertCheckBox);
            this.ldapServerGroupBox.Controls.Add(this.ldapPortTextBox);
            this.ldapServerGroupBox.Controls.Add(this.ldapHostTextBox);
            this.ldapServerGroupBox.Controls.Add(this.ldapPortLabel);
            this.ldapServerGroupBox.Controls.Add(this.ldapHostDescriptionLabel);
            this.ldapServerGroupBox.Location = new System.Drawing.Point(6, 6);
            this.ldapServerGroupBox.Name = "ldapServerGroupBox";
            this.ldapServerGroupBox.Size = new System.Drawing.Size(676, 151);
            this.ldapServerGroupBox.TabIndex = 8;
            this.ldapServerGroupBox.TabStop = false;
            this.ldapServerGroupBox.Text = "LDAP Server";
            // 
            // showPwCB
            // 
            this.showPwCB.AutoSize = true;
            this.showPwCB.Location = new System.Drawing.Point(578, 128);
            this.showPwCB.Name = "showPwCB";
            this.showPwCB.Size = new System.Drawing.Size(77, 17);
            this.showPwCB.TabIndex = 17;
            this.showPwCB.Text = "Show Text";
            this.showPwCB.UseVisualStyleBackColor = true;
            this.showPwCB.CheckedChanged += new System.EventHandler(this.showPwCB_CheckedChanged);
            // 
            // timeoutTextBox
            // 
            this.timeoutTextBox.Location = new System.Drawing.Point(265, 45);
            this.timeoutTextBox.Name = "timeoutTextBox";
            this.timeoutTextBox.Size = new System.Drawing.Size(76, 20);
            this.timeoutTextBox.TabIndex = 5;
            // 
            // timeoutLabel
            // 
            this.timeoutLabel.AutoSize = true;
            this.timeoutLabel.Location = new System.Drawing.Point(206, 48);
            this.timeoutLabel.Name = "timeoutLabel";
            this.timeoutLabel.Size = new System.Drawing.Size(45, 13);
            this.timeoutLabel.TabIndex = 4;
            this.timeoutLabel.Text = "Timeout";
            // 
            // searchPassTextBox
            // 
            this.searchPassTextBox.Location = new System.Drawing.Point(110, 123);
            this.searchPassTextBox.Name = "searchPassTextBox";
            this.searchPassTextBox.Size = new System.Drawing.Size(462, 20);
            this.searchPassTextBox.TabIndex = 16;
            this.searchPassTextBox.UseSystemPasswordChar = true;
            // 
            // sslCertFileBrowseButton
            // 
            this.sslCertFileBrowseButton.Location = new System.Drawing.Point(578, 71);
            this.sslCertFileBrowseButton.Name = "sslCertFileBrowseButton";
            this.sslCertFileBrowseButton.Size = new System.Drawing.Size(80, 20);
            this.sslCertFileBrowseButton.TabIndex = 11;
            this.sslCertFileBrowseButton.Text = "Browse...";
            this.sslCertFileBrowseButton.UseVisualStyleBackColor = true;
            this.sslCertFileBrowseButton.Click += new System.EventHandler(this.sslCertFileBrowseButton_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 126);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(90, 13);
            this.label3.TabIndex = 15;
            this.label3.Text = "Search Password";
            // 
            // searchDnTextBox
            // 
            this.searchDnTextBox.Location = new System.Drawing.Point(110, 97);
            this.searchDnTextBox.Name = "searchDnTextBox";
            this.searchDnTextBox.Size = new System.Drawing.Size(462, 20);
            this.searchDnTextBox.TabIndex = 13;
            // 
            // sslCertFileTextBox
            // 
            this.sslCertFileTextBox.Location = new System.Drawing.Point(109, 71);
            this.sslCertFileTextBox.Name = "sslCertFileTextBox";
            this.sslCertFileTextBox.Size = new System.Drawing.Size(463, 20);
            this.sslCertFileTextBox.TabIndex = 10;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 100);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(60, 13);
            this.label2.TabIndex = 12;
            this.label2.Text = "Search DN";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 74);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(54, 13);
            this.label1.TabIndex = 9;
            this.label1.Text = "Certificate";
            // 
            // validateServerCertCheckBox
            // 
            this.validateServerCertCheckBox.AutoSize = true;
            this.validateServerCertCheckBox.Location = new System.Drawing.Point(347, 47);
            this.validateServerCertCheckBox.Name = "validateServerCertCheckBox";
            this.validateServerCertCheckBox.Size = new System.Drawing.Size(148, 17);
            this.validateServerCertCheckBox.TabIndex = 8;
            this.validateServerCertCheckBox.Text = "Validate Server Certificate";
            this.validateServerCertCheckBox.UseVisualStyleBackColor = true;
            // 
            // ldapPortTextBox
            // 
            this.ldapPortTextBox.Location = new System.Drawing.Point(110, 45);
            this.ldapPortTextBox.Name = "ldapPortTextBox";
            this.ldapPortTextBox.Size = new System.Drawing.Size(70, 20);
            this.ldapPortTextBox.TabIndex = 3;
            // 
            // ldapHostTextBox
            // 
            this.ldapHostTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ldapHostTextBox.Location = new System.Drawing.Point(110, 19);
            this.ldapHostTextBox.Name = "ldapHostTextBox";
            this.ldapHostTextBox.Size = new System.Drawing.Size(462, 20);
            this.ldapHostTextBox.TabIndex = 1;
            // 
            // ldapPortLabel
            // 
            this.ldapPortLabel.AutoSize = true;
            this.ldapPortLabel.Location = new System.Drawing.Point(6, 48);
            this.ldapPortLabel.Name = "ldapPortLabel";
            this.ldapPortLabel.Size = new System.Drawing.Size(57, 13);
            this.ldapPortLabel.TabIndex = 2;
            this.ldapPortLabel.Text = "LDAP Port";
            // 
            // ldapHostDescriptionLabel
            // 
            this.ldapHostDescriptionLabel.AutoSize = true;
            this.ldapHostDescriptionLabel.Location = new System.Drawing.Point(6, 22);
            this.ldapHostDescriptionLabel.Name = "ldapHostDescriptionLabel";
            this.ldapHostDescriptionLabel.Size = new System.Drawing.Size(71, 13);
            this.ldapHostDescriptionLabel.TabIndex = 0;
            this.ldapHostDescriptionLabel.Text = "LDAP Host(s)";
            // 
            // tpGroups
            // 
            this.tpGroups.Controls.Add(this.lblSelected);
            this.tpGroups.Controls.Add(this.lblNotSelected);
            this.tpGroups.Controls.Add(this.btLoadGroups);
            this.tpGroups.Controls.Add(this.btRemove);
            this.tpGroups.Controls.Add(this.btAdd);
            this.tpGroups.Controls.Add(this.lvGroupsSelected);
            this.tpGroups.Controls.Add(this.lvGroupsNotSelected);
            this.tpGroups.Controls.Add(this.cbDynamic);
            this.tpGroups.Location = new System.Drawing.Point(4, 22);
            this.tpGroups.Name = "tpGroups";
            this.tpGroups.Padding = new System.Windows.Forms.Padding(3);
            this.tpGroups.Size = new System.Drawing.Size(690, 209);
            this.tpGroups.TabIndex = 1;
            this.tpGroups.Text = "Groups";
            this.tpGroups.UseVisualStyleBackColor = true;
            // 
            // lblSelected
            // 
            this.lblSelected.AutoSize = true;
            this.lblSelected.Location = new System.Drawing.Point(441, 12);
            this.lblSelected.Name = "lblSelected";
            this.lblSelected.Size = new System.Drawing.Size(49, 13);
            this.lblSelected.TabIndex = 7;
            this.lblSelected.Text = "Selected";
            // 
            // lblNotSelected
            // 
            this.lblNotSelected.AutoSize = true;
            this.lblNotSelected.Location = new System.Drawing.Point(165, 12);
            this.lblNotSelected.Name = "lblNotSelected";
            this.lblNotSelected.Size = new System.Drawing.Size(69, 13);
            this.lblNotSelected.TabIndex = 6;
            this.lblNotSelected.Text = "Not Selected";
            // 
            // btLoadGroups
            // 
            this.btLoadGroups.Location = new System.Drawing.Point(30, 180);
            this.btLoadGroups.Name = "btLoadGroups";
            this.btLoadGroups.Size = new System.Drawing.Size(96, 23);
            this.btLoadGroups.TabIndex = 5;
            this.btLoadGroups.Text = "Load Groups";
            this.btLoadGroups.UseVisualStyleBackColor = true;
            this.btLoadGroups.Click += new System.EventHandler(this.btLoadGroups_Click);
            // 
            // btRemove
            // 
            this.btRemove.Location = new System.Drawing.Point(411, 115);
            this.btRemove.Name = "btRemove";
            this.btRemove.Size = new System.Drawing.Size(27, 23);
            this.btRemove.TabIndex = 4;
            this.btRemove.Text = "<-";
            this.btRemove.UseVisualStyleBackColor = true;
            this.btRemove.Click += new System.EventHandler(this.btRemove_Click);
            // 
            // btAdd
            // 
            this.btAdd.Location = new System.Drawing.Point(411, 86);
            this.btAdd.Name = "btAdd";
            this.btAdd.Size = new System.Drawing.Size(27, 23);
            this.btAdd.TabIndex = 3;
            this.btAdd.Text = "->";
            this.btAdd.UseVisualStyleBackColor = true;
            this.btAdd.Click += new System.EventHandler(this.btAdd_Click);
            // 
            // lvGroupsSelected
            // 
            this.lvGroupsSelected.Location = new System.Drawing.Point(444, 31);
            this.lvGroupsSelected.Name = "lvGroupsSelected";
            this.lvGroupsSelected.Size = new System.Drawing.Size(240, 172);
            this.lvGroupsSelected.TabIndex = 2;
            this.lvGroupsSelected.UseCompatibleStateImageBehavior = false;
            this.lvGroupsSelected.DoubleClick += new System.EventHandler(this.lvGroupsSelected_DoubleClicked);
            // 
            // lvGroupsNotSelected
            // 
            this.lvGroupsNotSelected.Location = new System.Drawing.Point(165, 31);
            this.lvGroupsNotSelected.Name = "lvGroupsNotSelected";
            this.lvGroupsNotSelected.Size = new System.Drawing.Size(240, 172);
            this.lvGroupsNotSelected.TabIndex = 1;
            this.lvGroupsNotSelected.UseCompatibleStateImageBehavior = false;
            this.lvGroupsNotSelected.DoubleClick += new System.EventHandler(this.lvGroupsNotSelected_DoubleClicked);
            // 
            // cbDynamic
            // 
            this.cbDynamic.AutoSize = true;
            this.cbDynamic.Location = new System.Drawing.Point(7, 7);
            this.cbDynamic.Name = "cbDynamic";
            this.cbDynamic.Size = new System.Drawing.Size(65, 17);
            this.cbDynamic.TabIndex = 0;
            this.cbDynamic.Text = "dynamic";
            this.cbDynamic.UseVisualStyleBackColor = true;
            this.cbDynamic.CheckedChanged += new System.EventHandler(this.cbDynamic_CheckedChanged);
            // 
            // tabPage3
            // 
            this.tabPage3.Location = new System.Drawing.Point(4, 22);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage3.Size = new System.Drawing.Size(690, 209);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "tabPage3";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // btnClose
            // 
            this.btnClose.Location = new System.Drawing.Point(552, 253);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(75, 23);
            this.btnClose.TabIndex = 10;
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // Configuration
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(733, 299);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.tcPluginSettings);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOk);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Configuration";
            this.Text = "Topicus KeyHub Plugin Configuration";
            this.tcPluginSettings.ResumeLayout(false);
            this.tpLDAPServer.ResumeLayout(false);
            this.ldapServerGroupBox.ResumeLayout(false);
            this.ldapServerGroupBox.PerformLayout();
            this.tpGroups.ResumeLayout(false);
            this.tpGroups.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Button btnOk;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.TabControl tcPluginSettings;
        private System.Windows.Forms.TabPage tpLDAPServer;
        private System.Windows.Forms.TabPage tpGroups;
        private System.Windows.Forms.GroupBox ldapServerGroupBox;
        private System.Windows.Forms.CheckBox showPwCB;
        private System.Windows.Forms.TextBox timeoutTextBox;
        private System.Windows.Forms.Label timeoutLabel;
        private System.Windows.Forms.TextBox searchPassTextBox;
        private System.Windows.Forms.Button sslCertFileBrowseButton;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox searchDnTextBox;
        private System.Windows.Forms.TextBox sslCertFileTextBox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox validateServerCertCheckBox;
        private System.Windows.Forms.TextBox ldapPortTextBox;
        private System.Windows.Forms.TextBox ldapHostTextBox;
        private System.Windows.Forms.Label ldapPortLabel;
        private System.Windows.Forms.Label ldapHostDescriptionLabel;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Button btnTestConnection;
        private System.Windows.Forms.Button btRemove;
        private System.Windows.Forms.Button btAdd;
        private System.Windows.Forms.ListView lvGroupsSelected;
        private System.Windows.Forms.ListView lvGroupsNotSelected;
        private System.Windows.Forms.CheckBox cbDynamic;
        private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.Button btLoadGroups;
        private System.Windows.Forms.Label lblSelected;
        private System.Windows.Forms.Label lblNotSelected;
    }
}