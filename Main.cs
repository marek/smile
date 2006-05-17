using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using System.Threading;


namespace smiletray
{
	// Main Options Form
	public class frmMain : System.Windows.Forms.Form
	{
		private System.Windows.Forms.NotifyIcon notifyIcon;
		private System.Windows.Forms.ContextMenu contextMenu;
		private System.Windows.Forms.MenuItem menuItem1;
		private System.Windows.Forms.MenuItem menuItem2;
		private System.Windows.Forms.MenuItem menuItem3;
		private System.Windows.Forms.Timer TimerCheckKills;
		private System.ComponentModel.IContainer components;
		private System.Windows.Forms.MenuItem menuItem4;
		private System.Windows.Forms.CheckBox chkSnapEnabled;
		private System.Windows.Forms.CheckBox chkStatsEnabled;
		private System.Windows.Forms.CheckBox chkAutoDetectUser;
		private System.Windows.Forms.Label lblAccount;
		private System.Windows.Forms.GroupBox grpGlobalSettings;
		private System.Windows.Forms.GroupBox grpSnapSettings;
		private System.Windows.Forms.GroupBox grpStatsSettings;
		private System.Windows.Forms.Button cmdResetStats;
		private System.Windows.Forms.Button cmdViewStats;
		private System.Windows.Forms.NumericUpDown upDelay;
		private System.Windows.Forms.Label lblSnapDelay;
		private System.Windows.Forms.TextBox txtAccount;
		private System.Windows.Forms.MenuItem menuItem5;
		private System.Windows.Forms.MenuItem menuItem6;
		private System.Windows.Forms.MenuItem menuItem7;
		private System.Windows.Forms.MenuItem menuItem8;
		private System.Windows.Forms.Label lblSnapDir;
		private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
		private System.Windows.Forms.TextBox txtSnapDir;
		private System.Windows.Forms.Button cmdBrowseSnapDir;

		// private stuff for this form
		private CConsoleParser ConsoleParser;
		private Settings_t Settings;
		private bool EnableStats;	// Volitile State
		private bool EnableSnaps;	// Volitile State

		public frmMain()
		{
			// Required for Windows Form Designer support
			InitializeComponent();

			// Read Settings
			this.Settings = new Settings_t();


			// Init Console Parser
			this.ConsoleParser = new CConsoleParser();
			this.ConsoleParser.Open();
			
			// Init TimerCheckKills Counter  (rename to TimerCheckConsoleLog?)
			TimerCheckKills.Start();

			Hide();
		}

		/// Clean up any resources being used.
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if (components != null) 
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(frmMain));
			this.notifyIcon = new System.Windows.Forms.NotifyIcon(this.components);
			this.contextMenu = new System.Windows.Forms.ContextMenu();
			this.menuItem1 = new System.Windows.Forms.MenuItem();
			this.menuItem5 = new System.Windows.Forms.MenuItem();
			this.menuItem6 = new System.Windows.Forms.MenuItem();
			this.menuItem7 = new System.Windows.Forms.MenuItem();
			this.menuItem8 = new System.Windows.Forms.MenuItem();
			this.menuItem4 = new System.Windows.Forms.MenuItem();
			this.menuItem2 = new System.Windows.Forms.MenuItem();
			this.menuItem3 = new System.Windows.Forms.MenuItem();
			this.TimerCheckKills = new System.Windows.Forms.Timer(this.components);
			this.grpSnapSettings = new System.Windows.Forms.GroupBox();
			this.txtSnapDir = new System.Windows.Forms.TextBox();
			this.lblSnapDir = new System.Windows.Forms.Label();
			this.lblSnapDelay = new System.Windows.Forms.Label();
			this.upDelay = new System.Windows.Forms.NumericUpDown();
			this.chkSnapEnabled = new System.Windows.Forms.CheckBox();
			this.cmdBrowseSnapDir = new System.Windows.Forms.Button();
			this.grpStatsSettings = new System.Windows.Forms.GroupBox();
			this.cmdViewStats = new System.Windows.Forms.Button();
			this.cmdResetStats = new System.Windows.Forms.Button();
			this.chkStatsEnabled = new System.Windows.Forms.CheckBox();
			this.grpGlobalSettings = new System.Windows.Forms.GroupBox();
			this.lblAccount = new System.Windows.Forms.Label();
			this.txtAccount = new System.Windows.Forms.TextBox();
			this.chkAutoDetectUser = new System.Windows.Forms.CheckBox();
			this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
			this.grpSnapSettings.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.upDelay)).BeginInit();
			this.grpStatsSettings.SuspendLayout();
			this.grpGlobalSettings.SuspendLayout();
			this.SuspendLayout();
			// 
			// notifyIcon
			// 
			this.notifyIcon.ContextMenu = this.contextMenu;
			this.notifyIcon.Icon = ((System.Drawing.Icon)(resources.GetObject("notifyIcon.Icon")));
			this.notifyIcon.Text = "notifyIcon1";
			this.notifyIcon.Visible = true;
			// 
			// contextMenu
			// 
			this.contextMenu.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																						this.menuItem1,
																						this.menuItem5,
																						this.menuItem6,
																						this.menuItem7,
																						this.menuItem8,
																						this.menuItem4,
																						this.menuItem2,
																						this.menuItem3});
			// 
			// menuItem1
			// 
			this.menuItem1.Checked = true;
			this.menuItem1.Index = 0;
			this.menuItem1.Text = "Enable Snap";
			this.menuItem1.Click += new System.EventHandler(this.menuItem1_Click);
			// 
			// menuItem5
			// 
			this.menuItem5.Checked = true;
			this.menuItem5.Index = 1;
			this.menuItem5.Text = "Enable Stats";
			// 
			// menuItem6
			// 
			this.menuItem6.Index = 2;
			this.menuItem6.Text = "-";
			// 
			// menuItem7
			// 
			this.menuItem7.Index = 3;
			this.menuItem7.Text = "View Stats";
			// 
			// menuItem8
			// 
			this.menuItem8.Index = 4;
			this.menuItem8.Text = "Open Snap Folder";
			// 
			// menuItem4
			// 
			this.menuItem4.Index = 5;
			this.menuItem4.Text = "-";
			// 
			// menuItem2
			// 
			this.menuItem2.Index = 6;
			this.menuItem2.Text = "Open";
			this.menuItem2.Click += new System.EventHandler(this.menuItem2_Click);
			// 
			// menuItem3
			// 
			this.menuItem3.Index = 7;
			this.menuItem3.Text = "Exit";
			this.menuItem3.Click += new System.EventHandler(this.menuItem3_Click);
			// 
			// TimerCheckKills
			// 
			this.TimerCheckKills.Tick += new System.EventHandler(this.TimerCheckKills_Tick);
			// 
			// grpSnapSettings
			// 
			this.grpSnapSettings.Controls.Add(this.txtSnapDir);
			this.grpSnapSettings.Controls.Add(this.lblSnapDir);
			this.grpSnapSettings.Controls.Add(this.lblSnapDelay);
			this.grpSnapSettings.Controls.Add(this.upDelay);
			this.grpSnapSettings.Controls.Add(this.chkSnapEnabled);
			this.grpSnapSettings.Controls.Add(this.cmdBrowseSnapDir);
			this.grpSnapSettings.Location = new System.Drawing.Point(8, 8);
			this.grpSnapSettings.Name = "grpSnapSettings";
			this.grpSnapSettings.Size = new System.Drawing.Size(280, 88);
			this.grpSnapSettings.TabIndex = 1;
			this.grpSnapSettings.TabStop = false;
			this.grpSnapSettings.Text = "Snap Settings";
			// 
			// txtSnapDir
			// 
			this.txtSnapDir.Location = new System.Drawing.Point(72, 56);
			this.txtSnapDir.Name = "txtSnapDir";
			this.txtSnapDir.Size = new System.Drawing.Size(136, 20);
			this.txtSnapDir.TabIndex = 5;
			this.txtSnapDir.Text = "";
			// 
			// lblSnapDir
			// 
			this.lblSnapDir.AutoSize = true;
			this.lblSnapDir.Location = new System.Drawing.Point(16, 56);
			this.lblSnapDir.Name = "lblSnapDir";
			this.lblSnapDir.Size = new System.Drawing.Size(51, 16);
			this.lblSnapDir.TabIndex = 4;
			this.lblSnapDir.Text = "Snap Dir:";
			// 
			// lblSnapDelay
			// 
			this.lblSnapDelay.AutoSize = true;
			this.lblSnapDelay.Location = new System.Drawing.Point(104, 24);
			this.lblSnapDelay.Name = "lblSnapDelay";
			this.lblSnapDelay.Size = new System.Drawing.Size(66, 16);
			this.lblSnapDelay.TabIndex = 3;
			this.lblSnapDelay.Text = "Snap Delay:";
			// 
			// upDelay
			// 
			this.upDelay.Location = new System.Drawing.Point(176, 24);
			this.upDelay.Maximum = new System.Decimal(new int[] {
																	10000,
																	0,
																	0,
																	0});
			this.upDelay.Name = "upDelay";
			this.upDelay.Size = new System.Drawing.Size(96, 20);
			this.upDelay.TabIndex = 2;
			this.upDelay.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.upDelay.Value = new System.Decimal(new int[] {
																  100,
																  0,
																  0,
																  0});
			// 
			// chkSnapEnabled
			// 
			this.chkSnapEnabled.Location = new System.Drawing.Point(16, 24);
			this.chkSnapEnabled.Name = "chkSnapEnabled";
			this.chkSnapEnabled.Size = new System.Drawing.Size(80, 24);
			this.chkSnapEnabled.TabIndex = 1;
			this.chkSnapEnabled.Text = "Enabled";
			this.chkSnapEnabled.CheckedChanged += new System.EventHandler(this.chkSnapEnabled_CheckedChanged);
			// 
			// cmdBrowseSnapDir
			// 
			this.cmdBrowseSnapDir.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.cmdBrowseSnapDir.Location = new System.Drawing.Point(216, 56);
			this.cmdBrowseSnapDir.Name = "cmdBrowseSnapDir";
			this.cmdBrowseSnapDir.Size = new System.Drawing.Size(56, 24);
			this.cmdBrowseSnapDir.TabIndex = 6;
			this.cmdBrowseSnapDir.Text = "Browse";
			// 
			// grpStatsSettings
			// 
			this.grpStatsSettings.Controls.Add(this.cmdViewStats);
			this.grpStatsSettings.Controls.Add(this.cmdResetStats);
			this.grpStatsSettings.Controls.Add(this.chkStatsEnabled);
			this.grpStatsSettings.Location = new System.Drawing.Point(8, 112);
			this.grpStatsSettings.Name = "grpStatsSettings";
			this.grpStatsSettings.Size = new System.Drawing.Size(280, 64);
			this.grpStatsSettings.TabIndex = 3;
			this.grpStatsSettings.TabStop = false;
			this.grpStatsSettings.Text = "Stats Settings";
			// 
			// cmdViewStats
			// 
			this.cmdViewStats.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.cmdViewStats.Location = new System.Drawing.Point(128, 24);
			this.cmdViewStats.Name = "cmdViewStats";
			this.cmdViewStats.Size = new System.Drawing.Size(72, 24);
			this.cmdViewStats.TabIndex = 5;
			this.cmdViewStats.Text = "View";
			// 
			// cmdResetStats
			// 
			this.cmdResetStats.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.cmdResetStats.Location = new System.Drawing.Point(200, 24);
			this.cmdResetStats.Name = "cmdResetStats";
			this.cmdResetStats.Size = new System.Drawing.Size(72, 24);
			this.cmdResetStats.TabIndex = 4;
			this.cmdResetStats.Text = "Reset";
			// 
			// chkStatsEnabled
			// 
			this.chkStatsEnabled.Location = new System.Drawing.Point(16, 24);
			this.chkStatsEnabled.Name = "chkStatsEnabled";
			this.chkStatsEnabled.Size = new System.Drawing.Size(80, 24);
			this.chkStatsEnabled.TabIndex = 3;
			this.chkStatsEnabled.Text = "Enabled";
			// 
			// grpGlobalSettings
			// 
			this.grpGlobalSettings.Controls.Add(this.lblAccount);
			this.grpGlobalSettings.Controls.Add(this.txtAccount);
			this.grpGlobalSettings.Controls.Add(this.chkAutoDetectUser);
			this.grpGlobalSettings.Location = new System.Drawing.Point(8, 184);
			this.grpGlobalSettings.Name = "grpGlobalSettings";
			this.grpGlobalSettings.Size = new System.Drawing.Size(280, 56);
			this.grpGlobalSettings.TabIndex = 4;
			this.grpGlobalSettings.TabStop = false;
			this.grpGlobalSettings.Text = "Global Settings";
			// 
			// lblAccount
			// 
			this.lblAccount.Location = new System.Drawing.Point(120, 24);
			this.lblAccount.Name = "lblAccount";
			this.lblAccount.Size = new System.Drawing.Size(48, 16);
			this.lblAccount.TabIndex = 2;
			this.lblAccount.Text = "Account:";
			this.lblAccount.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// txtAccount
			// 
			this.txtAccount.Location = new System.Drawing.Point(176, 24);
			this.txtAccount.Name = "txtAccount";
			this.txtAccount.Size = new System.Drawing.Size(96, 20);
			this.txtAccount.TabIndex = 1;
			this.txtAccount.Text = "";
			// 
			// chkAutoDetectUser
			// 
			this.chkAutoDetectUser.Location = new System.Drawing.Point(16, 24);
			this.chkAutoDetectUser.Name = "chkAutoDetectUser";
			this.chkAutoDetectUser.TabIndex = 0;
			this.chkAutoDetectUser.Text = "Auto Dect User";
			// 
			// frmMain
			// 
			this.AutoScale = false;
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(296, 245);
			this.Controls.Add(this.grpGlobalSettings);
			this.Controls.Add(this.grpStatsSettings);
			this.Controls.Add(this.grpSnapSettings);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.Name = "frmMain";
			this.Text = "Smile!";
			this.WindowState = System.Windows.Forms.FormWindowState.Minimized;
			this.Load += new System.EventHandler(this.frmMain_Load);
			this.grpSnapSettings.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.upDelay)).EndInit();
			this.grpStatsSettings.ResumeLayout(false);
			this.grpGlobalSettings.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion

		/// The main entry point for the application.
		[STAThread]
		static void Main() 
		{
			// Start Main Form
			Application.Run(new frmMain());
		}

		private void frmMain_Load(object sender, System.EventArgs e)
		{

		}

		private void frmMain_Resize(object sender, System.EventArgs e)
		{
			if (FormWindowState.Minimized == WindowState)
				Hide();
		}

		private void notifyIcon_DoubleClick(object sender, System.EventArgs e)
		{
			// TODO: Add toggle
			Show();
			WindowState = FormWindowState.Normal;
		}

		private void menuItem2_Click(object sender, System.EventArgs e)
		{
			Show();
			WindowState = FormWindowState.Normal;
		}

		private void menuItem3_Click(object sender, System.EventArgs e)
		{
			Close();
		}

		private void TimerCheckKills_Tick(object sender, System.EventArgs e)
		{
			Image img;
			if(this.ConsoleParser.NewKills())
			{
				img = NativeMethods.GetDesktopBitmap();
				Thread.Sleep(1000);
				img.Save(this.ConsoleParser.dir + @"\kill-" + 
					   DateTime.Now.Year.ToString() + DateTime.Now.Month.ToString() + DateTime.Now.Day.ToString() + "_" + 
					   DateTime.Now.Hour.ToString() + DateTime.Now.Minute.ToString() + DateTime.Now.Second.ToString() + DateTime.Now.Millisecond.ToString() 
					   + ".jpg", 
					System.Drawing.Imaging.ImageFormat.Jpeg);
			}
		}

		private void menuItem1_Click(object sender, System.EventArgs e)
		{
			menuItem1.Checked = !menuItem1.Checked;
			if(menuItem1.Checked == true)
				TimerCheckKills.Enabled = true;
			else
				TimerCheckKills.Enabled = false;
		}

		private void chkSnapEnabled_CheckedChanged(object sender, System.EventArgs e)
		{
			if(chkSnapEnabled.Checked)
				;
			else
				;
		}


	}
}
