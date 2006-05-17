/////////////////////////////////////////////////////////////////////////////
//
// Smile! -- CounterStrike: Source Screenshot and Statistics Utility
// v1.2
// Written by Marek Kudlacz
// Copyright (c)2005
//
/////////////////////////////////////////////////////////////////////////////


using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.IO;
using System.Data;
using System.Threading;
using System.Xml.Serialization;
using System.Diagnostics;
using System.Drawing.Imaging;

namespace smiletray
{
	// Main Options Form
	public class frmMain : System.Windows.Forms.Form
	{
		// Volitile State	

		// private stuff for this form
		private Settings_t Settings;
		private bool EnableStats;	// Volitile State
		private bool EnableSnaps;	// Volitile State
		private bool AllowShow;		
		private EncoderParameters encoderParams;
		private ImageCodecInfo encoder;
		private string encoderstring;
		private string encoderext;
		private Font encoderfont;
		private CProfile [] Profiles;
		private CProfile [] TempProfiles;
		private int ActiveTempProfile;
		private CProfile ActiveProfile;

		// form elements
		private System.Windows.Forms.NotifyIcon notifyIcon;
		private System.Windows.Forms.ContextMenu contextMenu;
		private System.ComponentModel.IContainer components;
		private System.Windows.Forms.MenuItem menuItem4;
		private System.Windows.Forms.MenuItem menuItem6;
		private System.Windows.Forms.Timer TimerMisc;
		private System.Windows.Forms.Button cmdOK;
		private System.Windows.Forms.Button cmdCancel;
		private System.Windows.Forms.Timer TimerCheckConsoleLog;
		private System.Windows.Forms.MenuItem mnuViewStats;
		private System.Windows.Forms.MenuItem mnuOpenSnapDir;
		private System.Windows.Forms.MenuItem menuOpen;
		private System.Windows.Forms.MenuItem mnuEnableSnaps;
		private System.Windows.Forms.MenuItem mnuEnableStats;	
		private System.Windows.Forms.MenuItem mnuExit;	
		private System.Windows.Forms.MenuItem menuItem1;
		private System.Windows.Forms.MenuItem mnuAbout;
		private System.Windows.Forms.TabPage tabGeneral;
		private System.Windows.Forms.TabPage tabProfiles;
		private System.Windows.Forms.GroupBox grpProfiles_SnapSettingsSub;
		private System.Windows.Forms.Button cmdProfiles_BrowseSnapDir;
		private System.Windows.Forms.TabPage tabProfiles_GameSettings;
		private System.Windows.Forms.TabPage tabProfiles_SnapSettings;
		private System.Windows.Forms.TabPage tabProfiles_StatsSettings;
		private System.Windows.Forms.TabPage tabGeneral_GlobalSnapSettings;
		private System.Windows.Forms.TabPage tabGeneral_GlobalStatsSettings;
		private System.Windows.Forms.TabPage tabAbout;
		private System.Windows.Forms.CheckBox chkGeneral_SnapSettings_SingleDisplay;
		private System.Windows.Forms.CheckBox chkGeneral_SnapSettings_Enabled;
		private System.Windows.Forms.TabControl tabOptions;
		private System.Windows.Forms.TabControl tabGeneralOptions;
		private System.Windows.Forms.TabControl tabProfileOptions;
		private System.Windows.Forms.RichTextBox rtxtAbout;
		private System.Windows.Forms.PictureBox picAboutIcon;
		private System.Windows.Forms.TextBox txtGeneral_SnapSettings_SnapDir;
		private System.Windows.Forms.GroupBox grpGeneral_SnapSettings_ImageFormat;
		private System.Windows.Forms.Label lblGeneral_SnapSettings_Quality;
		private System.Windows.Forms.TrackBar tbGeneral_SnapSettings_Quality;
		private System.Windows.Forms.GroupBox grpGeneral_SnapSettings_SnapDir;
		private System.Windows.Forms.Button cmdGeneral_SnapSettings_BrowseSnapDir;
		private System.Windows.Forms.GroupBox grpGeneral_SnapSettings_Delay;
		private System.Windows.Forms.NumericUpDown udGeneral_SnapSettings_Delay;
		private System.Windows.Forms.Button cmdGeneral_StatsSettings_ViewStats;
		private System.Windows.Forms.Button cmdGeneral_StatsSettings_Reset;
		private System.Windows.Forms.GroupBox grpProfiles_GameSettings_Path;
		private System.Windows.Forms.Button cmdProfiles_GameSettings_AutoDetect;
		private System.Windows.Forms.Button cmdProfiles_GameSettings_BrowseGamePath;
		private System.Windows.Forms.TextBox txtProfiles_GameSettings_Path;
		private System.Windows.Forms.CheckBox chkProfiles_SnapSettings_UseGlobal;
		private System.Windows.Forms.GroupBox grpProfiles_SnapSettings_SnapDir;
		private System.Windows.Forms.TextBox txtProfiles_SnapSettings_SnapDir;
		private System.Windows.Forms.GroupBox grpProfiles_SnapSettings_Delay;
		private System.Windows.Forms.GroupBox grpProfiles_StatsSettingsSub;
		private System.Windows.Forms.CheckBox chkProfiles_StatsSettings_UseGlobal;
		private System.Windows.Forms.CheckBox chkProfiles_SnapSettings_Enabled;
		private System.Windows.Forms.CheckBox chkProfiles_StatsSettings_Enabled;
		private System.Windows.Forms.CheckBox chkProfiles_SnapSettings_SingleDisplay;
		private System.Windows.Forms.ListView lstProfiles_Games;
		private System.Windows.Forms.NumericUpDown udProfiles_SnapSettings_Delay;
		private System.Windows.Forms.FolderBrowserDialog dlgBrowseDir;
		private System.Windows.Forms.ComboBox cbGeneral_SnapSettings_ImageFormat;
		private System.Windows.Forms.Label lblGeneral_SnapSettings_ImageFormat;
		private System.Windows.Forms.Button cmdProfiles_StatsSettings_View;
		private System.Windows.Forms.Button cmdProfiles_StatsSettings_Reset;
		private System.Windows.Forms.Label lblProfiles_ActiveProfile;
		private System.Windows.Forms.TabPage tabLog;
		private System.Windows.Forms.RichTextBox rtxtLog;
		private System.Windows.Forms.CheckBox chkGeneral_StatsSettings_Enabled;

		//////////////////////////////////////////////////////////////////////////////////////////////
		/// Form Methods
		//////////////////////////////////////////////////////////////////////////////////////////////
	
		public frmMain()
		{
			// Required for Windows Form Designer support
			InitializeComponent();

			// Read Settings
			this.Settings = new Settings_t();
			AddProfiles ( ref Profiles );  // Just in case none where created
			this.LoadSettings();

			// Populate profiles
			AddProfiles ( ref Profiles );
			TempProfiles = new CProfile [ Profiles.Length ];
			for(int i = 0; i < Profiles.Length; i++)
			{
				TempProfiles[i] = (CProfile) System.Activator.CreateInstance(Profiles[i].GetType());
				ListViewItem item = new ListViewItem();
				item.Text = Profiles[i].ProfileName;
				item.Tag = i;
				lstProfiles_Games.Items.Add(item);
			}

			// Encoders
			ImageCodecInfo [] encoders = ImageCodecInfo.GetImageEncoders();
			foreach(ImageCodecInfo enc in encoders)
			{
				cbGeneral_SnapSettings_ImageFormat.Items.Add(enc.MimeType);
			}

			UpdateEncoder();

			// Misc Dialog Options
			this.AllowShow = true;
			this.notifyIcon.Text = this.Text = "Smile! " + Info.version;

			// Populate Dialog
			PopulateOptions();
			this.EnableSnaps = this.Settings.SnapSettings.Enabled;
			this.EnableStats = this.Settings.StatsSettings.Enabled;

			// Populate the rich text box for about screen
			rtxtAbout.SelectionStart = 0 ;
			rtxtAbout.SelectionFont = new Font("Verdana", 12, FontStyle.Bold);
			rtxtAbout.SelectedText = "Smile! " ;
			rtxtAbout.SelectionFont = new Font("Verdana", 10, FontStyle.Regular);
			rtxtAbout.SelectedText = "v" + Info.version + "\n";
			rtxtAbout.SelectedText = "�2005 Marek Kudlacz\nhttp://www.kudlacz.com\n";
			rtxtAbout.SelectionFont = new Font("Verdana", 12, FontStyle.Bold|FontStyle.Underline);
			rtxtAbout.SelectedText = "                                                                                                            \n\n";
			rtxtAbout.SelectedText = "This is a program designed in C#/.NET to keep track of statistics, and to take \"kill\" screenshots when you actually manage to kill someone. Enjoy!\n";
			rtxtAbout.SelectedText = "\n\nSmile!, Copyright (C) 2005 Marek Kudlacz. Smile! comes with ABSOLUTELY NO WARRANTY. This is free software, and you are welcome to redistribute it under certain conditions; for details see bundled LICENSE.TXT.";
			rtxtAbout.SelectionStart = 0 ;

			CheckEnabled();
		}

		private void rtxtAbout_LinkClicked(object sender, System.Windows.Forms.LinkClickedEventArgs e)
		{
			try
			{
				Process.Start(e.LinkText);
			}
			catch
			{
				MessageBox.Show("Error: Could not open default browser!", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
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
			this.mnuEnableSnaps = new System.Windows.Forms.MenuItem();
			this.mnuEnableStats = new System.Windows.Forms.MenuItem();
			this.menuItem6 = new System.Windows.Forms.MenuItem();
			this.mnuViewStats = new System.Windows.Forms.MenuItem();
			this.mnuOpenSnapDir = new System.Windows.Forms.MenuItem();
			this.menuItem4 = new System.Windows.Forms.MenuItem();
			this.menuOpen = new System.Windows.Forms.MenuItem();
			this.mnuAbout = new System.Windows.Forms.MenuItem();
			this.menuItem1 = new System.Windows.Forms.MenuItem();
			this.mnuExit = new System.Windows.Forms.MenuItem();
			this.TimerCheckConsoleLog = new System.Windows.Forms.Timer(this.components);
			this.dlgBrowseDir = new System.Windows.Forms.FolderBrowserDialog();
			this.TimerMisc = new System.Windows.Forms.Timer(this.components);
			this.cmdOK = new System.Windows.Forms.Button();
			this.cmdCancel = new System.Windows.Forms.Button();
			this.lstProfiles_Games = new System.Windows.Forms.ListView();
			this.tabOptions = new System.Windows.Forms.TabControl();
			this.tabGeneral = new System.Windows.Forms.TabPage();
			this.tabGeneralOptions = new System.Windows.Forms.TabControl();
			this.tabGeneral_GlobalSnapSettings = new System.Windows.Forms.TabPage();
			this.grpGeneral_SnapSettings_ImageFormat = new System.Windows.Forms.GroupBox();
			this.lblGeneral_SnapSettings_ImageFormat = new System.Windows.Forms.Label();
			this.cbGeneral_SnapSettings_ImageFormat = new System.Windows.Forms.ComboBox();
			this.lblGeneral_SnapSettings_Quality = new System.Windows.Forms.Label();
			this.tbGeneral_SnapSettings_Quality = new System.Windows.Forms.TrackBar();
			this.grpGeneral_SnapSettings_SnapDir = new System.Windows.Forms.GroupBox();
			this.txtGeneral_SnapSettings_SnapDir = new System.Windows.Forms.TextBox();
			this.cmdGeneral_SnapSettings_BrowseSnapDir = new System.Windows.Forms.Button();
			this.grpGeneral_SnapSettings_Delay = new System.Windows.Forms.GroupBox();
			this.udGeneral_SnapSettings_Delay = new System.Windows.Forms.NumericUpDown();
			this.chkGeneral_SnapSettings_SingleDisplay = new System.Windows.Forms.CheckBox();
			this.chkGeneral_SnapSettings_Enabled = new System.Windows.Forms.CheckBox();
			this.tabGeneral_GlobalStatsSettings = new System.Windows.Forms.TabPage();
			this.cmdGeneral_StatsSettings_ViewStats = new System.Windows.Forms.Button();
			this.cmdGeneral_StatsSettings_Reset = new System.Windows.Forms.Button();
			this.chkGeneral_StatsSettings_Enabled = new System.Windows.Forms.CheckBox();
			this.tabProfiles = new System.Windows.Forms.TabPage();
			this.lblProfiles_ActiveProfile = new System.Windows.Forms.Label();
			this.tabProfileOptions = new System.Windows.Forms.TabControl();
			this.tabProfiles_GameSettings = new System.Windows.Forms.TabPage();
			this.grpProfiles_GameSettings_Path = new System.Windows.Forms.GroupBox();
			this.cmdProfiles_GameSettings_AutoDetect = new System.Windows.Forms.Button();
			this.cmdProfiles_GameSettings_BrowseGamePath = new System.Windows.Forms.Button();
			this.txtProfiles_GameSettings_Path = new System.Windows.Forms.TextBox();
			this.tabProfiles_SnapSettings = new System.Windows.Forms.TabPage();
			this.chkProfiles_SnapSettings_UseGlobal = new System.Windows.Forms.CheckBox();
			this.grpProfiles_SnapSettingsSub = new System.Windows.Forms.GroupBox();
			this.grpProfiles_SnapSettings_SnapDir = new System.Windows.Forms.GroupBox();
			this.txtProfiles_SnapSettings_SnapDir = new System.Windows.Forms.TextBox();
			this.cmdProfiles_BrowseSnapDir = new System.Windows.Forms.Button();
			this.grpProfiles_SnapSettings_Delay = new System.Windows.Forms.GroupBox();
			this.udProfiles_SnapSettings_Delay = new System.Windows.Forms.NumericUpDown();
			this.chkProfiles_SnapSettings_SingleDisplay = new System.Windows.Forms.CheckBox();
			this.chkProfiles_SnapSettings_Enabled = new System.Windows.Forms.CheckBox();
			this.tabProfiles_StatsSettings = new System.Windows.Forms.TabPage();
			this.chkProfiles_StatsSettings_UseGlobal = new System.Windows.Forms.CheckBox();
			this.grpProfiles_StatsSettingsSub = new System.Windows.Forms.GroupBox();
			this.chkProfiles_StatsSettings_Enabled = new System.Windows.Forms.CheckBox();
			this.cmdProfiles_StatsSettings_View = new System.Windows.Forms.Button();
			this.cmdProfiles_StatsSettings_Reset = new System.Windows.Forms.Button();
			this.tabAbout = new System.Windows.Forms.TabPage();
			this.rtxtAbout = new System.Windows.Forms.RichTextBox();
			this.picAboutIcon = new System.Windows.Forms.PictureBox();
			this.tabLog = new System.Windows.Forms.TabPage();
			this.rtxtLog = new System.Windows.Forms.RichTextBox();
			this.tabOptions.SuspendLayout();
			this.tabGeneral.SuspendLayout();
			this.tabGeneralOptions.SuspendLayout();
			this.tabGeneral_GlobalSnapSettings.SuspendLayout();
			this.grpGeneral_SnapSettings_ImageFormat.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.tbGeneral_SnapSettings_Quality)).BeginInit();
			this.grpGeneral_SnapSettings_SnapDir.SuspendLayout();
			this.grpGeneral_SnapSettings_Delay.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.udGeneral_SnapSettings_Delay)).BeginInit();
			this.tabGeneral_GlobalStatsSettings.SuspendLayout();
			this.tabProfiles.SuspendLayout();
			this.tabProfileOptions.SuspendLayout();
			this.tabProfiles_GameSettings.SuspendLayout();
			this.grpProfiles_GameSettings_Path.SuspendLayout();
			this.tabProfiles_SnapSettings.SuspendLayout();
			this.grpProfiles_SnapSettingsSub.SuspendLayout();
			this.grpProfiles_SnapSettings_SnapDir.SuspendLayout();
			this.grpProfiles_SnapSettings_Delay.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.udProfiles_SnapSettings_Delay)).BeginInit();
			this.tabProfiles_StatsSettings.SuspendLayout();
			this.grpProfiles_StatsSettingsSub.SuspendLayout();
			this.tabAbout.SuspendLayout();
			this.tabLog.SuspendLayout();
			this.SuspendLayout();
			// 
			// notifyIcon
			// 
			this.notifyIcon.ContextMenu = this.contextMenu;
			this.notifyIcon.Icon = ((System.Drawing.Icon)(resources.GetObject("notifyIcon.Icon")));
			this.notifyIcon.Text = "Smile!";
			this.notifyIcon.Visible = true;
			this.notifyIcon.DoubleClick += new System.EventHandler(this.notifyIcon_DoubleClick);
			// 
			// contextMenu
			// 
			this.contextMenu.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																						this.mnuEnableSnaps,
																						this.mnuEnableStats,
																						this.menuItem6,
																						this.mnuViewStats,
																						this.mnuOpenSnapDir,
																						this.menuItem4,
																						this.menuOpen,
																						this.mnuAbout,
																						this.menuItem1,
																						this.mnuExit});
			// 
			// mnuEnableSnaps
			// 
			this.mnuEnableSnaps.Checked = true;
			this.mnuEnableSnaps.Index = 0;
			this.mnuEnableSnaps.Text = "Enable Snaps";
			this.mnuEnableSnaps.Click += new System.EventHandler(this.mnuEnableSnaps_Click);
			// 
			// mnuEnableStats
			// 
			this.mnuEnableStats.Checked = true;
			this.mnuEnableStats.Index = 1;
			this.mnuEnableStats.Text = "Enable Stats";
			this.mnuEnableStats.Click += new System.EventHandler(this.mnuEnableStats_Click);
			// 
			// menuItem6
			// 
			this.menuItem6.Index = 2;
			this.menuItem6.Text = "-";
			// 
			// mnuViewStats
			// 
			this.mnuViewStats.Index = 3;
			this.mnuViewStats.Text = "View Stats";
			this.mnuViewStats.Click += new System.EventHandler(this.mnuViewStats_Click);
			// 
			// mnuOpenSnapDir
			// 
			this.mnuOpenSnapDir.Index = 4;
			this.mnuOpenSnapDir.Text = "Open Snap Folder";
			this.mnuOpenSnapDir.Click += new System.EventHandler(this.mnuOpenSnapDir_Click);
			// 
			// menuItem4
			// 
			this.menuItem4.Index = 5;
			this.menuItem4.Text = "-";
			// 
			// menuOpen
			// 
			this.menuOpen.Index = 6;
			this.menuOpen.Text = "Open";
			this.menuOpen.Click += new System.EventHandler(this.mnuOpen_Click);
			// 
			// mnuAbout
			// 
			this.mnuAbout.Index = 7;
			this.mnuAbout.Text = "About";
			this.mnuAbout.Click += new System.EventHandler(this.mnuAbout_Click);
			// 
			// menuItem1
			// 
			this.menuItem1.Index = 8;
			this.menuItem1.Text = "-";
			// 
			// mnuExit
			// 
			this.mnuExit.Index = 9;
			this.mnuExit.Text = "Exit";
			this.mnuExit.Click += new System.EventHandler(this.mnuExit_Click);
			// 
			// TimerCheckConsoleLog
			// 
			this.TimerCheckConsoleLog.Interval = 10;
			this.TimerCheckConsoleLog.Tick += new System.EventHandler(this.TimerCheckConsoleLog_Tick);
			// 
			// TimerMisc
			// 
			this.TimerMisc.Enabled = true;
			this.TimerMisc.Interval = 5000;
			this.TimerMisc.Tick += new System.EventHandler(this.TimerMisc_Tick);
			// 
			// cmdOK
			// 
			this.cmdOK.Location = new System.Drawing.Point(8, 416);
			this.cmdOK.Name = "cmdOK";
			this.cmdOK.Size = new System.Drawing.Size(64, 24);
			this.cmdOK.TabIndex = 5;
			this.cmdOK.Text = "OK";
			this.cmdOK.Click += new System.EventHandler(this.cmdOK_Click);
			// 
			// cmdCancel
			// 
			this.cmdCancel.Location = new System.Drawing.Point(72, 416);
			this.cmdCancel.Name = "cmdCancel";
			this.cmdCancel.Size = new System.Drawing.Size(64, 24);
			this.cmdCancel.TabIndex = 6;
			this.cmdCancel.Text = "Cancel";
			this.cmdCancel.Click += new System.EventHandler(this.cmdCancel_Click);
			// 
			// lstProfiles_Games
			// 
			this.lstProfiles_Games.FullRowSelect = true;
			this.lstProfiles_Games.Location = new System.Drawing.Point(8, 8);
			this.lstProfiles_Games.MultiSelect = false;
			this.lstProfiles_Games.Name = "lstProfiles_Games";
			this.lstProfiles_Games.Size = new System.Drawing.Size(136, 360);
			this.lstProfiles_Games.Sorting = System.Windows.Forms.SortOrder.Ascending;
			this.lstProfiles_Games.TabIndex = 8;
			this.lstProfiles_Games.View = System.Windows.Forms.View.List;
			this.lstProfiles_Games.SelectedIndexChanged += new System.EventHandler(this.lstProfiles_Games_SelectedIndexChanged);
			// 
			// tabOptions
			// 
			this.tabOptions.Appearance = System.Windows.Forms.TabAppearance.FlatButtons;
			this.tabOptions.Controls.Add(this.tabGeneral);
			this.tabOptions.Controls.Add(this.tabProfiles);
			this.tabOptions.Controls.Add(this.tabAbout);
			this.tabOptions.Controls.Add(this.tabLog);
			this.tabOptions.Location = new System.Drawing.Point(0, 8);
			this.tabOptions.Name = "tabOptions";
			this.tabOptions.SelectedIndex = 0;
			this.tabOptions.Size = new System.Drawing.Size(432, 408);
			this.tabOptions.TabIndex = 9;
			// 
			// tabGeneral
			// 
			this.tabGeneral.Controls.Add(this.tabGeneralOptions);
			this.tabGeneral.Location = new System.Drawing.Point(4, 25);
			this.tabGeneral.Name = "tabGeneral";
			this.tabGeneral.Size = new System.Drawing.Size(424, 379);
			this.tabGeneral.TabIndex = 0;
			this.tabGeneral.Text = "General";
			// 
			// tabGeneralOptions
			// 
			this.tabGeneralOptions.Controls.Add(this.tabGeneral_GlobalSnapSettings);
			this.tabGeneralOptions.Controls.Add(this.tabGeneral_GlobalStatsSettings);
			this.tabGeneralOptions.Location = new System.Drawing.Point(8, 8);
			this.tabGeneralOptions.Name = "tabGeneralOptions";
			this.tabGeneralOptions.SelectedIndex = 0;
			this.tabGeneralOptions.Size = new System.Drawing.Size(408, 368);
			this.tabGeneralOptions.TabIndex = 4;
			// 
			// tabGeneral_GlobalSnapSettings
			// 
			this.tabGeneral_GlobalSnapSettings.Controls.Add(this.grpGeneral_SnapSettings_ImageFormat);
			this.tabGeneral_GlobalSnapSettings.Controls.Add(this.grpGeneral_SnapSettings_SnapDir);
			this.tabGeneral_GlobalSnapSettings.Controls.Add(this.grpGeneral_SnapSettings_Delay);
			this.tabGeneral_GlobalSnapSettings.Controls.Add(this.chkGeneral_SnapSettings_SingleDisplay);
			this.tabGeneral_GlobalSnapSettings.Controls.Add(this.chkGeneral_SnapSettings_Enabled);
			this.tabGeneral_GlobalSnapSettings.Location = new System.Drawing.Point(4, 22);
			this.tabGeneral_GlobalSnapSettings.Name = "tabGeneral_GlobalSnapSettings";
			this.tabGeneral_GlobalSnapSettings.Size = new System.Drawing.Size(400, 342);
			this.tabGeneral_GlobalSnapSettings.TabIndex = 0;
			this.tabGeneral_GlobalSnapSettings.Text = "Global Snap Settings";
			// 
			// grpGeneral_SnapSettings_ImageFormat
			// 
			this.grpGeneral_SnapSettings_ImageFormat.Controls.Add(this.lblGeneral_SnapSettings_ImageFormat);
			this.grpGeneral_SnapSettings_ImageFormat.Controls.Add(this.cbGeneral_SnapSettings_ImageFormat);
			this.grpGeneral_SnapSettings_ImageFormat.Controls.Add(this.lblGeneral_SnapSettings_Quality);
			this.grpGeneral_SnapSettings_ImageFormat.Controls.Add(this.tbGeneral_SnapSettings_Quality);
			this.grpGeneral_SnapSettings_ImageFormat.Location = new System.Drawing.Point(8, 168);
			this.grpGeneral_SnapSettings_ImageFormat.Name = "grpGeneral_SnapSettings_ImageFormat";
			this.grpGeneral_SnapSettings_ImageFormat.Size = new System.Drawing.Size(384, 128);
			this.grpGeneral_SnapSettings_ImageFormat.TabIndex = 23;
			this.grpGeneral_SnapSettings_ImageFormat.TabStop = false;
			this.grpGeneral_SnapSettings_ImageFormat.Text = "Image Format";
			// 
			// lblGeneral_SnapSettings_ImageFormat
			// 
			this.lblGeneral_SnapSettings_ImageFormat.Location = new System.Drawing.Point(16, 24);
			this.lblGeneral_SnapSettings_ImageFormat.Name = "lblGeneral_SnapSettings_ImageFormat";
			this.lblGeneral_SnapSettings_ImageFormat.Size = new System.Drawing.Size(104, 16);
			this.lblGeneral_SnapSettings_ImageFormat.TabIndex = 21;
			this.lblGeneral_SnapSettings_ImageFormat.Text = "Image Output Type:";
			// 
			// cbGeneral_SnapSettings_ImageFormat
			// 
			this.cbGeneral_SnapSettings_ImageFormat.Location = new System.Drawing.Point(120, 24);
			this.cbGeneral_SnapSettings_ImageFormat.Name = "cbGeneral_SnapSettings_ImageFormat";
			this.cbGeneral_SnapSettings_ImageFormat.Size = new System.Drawing.Size(256, 21);
			this.cbGeneral_SnapSettings_ImageFormat.TabIndex = 20;
			this.cbGeneral_SnapSettings_ImageFormat.Text = "Image Encoders";
			// 
			// lblGeneral_SnapSettings_Quality
			// 
			this.lblGeneral_SnapSettings_Quality.Location = new System.Drawing.Point(16, 64);
			this.lblGeneral_SnapSettings_Quality.Name = "lblGeneral_SnapSettings_Quality";
			this.lblGeneral_SnapSettings_Quality.Size = new System.Drawing.Size(176, 16);
			this.lblGeneral_SnapSettings_Quality.TabIndex = 19;
			this.lblGeneral_SnapSettings_Quality.Text = "Quality x% (100% = Clearest):";
			// 
			// tbGeneral_SnapSettings_Quality
			// 
			this.tbGeneral_SnapSettings_Quality.Location = new System.Drawing.Point(8, 80);
			this.tbGeneral_SnapSettings_Quality.Maximum = 100;
			this.tbGeneral_SnapSettings_Quality.Name = "tbGeneral_SnapSettings_Quality";
			this.tbGeneral_SnapSettings_Quality.Size = new System.Drawing.Size(368, 42);
			this.tbGeneral_SnapSettings_Quality.TabIndex = 18;
			this.tbGeneral_SnapSettings_Quality.Value = 100;
			this.tbGeneral_SnapSettings_Quality.Scroll += new System.EventHandler(this.tbGeneral_SnapSettings_Quality_Scroll);
			// 
			// grpGeneral_SnapSettings_SnapDir
			// 
			this.grpGeneral_SnapSettings_SnapDir.Controls.Add(this.txtGeneral_SnapSettings_SnapDir);
			this.grpGeneral_SnapSettings_SnapDir.Controls.Add(this.cmdGeneral_SnapSettings_BrowseSnapDir);
			this.grpGeneral_SnapSettings_SnapDir.Location = new System.Drawing.Point(8, 112);
			this.grpGeneral_SnapSettings_SnapDir.Name = "grpGeneral_SnapSettings_SnapDir";
			this.grpGeneral_SnapSettings_SnapDir.Size = new System.Drawing.Size(384, 48);
			this.grpGeneral_SnapSettings_SnapDir.TabIndex = 22;
			this.grpGeneral_SnapSettings_SnapDir.TabStop = false;
			this.grpGeneral_SnapSettings_SnapDir.Text = "Snap Directory";
			// 
			// txtGeneral_SnapSettings_SnapDir
			// 
			this.txtGeneral_SnapSettings_SnapDir.Location = new System.Drawing.Point(8, 16);
			this.txtGeneral_SnapSettings_SnapDir.Name = "txtGeneral_SnapSettings_SnapDir";
			this.txtGeneral_SnapSettings_SnapDir.Size = new System.Drawing.Size(304, 20);
			this.txtGeneral_SnapSettings_SnapDir.TabIndex = 5;
			this.txtGeneral_SnapSettings_SnapDir.Text = "";
			// 
			// cmdGeneral_SnapSettings_BrowseSnapDir
			// 
			this.cmdGeneral_SnapSettings_BrowseSnapDir.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.cmdGeneral_SnapSettings_BrowseSnapDir.Location = new System.Drawing.Point(320, 16);
			this.cmdGeneral_SnapSettings_BrowseSnapDir.Name = "cmdGeneral_SnapSettings_BrowseSnapDir";
			this.cmdGeneral_SnapSettings_BrowseSnapDir.Size = new System.Drawing.Size(56, 24);
			this.cmdGeneral_SnapSettings_BrowseSnapDir.TabIndex = 6;
			this.cmdGeneral_SnapSettings_BrowseSnapDir.Text = "Browse";
			this.cmdGeneral_SnapSettings_BrowseSnapDir.Click += new System.EventHandler(this.cmdGeneral_SnapSettings_BrowseSnapDir_Click);
			// 
			// grpGeneral_SnapSettings_Delay
			// 
			this.grpGeneral_SnapSettings_Delay.Controls.Add(this.udGeneral_SnapSettings_Delay);
			this.grpGeneral_SnapSettings_Delay.Location = new System.Drawing.Point(8, 56);
			this.grpGeneral_SnapSettings_Delay.Name = "grpGeneral_SnapSettings_Delay";
			this.grpGeneral_SnapSettings_Delay.Size = new System.Drawing.Size(384, 48);
			this.grpGeneral_SnapSettings_Delay.TabIndex = 21;
			this.grpGeneral_SnapSettings_Delay.TabStop = false;
			this.grpGeneral_SnapSettings_Delay.Text = "Delay";
			// 
			// udGeneral_SnapSettings_Delay
			// 
			this.udGeneral_SnapSettings_Delay.Location = new System.Drawing.Point(8, 16);
			this.udGeneral_SnapSettings_Delay.Maximum = new System.Decimal(new int[] {
																						 10000,
																						 0,
																						 0,
																						 0});
			this.udGeneral_SnapSettings_Delay.Name = "udGeneral_SnapSettings_Delay";
			this.udGeneral_SnapSettings_Delay.Size = new System.Drawing.Size(368, 20);
			this.udGeneral_SnapSettings_Delay.TabIndex = 21;
			this.udGeneral_SnapSettings_Delay.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// chkGeneral_SnapSettings_SingleDisplay
			// 
			this.chkGeneral_SnapSettings_SingleDisplay.Location = new System.Drawing.Point(8, 32);
			this.chkGeneral_SnapSettings_SingleDisplay.Name = "chkGeneral_SnapSettings_SingleDisplay";
			this.chkGeneral_SnapSettings_SingleDisplay.TabIndex = 17;
			this.chkGeneral_SnapSettings_SingleDisplay.Text = "Single Display";
			// 
			// chkGeneral_SnapSettings_Enabled
			// 
			this.chkGeneral_SnapSettings_Enabled.Location = new System.Drawing.Point(8, 8);
			this.chkGeneral_SnapSettings_Enabled.Name = "chkGeneral_SnapSettings_Enabled";
			this.chkGeneral_SnapSettings_Enabled.Size = new System.Drawing.Size(80, 24);
			this.chkGeneral_SnapSettings_Enabled.TabIndex = 11;
			this.chkGeneral_SnapSettings_Enabled.Text = "Enabled";
			// 
			// tabGeneral_GlobalStatsSettings
			// 
			this.tabGeneral_GlobalStatsSettings.Controls.Add(this.cmdGeneral_StatsSettings_ViewStats);
			this.tabGeneral_GlobalStatsSettings.Controls.Add(this.cmdGeneral_StatsSettings_Reset);
			this.tabGeneral_GlobalStatsSettings.Controls.Add(this.chkGeneral_StatsSettings_Enabled);
			this.tabGeneral_GlobalStatsSettings.Location = new System.Drawing.Point(4, 22);
			this.tabGeneral_GlobalStatsSettings.Name = "tabGeneral_GlobalStatsSettings";
			this.tabGeneral_GlobalStatsSettings.Size = new System.Drawing.Size(400, 342);
			this.tabGeneral_GlobalStatsSettings.TabIndex = 1;
			this.tabGeneral_GlobalStatsSettings.Text = "Global Stats Settings";
			// 
			// cmdGeneral_StatsSettings_ViewStats
			// 
			this.cmdGeneral_StatsSettings_ViewStats.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.cmdGeneral_StatsSettings_ViewStats.Location = new System.Drawing.Point(248, 8);
			this.cmdGeneral_StatsSettings_ViewStats.Name = "cmdGeneral_StatsSettings_ViewStats";
			this.cmdGeneral_StatsSettings_ViewStats.Size = new System.Drawing.Size(72, 24);
			this.cmdGeneral_StatsSettings_ViewStats.TabIndex = 8;
			this.cmdGeneral_StatsSettings_ViewStats.Text = "View";
			this.cmdGeneral_StatsSettings_ViewStats.Click += new System.EventHandler(this.cmdGeneral_StatsSettings_ViewStats_Click);
			// 
			// cmdGeneral_StatsSettings_Reset
			// 
			this.cmdGeneral_StatsSettings_Reset.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.cmdGeneral_StatsSettings_Reset.Location = new System.Drawing.Point(320, 8);
			this.cmdGeneral_StatsSettings_Reset.Name = "cmdGeneral_StatsSettings_Reset";
			this.cmdGeneral_StatsSettings_Reset.Size = new System.Drawing.Size(72, 24);
			this.cmdGeneral_StatsSettings_Reset.TabIndex = 7;
			this.cmdGeneral_StatsSettings_Reset.Text = "Reset";
			this.cmdGeneral_StatsSettings_Reset.Click += new System.EventHandler(this.cmdGeneral_StatsSettings_Reset_Click);
			// 
			// chkGeneral_StatsSettings_Enabled
			// 
			this.chkGeneral_StatsSettings_Enabled.Location = new System.Drawing.Point(8, 8);
			this.chkGeneral_StatsSettings_Enabled.Name = "chkGeneral_StatsSettings_Enabled";
			this.chkGeneral_StatsSettings_Enabled.Size = new System.Drawing.Size(80, 24);
			this.chkGeneral_StatsSettings_Enabled.TabIndex = 6;
			this.chkGeneral_StatsSettings_Enabled.Text = "Enabled";
			// 
			// tabProfiles
			// 
			this.tabProfiles.Controls.Add(this.lblProfiles_ActiveProfile);
			this.tabProfiles.Controls.Add(this.tabProfileOptions);
			this.tabProfiles.Controls.Add(this.lstProfiles_Games);
			this.tabProfiles.Location = new System.Drawing.Point(4, 25);
			this.tabProfiles.Name = "tabProfiles";
			this.tabProfiles.Size = new System.Drawing.Size(424, 379);
			this.tabProfiles.TabIndex = 1;
			this.tabProfiles.Text = "Profiles";
			// 
			// lblProfiles_ActiveProfile
			// 
			this.lblProfiles_ActiveProfile.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.lblProfiles_ActiveProfile.Location = new System.Drawing.Point(160, 8);
			this.lblProfiles_ActiveProfile.Name = "lblProfiles_ActiveProfile";
			this.lblProfiles_ActiveProfile.Size = new System.Drawing.Size(256, 16);
			this.lblProfiles_ActiveProfile.TabIndex = 12;
			this.lblProfiles_ActiveProfile.Text = "Title";
			this.lblProfiles_ActiveProfile.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// tabProfileOptions
			// 
			this.tabProfileOptions.Controls.Add(this.tabProfiles_GameSettings);
			this.tabProfileOptions.Controls.Add(this.tabProfiles_SnapSettings);
			this.tabProfileOptions.Controls.Add(this.tabProfiles_StatsSettings);
			this.tabProfileOptions.Location = new System.Drawing.Point(152, 24);
			this.tabProfileOptions.Name = "tabProfileOptions";
			this.tabProfileOptions.SelectedIndex = 0;
			this.tabProfileOptions.Size = new System.Drawing.Size(264, 344);
			this.tabProfileOptions.TabIndex = 11;
			// 
			// tabProfiles_GameSettings
			// 
			this.tabProfiles_GameSettings.Controls.Add(this.grpProfiles_GameSettings_Path);
			this.tabProfiles_GameSettings.Location = new System.Drawing.Point(4, 22);
			this.tabProfiles_GameSettings.Name = "tabProfiles_GameSettings";
			this.tabProfiles_GameSettings.Size = new System.Drawing.Size(256, 318);
			this.tabProfiles_GameSettings.TabIndex = 0;
			this.tabProfiles_GameSettings.Text = "Game Settings";
			// 
			// grpProfiles_GameSettings_Path
			// 
			this.grpProfiles_GameSettings_Path.Controls.Add(this.cmdProfiles_GameSettings_AutoDetect);
			this.grpProfiles_GameSettings_Path.Controls.Add(this.cmdProfiles_GameSettings_BrowseGamePath);
			this.grpProfiles_GameSettings_Path.Controls.Add(this.txtProfiles_GameSettings_Path);
			this.grpProfiles_GameSettings_Path.Location = new System.Drawing.Point(8, 8);
			this.grpProfiles_GameSettings_Path.Name = "grpProfiles_GameSettings_Path";
			this.grpProfiles_GameSettings_Path.Size = new System.Drawing.Size(248, 80);
			this.grpProfiles_GameSettings_Path.TabIndex = 2;
			this.grpProfiles_GameSettings_Path.TabStop = false;
			this.grpProfiles_GameSettings_Path.Text = "Path";
			// 
			// cmdProfiles_GameSettings_AutoDetect
			// 
			this.cmdProfiles_GameSettings_AutoDetect.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.cmdProfiles_GameSettings_AutoDetect.Location = new System.Drawing.Point(8, 16);
			this.cmdProfiles_GameSettings_AutoDetect.Name = "cmdProfiles_GameSettings_AutoDetect";
			this.cmdProfiles_GameSettings_AutoDetect.Size = new System.Drawing.Size(80, 24);
			this.cmdProfiles_GameSettings_AutoDetect.TabIndex = 4;
			this.cmdProfiles_GameSettings_AutoDetect.Text = "Auto Detect";
			this.cmdProfiles_GameSettings_AutoDetect.Click += new System.EventHandler(this.cmdProfiles_GameSettings_AutoDetect_Click);
			// 
			// cmdProfiles_GameSettings_BrowseGamePath
			// 
			this.cmdProfiles_GameSettings_BrowseGamePath.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.cmdProfiles_GameSettings_BrowseGamePath.Location = new System.Drawing.Point(184, 16);
			this.cmdProfiles_GameSettings_BrowseGamePath.Name = "cmdProfiles_GameSettings_BrowseGamePath";
			this.cmdProfiles_GameSettings_BrowseGamePath.Size = new System.Drawing.Size(56, 24);
			this.cmdProfiles_GameSettings_BrowseGamePath.TabIndex = 3;
			this.cmdProfiles_GameSettings_BrowseGamePath.Text = "Browse";
			this.cmdProfiles_GameSettings_BrowseGamePath.Click += new System.EventHandler(this.cmdProfiles_GameSettings_BrowseGamePath_Click);
			// 
			// txtProfiles_GameSettings_Path
			// 
			this.txtProfiles_GameSettings_Path.Location = new System.Drawing.Point(8, 48);
			this.txtProfiles_GameSettings_Path.Name = "txtProfiles_GameSettings_Path";
			this.txtProfiles_GameSettings_Path.Size = new System.Drawing.Size(232, 20);
			this.txtProfiles_GameSettings_Path.TabIndex = 2;
			this.txtProfiles_GameSettings_Path.Text = "Path To Game";
			// 
			// tabProfiles_SnapSettings
			// 
			this.tabProfiles_SnapSettings.Controls.Add(this.chkProfiles_SnapSettings_UseGlobal);
			this.tabProfiles_SnapSettings.Controls.Add(this.grpProfiles_SnapSettingsSub);
			this.tabProfiles_SnapSettings.Location = new System.Drawing.Point(4, 22);
			this.tabProfiles_SnapSettings.Name = "tabProfiles_SnapSettings";
			this.tabProfiles_SnapSettings.Size = new System.Drawing.Size(256, 318);
			this.tabProfiles_SnapSettings.TabIndex = 1;
			this.tabProfiles_SnapSettings.Text = "Snap Settings";
			// 
			// chkProfiles_SnapSettings_UseGlobal
			// 
			this.chkProfiles_SnapSettings_UseGlobal.Location = new System.Drawing.Point(8, 8);
			this.chkProfiles_SnapSettings_UseGlobal.Name = "chkProfiles_SnapSettings_UseGlobal";
			this.chkProfiles_SnapSettings_UseGlobal.Size = new System.Drawing.Size(128, 16);
			this.chkProfiles_SnapSettings_UseGlobal.TabIndex = 0;
			this.chkProfiles_SnapSettings_UseGlobal.Text = "Use Global Settings";
			this.chkProfiles_SnapSettings_UseGlobal.CheckedChanged += new System.EventHandler(this.chkProfiles_SnapSettings_UseGlobal_CheckedChanged);
			// 
			// grpProfiles_SnapSettingsSub
			// 
			this.grpProfiles_SnapSettingsSub.Controls.Add(this.grpProfiles_SnapSettings_SnapDir);
			this.grpProfiles_SnapSettingsSub.Controls.Add(this.grpProfiles_SnapSettings_Delay);
			this.grpProfiles_SnapSettingsSub.Controls.Add(this.chkProfiles_SnapSettings_SingleDisplay);
			this.grpProfiles_SnapSettingsSub.Controls.Add(this.chkProfiles_SnapSettings_Enabled);
			this.grpProfiles_SnapSettingsSub.Location = new System.Drawing.Point(8, 24);
			this.grpProfiles_SnapSettingsSub.Name = "grpProfiles_SnapSettingsSub";
			this.grpProfiles_SnapSettingsSub.Size = new System.Drawing.Size(240, 288);
			this.grpProfiles_SnapSettingsSub.TabIndex = 1;
			this.grpProfiles_SnapSettingsSub.TabStop = false;
			// 
			// grpProfiles_SnapSettings_SnapDir
			// 
			this.grpProfiles_SnapSettings_SnapDir.Controls.Add(this.txtProfiles_SnapSettings_SnapDir);
			this.grpProfiles_SnapSettings_SnapDir.Controls.Add(this.cmdProfiles_BrowseSnapDir);
			this.grpProfiles_SnapSettings_SnapDir.Location = new System.Drawing.Point(8, 96);
			this.grpProfiles_SnapSettings_SnapDir.Name = "grpProfiles_SnapSettings_SnapDir";
			this.grpProfiles_SnapSettings_SnapDir.Size = new System.Drawing.Size(224, 48);
			this.grpProfiles_SnapSettings_SnapDir.TabIndex = 8;
			this.grpProfiles_SnapSettings_SnapDir.TabStop = false;
			this.grpProfiles_SnapSettings_SnapDir.Text = "Snap Directory";
			// 
			// txtProfiles_SnapSettings_SnapDir
			// 
			this.txtProfiles_SnapSettings_SnapDir.Location = new System.Drawing.Point(8, 16);
			this.txtProfiles_SnapSettings_SnapDir.Name = "txtProfiles_SnapSettings_SnapDir";
			this.txtProfiles_SnapSettings_SnapDir.Size = new System.Drawing.Size(144, 20);
			this.txtProfiles_SnapSettings_SnapDir.TabIndex = 5;
			this.txtProfiles_SnapSettings_SnapDir.Text = "";
			// 
			// cmdProfiles_BrowseSnapDir
			// 
			this.cmdProfiles_BrowseSnapDir.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.cmdProfiles_BrowseSnapDir.Location = new System.Drawing.Point(160, 16);
			this.cmdProfiles_BrowseSnapDir.Name = "cmdProfiles_BrowseSnapDir";
			this.cmdProfiles_BrowseSnapDir.Size = new System.Drawing.Size(56, 24);
			this.cmdProfiles_BrowseSnapDir.TabIndex = 6;
			this.cmdProfiles_BrowseSnapDir.Text = "Browse";
			this.cmdProfiles_BrowseSnapDir.Click += new System.EventHandler(this.cmdProfiles_BrowseSnapDir_Click);
			// 
			// grpProfiles_SnapSettings_Delay
			// 
			this.grpProfiles_SnapSettings_Delay.Controls.Add(this.udProfiles_SnapSettings_Delay);
			this.grpProfiles_SnapSettings_Delay.Location = new System.Drawing.Point(8, 40);
			this.grpProfiles_SnapSettings_Delay.Name = "grpProfiles_SnapSettings_Delay";
			this.grpProfiles_SnapSettings_Delay.Size = new System.Drawing.Size(224, 48);
			this.grpProfiles_SnapSettings_Delay.TabIndex = 7;
			this.grpProfiles_SnapSettings_Delay.TabStop = false;
			this.grpProfiles_SnapSettings_Delay.Text = "Delay";
			// 
			// udProfiles_SnapSettings_Delay
			// 
			this.udProfiles_SnapSettings_Delay.Location = new System.Drawing.Point(8, 16);
			this.udProfiles_SnapSettings_Delay.Maximum = new System.Decimal(new int[] {
																						  10000,
																						  0,
																						  0,
																						  0});
			this.udProfiles_SnapSettings_Delay.Name = "udProfiles_SnapSettings_Delay";
			this.udProfiles_SnapSettings_Delay.Size = new System.Drawing.Size(208, 20);
			this.udProfiles_SnapSettings_Delay.TabIndex = 22;
			this.udProfiles_SnapSettings_Delay.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// chkProfiles_SnapSettings_SingleDisplay
			// 
			this.chkProfiles_SnapSettings_SingleDisplay.Location = new System.Drawing.Point(136, 16);
			this.chkProfiles_SnapSettings_SingleDisplay.Name = "chkProfiles_SnapSettings_SingleDisplay";
			this.chkProfiles_SnapSettings_SingleDisplay.Size = new System.Drawing.Size(96, 16);
			this.chkProfiles_SnapSettings_SingleDisplay.TabIndex = 1;
			this.chkProfiles_SnapSettings_SingleDisplay.Text = "Single Display";
			// 
			// chkProfiles_SnapSettings_Enabled
			// 
			this.chkProfiles_SnapSettings_Enabled.Location = new System.Drawing.Point(8, 16);
			this.chkProfiles_SnapSettings_Enabled.Name = "chkProfiles_SnapSettings_Enabled";
			this.chkProfiles_SnapSettings_Enabled.Size = new System.Drawing.Size(72, 16);
			this.chkProfiles_SnapSettings_Enabled.TabIndex = 0;
			this.chkProfiles_SnapSettings_Enabled.Text = "Enabled";
			// 
			// tabProfiles_StatsSettings
			// 
			this.tabProfiles_StatsSettings.Controls.Add(this.chkProfiles_StatsSettings_UseGlobal);
			this.tabProfiles_StatsSettings.Controls.Add(this.grpProfiles_StatsSettingsSub);
			this.tabProfiles_StatsSettings.Controls.Add(this.cmdProfiles_StatsSettings_View);
			this.tabProfiles_StatsSettings.Controls.Add(this.cmdProfiles_StatsSettings_Reset);
			this.tabProfiles_StatsSettings.Location = new System.Drawing.Point(4, 22);
			this.tabProfiles_StatsSettings.Name = "tabProfiles_StatsSettings";
			this.tabProfiles_StatsSettings.Size = new System.Drawing.Size(256, 318);
			this.tabProfiles_StatsSettings.TabIndex = 2;
			this.tabProfiles_StatsSettings.Text = "Stats Settings";
			// 
			// chkProfiles_StatsSettings_UseGlobal
			// 
			this.chkProfiles_StatsSettings_UseGlobal.Location = new System.Drawing.Point(8, 7);
			this.chkProfiles_StatsSettings_UseGlobal.Name = "chkProfiles_StatsSettings_UseGlobal";
			this.chkProfiles_StatsSettings_UseGlobal.Size = new System.Drawing.Size(128, 16);
			this.chkProfiles_StatsSettings_UseGlobal.TabIndex = 2;
			this.chkProfiles_StatsSettings_UseGlobal.Text = "Use Global Settings";
			this.chkProfiles_StatsSettings_UseGlobal.CheckedChanged += new System.EventHandler(this.chkProfiles_StatsSettings_UseGlobal_CheckedChanged);
			// 
			// grpProfiles_StatsSettingsSub
			// 
			this.grpProfiles_StatsSettingsSub.Controls.Add(this.chkProfiles_StatsSettings_Enabled);
			this.grpProfiles_StatsSettingsSub.Location = new System.Drawing.Point(8, 23);
			this.grpProfiles_StatsSettingsSub.Name = "grpProfiles_StatsSettingsSub";
			this.grpProfiles_StatsSettingsSub.Size = new System.Drawing.Size(240, 257);
			this.grpProfiles_StatsSettingsSub.TabIndex = 3;
			this.grpProfiles_StatsSettingsSub.TabStop = false;
			// 
			// chkProfiles_StatsSettings_Enabled
			// 
			this.chkProfiles_StatsSettings_Enabled.Location = new System.Drawing.Point(8, 16);
			this.chkProfiles_StatsSettings_Enabled.Name = "chkProfiles_StatsSettings_Enabled";
			this.chkProfiles_StatsSettings_Enabled.Size = new System.Drawing.Size(72, 16);
			this.chkProfiles_StatsSettings_Enabled.TabIndex = 0;
			this.chkProfiles_StatsSettings_Enabled.Text = "Enabled";
			// 
			// cmdProfiles_StatsSettings_View
			// 
			this.cmdProfiles_StatsSettings_View.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.cmdProfiles_StatsSettings_View.Location = new System.Drawing.Point(8, 288);
			this.cmdProfiles_StatsSettings_View.Name = "cmdProfiles_StatsSettings_View";
			this.cmdProfiles_StatsSettings_View.Size = new System.Drawing.Size(72, 24);
			this.cmdProfiles_StatsSettings_View.TabIndex = 7;
			this.cmdProfiles_StatsSettings_View.Text = "View";
			this.cmdProfiles_StatsSettings_View.Click += new System.EventHandler(this.cmdProfiles_StatsSettings_View_Click);
			// 
			// cmdProfiles_StatsSettings_Reset
			// 
			this.cmdProfiles_StatsSettings_Reset.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.cmdProfiles_StatsSettings_Reset.Location = new System.Drawing.Point(176, 288);
			this.cmdProfiles_StatsSettings_Reset.Name = "cmdProfiles_StatsSettings_Reset";
			this.cmdProfiles_StatsSettings_Reset.Size = new System.Drawing.Size(72, 24);
			this.cmdProfiles_StatsSettings_Reset.TabIndex = 6;
			this.cmdProfiles_StatsSettings_Reset.Text = "Reset";
			this.cmdProfiles_StatsSettings_Reset.Click += new System.EventHandler(this.cmdProfiles_StatsSettings_Reset_Click);
			// 
			// tabAbout
			// 
			this.tabAbout.Controls.Add(this.rtxtAbout);
			this.tabAbout.Controls.Add(this.picAboutIcon);
			this.tabAbout.Location = new System.Drawing.Point(4, 25);
			this.tabAbout.Name = "tabAbout";
			this.tabAbout.Size = new System.Drawing.Size(424, 379);
			this.tabAbout.TabIndex = 2;
			this.tabAbout.Text = "About";
			// 
			// rtxtAbout
			// 
			this.rtxtAbout.BackColor = System.Drawing.SystemColors.Control;
			this.rtxtAbout.Location = new System.Drawing.Point(72, 8);
			this.rtxtAbout.Name = "rtxtAbout";
			this.rtxtAbout.ReadOnly = true;
			this.rtxtAbout.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.ForcedVertical;
			this.rtxtAbout.Size = new System.Drawing.Size(344, 360);
			this.rtxtAbout.TabIndex = 3;
			this.rtxtAbout.Text = "";
			// 
			// picAboutIcon
			// 
			this.picAboutIcon.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.picAboutIcon.Image = ((System.Drawing.Image)(resources.GetObject("picAboutIcon.Image")));
			this.picAboutIcon.Location = new System.Drawing.Point(8, 8);
			this.picAboutIcon.Name = "picAboutIcon";
			this.picAboutIcon.Size = new System.Drawing.Size(56, 56);
			this.picAboutIcon.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
			this.picAboutIcon.TabIndex = 2;
			this.picAboutIcon.TabStop = false;
			// 
			// tabLog
			// 
			this.tabLog.Controls.Add(this.rtxtLog);
			this.tabLog.Location = new System.Drawing.Point(4, 25);
			this.tabLog.Name = "tabLog";
			this.tabLog.Size = new System.Drawing.Size(424, 379);
			this.tabLog.TabIndex = 3;
			this.tabLog.Text = "Log";
			// 
			// rtxtLog
			// 
			this.rtxtLog.Location = new System.Drawing.Point(4, 8);
			this.rtxtLog.Name = "rtxtLog";
			this.rtxtLog.ReadOnly = true;
			this.rtxtLog.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.ForcedVertical;
			this.rtxtLog.Size = new System.Drawing.Size(416, 368);
			this.rtxtLog.TabIndex = 11;
			this.rtxtLog.Text = "";
			// 
			// frmMain
			// 
			this.AutoScale = false;
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(434, 447);
			this.Controls.Add(this.tabOptions);
			this.Controls.Add(this.cmdCancel);
			this.Controls.Add(this.cmdOK);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.Name = "frmMain";
			this.Text = "Smile!";
			this.WindowState = System.Windows.Forms.FormWindowState.Minimized;
			this.Load += new System.EventHandler(this.frmMain_Load);
			this.Closed += new System.EventHandler(this.frmMain_Closed);
			this.VisibleChanged += new System.EventHandler(this.frmMain_VisibleChanged);
			this.tabOptions.ResumeLayout(false);
			this.tabGeneral.ResumeLayout(false);
			this.tabGeneralOptions.ResumeLayout(false);
			this.tabGeneral_GlobalSnapSettings.ResumeLayout(false);
			this.grpGeneral_SnapSettings_ImageFormat.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.tbGeneral_SnapSettings_Quality)).EndInit();
			this.grpGeneral_SnapSettings_SnapDir.ResumeLayout(false);
			this.grpGeneral_SnapSettings_Delay.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.udGeneral_SnapSettings_Delay)).EndInit();
			this.tabGeneral_GlobalStatsSettings.ResumeLayout(false);
			this.tabProfiles.ResumeLayout(false);
			this.tabProfileOptions.ResumeLayout(false);
			this.tabProfiles_GameSettings.ResumeLayout(false);
			this.grpProfiles_GameSettings_Path.ResumeLayout(false);
			this.tabProfiles_SnapSettings.ResumeLayout(false);
			this.grpProfiles_SnapSettingsSub.ResumeLayout(false);
			this.grpProfiles_SnapSettings_SnapDir.ResumeLayout(false);
			this.grpProfiles_SnapSettings_Delay.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.udProfiles_SnapSettings_Delay)).EndInit();
			this.tabProfiles_StatsSettings.ResumeLayout(false);
			this.grpProfiles_StatsSettingsSub.ResumeLayout(false);
			this.tabAbout.ResumeLayout(false);
			this.tabLog.ResumeLayout(false);
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
			Hide();
		}

		private void frmMain_Closed(object sender, System.EventArgs e)
		{
			SaveSettings();
		}

		private void frmMain_Resize(object sender, System.EventArgs e)
		{
			if (FormWindowState.Minimized == WindowState)
				Hide();
		}

		private void mnuOpen_Click(object sender, System.EventArgs e)
		{
			if(this.AllowShow)
			{
				Show();
				WindowState = FormWindowState.Normal;
			}
		}

		private void cmdCancel_Click(object sender, System.EventArgs e)
		{
			Hide();
			SaveSettings();
		}

		// Check/Save new settings
		private void cmdOK_Click(object sender, System.EventArgs e)
		{
			// Global Snap Settings
			Settings.SnapSettings.Enabled = chkGeneral_SnapSettings_Enabled.Checked;
			Settings.SnapSettings.SingleDisplay = chkGeneral_SnapSettings_SingleDisplay.Checked;
			Settings.SnapSettings.Delay = (int)udGeneral_SnapSettings_Delay.Value;
			Settings.SnapSettings.SnapDir = txtGeneral_SnapSettings_SnapDir.Text;
			Settings.SnapSettings.Quality = tbGeneral_SnapSettings_Quality.Value;
			Settings.SnapSettings.Encoder = cbGeneral_SnapSettings_ImageFormat.SelectedItem.ToString();

			// Global Stats Settings
			Settings.StatsSettings.Enabled = chkGeneral_StatsSettings_Enabled.Checked;

			// Copy profiles
			// Copy last active profile
			SaveProfile(TempProfiles[ActiveTempProfile]);
			for(int i = 0; i < Profiles.Length; i++)
			{
				CopyProfile(TempProfiles[i], Profiles[i]);
			}

			SaveSettings();

			UpdateEncoder();

			CheckEnabled();

			Hide();
		}

		private void tbGeneral_SnapSettings_Quality_Scroll(object sender, System.EventArgs e)
		{
			lblGeneral_SnapSettings_Quality.Text = "Quality " + tbGeneral_SnapSettings_Quality.Value + "% (100% = Clearest):";
		}

		private void cmdProfiles_StatsSettings_View_Click(object sender, System.EventArgs e)
		{
			this.TopMost = true;
			Form stats = new frmViewStats(Profiles, ActiveTempProfile);
			stats.ShowDialog();
			stats.Dispose();
			this.TopMost = false;
		}

		private void cmdGeneral_StatsSettings_ViewStats_Click(object sender, System.EventArgs e)
		{
			this.TopMost = true;
			Form stats = new frmViewStats(Profiles, 0);
			stats.ShowDialog();
			stats.Dispose();
			this.TopMost = false;
		}

		private void cmdProfiles_StatsSettings_Reset_Click(object sender, System.EventArgs e)
		{
			if(MessageBox.Show("Are you sure you want to reset the " + Profiles[ActiveTempProfile].ProfileName + " statistics?\n\nNote: This will also cause your settings to save!", "Question!", MessageBoxButtons.YesNo).Equals(DialogResult.Yes))
			{
				Profiles[ActiveTempProfile].ResetStats();
				SaveSettings();
			}
		}

		// Reset gun statistics
		private void cmdResetStats_Click(object sender, System.EventArgs e)
		{
			if(MessageBox.Show("Are you sure you want to reset the statistics?\n\nNote: This will also cause your settings to save!", "Question!", MessageBoxButtons.YesNo).Equals(DialogResult.Yes))
			{
				foreach(CProfile profile in Profiles)
				{
					profile.ResetStats();
				}
				SaveSettings();
			}
		}

		private void cmdGeneral_SnapSettings_BrowseSnapDir_Click(object sender, System.EventArgs e)
		{
			dlgBrowseDir.SelectedPath = txtGeneral_SnapSettings_SnapDir.Text;
			dlgBrowseDir.ShowDialog();
			txtGeneral_SnapSettings_SnapDir.Text = dlgBrowseDir.SelectedPath;
		}

		private void cmdProfiles_GameSettings_BrowseGamePath_Click(object sender, System.EventArgs e)
		{
			dlgBrowseDir.SelectedPath = txtProfiles_GameSettings_Path.Text;
			dlgBrowseDir.ShowDialog();
			txtProfiles_GameSettings_Path.Text = dlgBrowseDir.SelectedPath;
		}

		private void cmdProfiles_BrowseSnapDir_Click(object sender, System.EventArgs e)
		{
			dlgBrowseDir.SelectedPath = txtProfiles_SnapSettings_SnapDir.Text;
			dlgBrowseDir.ShowDialog();
			txtProfiles_SnapSettings_SnapDir.Text = dlgBrowseDir.SelectedPath;
		}

		private void cmdGeneral_StatsSettings_Reset_Click(object sender, System.EventArgs e)
		{
			if(MessageBox.Show("Are you sure you want to reset the statistics?\n\nNote: This will also cause your settings to save!", "Question!", MessageBoxButtons.YesNo).Equals(DialogResult.Yes))
			{
				foreach(CProfile profile in Profiles)
				{
					profile.ResetStats();
					SaveSettings();
				}
			}
		}

		private void lstProfiles_Games_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			if(lstProfiles_Games.SelectedIndices.Count > 0)
			{
				SaveProfile(TempProfiles[ActiveTempProfile]);
				ActiveTempProfile = (int)lstProfiles_Games.SelectedItems[0].Tag;
				PopulateProfile(TempProfiles[ActiveTempProfile]);
			}
		}

		private void frmMain_VisibleChanged(object sender, System.EventArgs e)
		{
			if(this.Visible == true)
			{
				for(int i=0;i<Profiles.Length;i++)
				{
					CopyProfile(Profiles[i], TempProfiles[i]);
				}
			}
		}

		private void cmdProfiles_GameSettings_AutoDetect_Click(object sender, System.EventArgs e)
		{
			txtProfiles_GameSettings_Path.Text = TempProfiles[ActiveTempProfile].GetDefaultPath();
		}

		private void chkProfiles_SnapSettings_UseGlobal_CheckedChanged(object sender, System.EventArgs e)
		{
			grpProfiles_SnapSettingsSub.Enabled = !chkProfiles_SnapSettings_UseGlobal.Checked;
		}

		private void chkProfiles_StatsSettings_UseGlobal_CheckedChanged(object sender, System.EventArgs e)
		{
			grpProfiles_StatsSettingsSub.Enabled = !chkProfiles_StatsSettings_UseGlobal.Checked;
		}

		//////////////////////////////////////////////////////////////////////////////////////////////
		/// Notify Icon (system tray) Methods
		//////////////////////////////////////////////////////////////////////////////////////////////

		private void mnuViewStats_Click(object sender, System.EventArgs e)
		{
			this.AllowShow = false;
			Form stats = new frmViewStats(Profiles, 0);
			stats.ShowDialog();
			stats.Dispose();
			this.AllowShow = true;
		}

		private void mnuEnableStats_Click(object sender, System.EventArgs e)
		{
			mnuEnableStats.Checked = !mnuEnableStats.Checked;
			EnableStats = mnuEnableStats.Checked;
			CheckEnabled();
		}

		private void mnuOpenSnapDir_Click(object sender, System.EventArgs e)
		{
			Process.Start("explorer.exe", Settings.SnapSettings.SnapDir);
		}

		private void mnuAbout_Click(object sender, System.EventArgs e)
		{
			PopulateOptions();
			tabOptions.SelectedIndex = 2;
			Show();
		}

		private void mnuExit_Click(object sender, System.EventArgs e)
		{
			Close();
		}

		private void mnuEnableSnaps_Click(object sender, System.EventArgs e)
		{
			mnuEnableSnaps.Checked = !mnuEnableSnaps.Checked;
			EnableSnaps = mnuEnableSnaps.Checked;
			CheckEnabled();
		}

		private void notifyIcon_DoubleClick(object sender, System.EventArgs e)
		{
			// TODO: Add toggle
			if(this.Visible)
				Hide();
			else
			{
				this.TopMost = true;
				Show();
				WindowState = FormWindowState.Normal;
				this.TopMost = false;
			}
		}

		//////////////////////////////////////////////////////////////////////////////////////////////
		/// Timer Routines
		//////////////////////////////////////////////////////////////////////////////////////////////

		// Main Statistics/Screenshot Routine
		public static String error;
		private void TimerCheckConsoleLog_Tick(object sender, System.EventArgs e)
		{
			if(ActiveProfile == null)
				return;

			if(encoder == null)
				return;
			
			if(!ActiveProfile.IsOpen())
				return;

			if(!EnableSnaps && !EnableStats)
				return;

			try
			{
				ActiveProfile.Parse();
			}
			catch(Exception ex)
			{
				AddLogMessage("Error: Parse() failed unexpectedly for " + ActiveProfile.ProfileName + "\"" + ex.Message + "\"");
			}
			if(EnableSnaps)
			{
				if(ActiveProfile.NewSnaps)
				{
					try
					{
						Thread.Sleep(ActiveProfile.SnapSettings.UseGlobal ? Settings.SnapSettings.Delay : ActiveProfile.SnapSettings.Delay);
						Image img = ScreenCapture.GetDesktopImage(Settings.SnapSettings.SingleDisplay);
						Graphics g = Graphics.FromImage(img);

						g.DrawString(encoderstring, encoderfont , new SolidBrush(Color.FromArgb(75, Color.White)), img.Width - g.MeasureString(encoderstring, encoderfont).Width - 5, img.Height - g.MeasureString(encoderstring, encoderfont).Height - 5);

						String dir = ActiveProfile.SnapSettings.UseGlobal ? Settings.SnapSettings.SnapDir + @"\" + ActiveProfile.SnapName : ActiveProfile.SnapSettings.SnapDir;
						if(!Directory.Exists(dir))
						{
							Directory.CreateDirectory(dir);
							AddLogMessage("Created Directory: " + dir);
						}
						String file = dir + @"\kill-" + DateTime.Now.ToString("yyyyMMdd_HHmmss_ffff") + encoderext;
						img.Save(file , encoder, encoderParams);
						AddLogMessage("Saved Image to: " + file);
						ActiveProfile.NewSnaps = false;
						g.Dispose();
						img.Dispose();
					}
					catch
					{
						AddLogMessage("Error Capturing/Saving Image (Check Paths/Encoders?)");
					}
				}
			}
		}

		// Misc Timer work
		private void TimerMisc_Tick(object sender, System.EventArgs e)
		{
			if(ActiveProfile != null)
			{
				if(!ActiveProfile.CheckActive())
				{
					TimerCheckConsoleLog.Stop();
					ActiveProfile.Close();
					ActiveProfile = null;
					SaveSettings();
					return;
				}
				if(!ActiveProfile.IsOpen())
				{
					ActiveProfile.Open();
					if(ActiveProfile.IsOpen())
						AddLogMessage("Begining Session For: " + ActiveProfile.ProfileName);
					else
					{
						AddLogMessage("Error Starting Session For: " + ActiveProfile.ProfileName);
						AddLogMessage(frmMain.error);
						frmMain.error = "";
					}
				}

				if(ActiveProfile.NewStats)
				{
					SaveSettings();
					ActiveProfile.NewStats = false;
				}	

				if(!ActiveProfile.StatsSettings.Enabled)
					mnuEnableStats.Checked = EnableStats = false;
				if(!ActiveProfile.SnapSettings.Enabled)
					mnuEnableSnaps.Checked = EnableSnaps = false;
			}
			else
			{
				foreach(CProfile profile in Profiles)
				{
					if(profile.CheckActive())
					{
						ActiveProfile = profile;
						CheckEnabled();
						TimerCheckConsoleLog.Start();
						break;
					}
				}
			}
		}


		//////////////////////////////////////////////////////////////////////////////////////////////
		/// Helper Methods
		//////////////////////////////////////////////////////////////////////////////////////////////


		private void AddLogMessage(string msg)
		{
			rtxtLog.SelectionStart = rtxtLog.Text.Length;
			rtxtLog.SelectedText = msg + "\n";
		}

		// Populate Options Form
		private void PopulateOptions()
		{
			// Global Snap Settings
			chkGeneral_SnapSettings_Enabled.Checked = Settings.SnapSettings.Enabled;
			chkGeneral_SnapSettings_SingleDisplay.Checked = Settings.SnapSettings.SingleDisplay;
			udGeneral_SnapSettings_Delay.Value = Settings.SnapSettings.Delay;
			txtGeneral_SnapSettings_SnapDir.Text = Settings.SnapSettings.SnapDir;
			tbGeneral_SnapSettings_Quality.Value = Settings.SnapSettings.Quality;
			lblGeneral_SnapSettings_Quality.Text = "Quality " + tbGeneral_SnapSettings_Quality.Value + "% (100% = Clearest):";
			cbGeneral_SnapSettings_ImageFormat.SelectedItem = Settings.SnapSettings.Encoder;

			// Global Stats Settings
			chkGeneral_StatsSettings_Enabled.Checked = Settings.StatsSettings.Enabled;

			// Load first profile options
			PopulateProfile(Profiles[0]);
			ActiveTempProfile = 0;
		}

		private void PopulateProfile(CProfile p)
		{
			// Show title
			lblProfiles_ActiveProfile.Text = p.ProfileName;

			// Select First Options tab
			tabProfileOptions.SelectedIndex = 0;

			// Game Settings
			txtProfiles_GameSettings_Path.Text = p.path;

			// SnapSettings
			chkProfiles_SnapSettings_UseGlobal.Checked = p.SnapSettings.UseGlobal;
			grpProfiles_SnapSettingsSub.Enabled = !p.SnapSettings.UseGlobal;
			chkProfiles_SnapSettings_Enabled.Checked = p.SnapSettings.Enabled;
			chkProfiles_SnapSettings_SingleDisplay.Checked = p.SnapSettings.SingleDisplay;
			txtProfiles_SnapSettings_SnapDir.Text = p.SnapSettings.SnapDir;
			udProfiles_SnapSettings_Delay.Value = p.SnapSettings.Delay;

			// StatsSettings
			chkProfiles_StatsSettings_UseGlobal.Checked = p.StatsSettings.UseGlobal;
			chkProfiles_StatsSettings_Enabled.Checked = p.StatsSettings.Enabled;
			grpProfiles_StatsSettingsSub.Enabled = !p.StatsSettings.UseGlobal;
		}

		private void SaveProfile(CProfile p)
		{
			// Game Settings
			p.path = txtProfiles_GameSettings_Path.Text;

			// SnapSettings
			p.SnapSettings.UseGlobal = chkProfiles_SnapSettings_UseGlobal.Checked;
			p.SnapSettings.Enabled = chkProfiles_SnapSettings_Enabled.Checked;
			p.SnapSettings.SingleDisplay = chkProfiles_SnapSettings_SingleDisplay.Checked;
			p.SnapSettings.SnapDir = txtProfiles_SnapSettings_SnapDir.Text;
			p.SnapSettings.Delay = (int)udProfiles_SnapSettings_Delay.Value;

			// StatsSettings
			p.StatsSettings.UseGlobal = chkProfiles_StatsSettings_UseGlobal.Checked;
			p.StatsSettings.Enabled = chkProfiles_StatsSettings_Enabled.Checked;
		}

		private void CheckEnabled()
		{
			if(ActiveProfile == null)
				return;

			// Check Snaps Enabled
			if(ActiveProfile.SnapSettings.UseGlobal && !Settings.SnapSettings.Enabled) 
				mnuEnableSnaps.Checked  = ActiveProfile.EnableSnaps = EnableSnaps = false;
			else if(!ActiveProfile.SnapSettings.Enabled)
				mnuEnableSnaps.Checked  = ActiveProfile.EnableSnaps = EnableSnaps = false;
			else
				ActiveProfile.EnableSnaps = EnableSnaps;

			// Check StatsEnabled
			if(ActiveProfile.StatsSettings.UseGlobal && !Settings.StatsSettings.Enabled) 
				mnuEnableStats.Checked  = ActiveProfile.EnableStats = EnableStats = false;
			else if(!ActiveProfile.StatsSettings.Enabled)
				mnuEnableStats.Checked  = ActiveProfile.EnableStats = EnableStats = false;
			else
				ActiveProfile.EnableStats = EnableStats;
		}

		private void CopyProfile(CProfile a, CProfile b)
		{
			// Game Settings
			b.path = a.path;

			// SnapSettings
			b.SnapSettings.UseGlobal = a.SnapSettings.UseGlobal;
			b.SnapSettings.Enabled = a.SnapSettings.Enabled;
			b.SnapSettings.SingleDisplay = a.SnapSettings.SingleDisplay;
			b.SnapSettings.SnapDir = a.SnapSettings.SnapDir;
			b.SnapSettings.Delay = a.SnapSettings.Delay;

			// StatsSettings
			b.StatsSettings.UseGlobal = a.StatsSettings.UseGlobal;
			b.StatsSettings.Enabled = a.StatsSettings.Enabled;
		}


		private void UpdateEncoder()
		{
			try
			{
				EncoderParameter qualityParam = new EncoderParameter (Encoder.Quality, this.Settings.SnapSettings.Quality);
				encoder = ScreenCapture.GetEncoderInfo(Settings.SnapSettings.Encoder);
				encoderParams = new EncoderParameters(1); 
				encoderParams.Param[0] = qualityParam;

				encoderfont = new Font("Verdana", 10);
				encoderstring = "Captured with Smile! -- www.Kudlacz.com";
				encoderext = Path.GetExtension(encoder.FilenameExtension.Split(';')[0]).ToLower();

				AddLogMessage("Image Encoder Created");
			}
			catch
			{
				encoder = null;
				AddLogMessage("Error creating appropriate Image Encoder for: " + Settings.SnapSettings.Encoder);
			}
		}

		// Load settings from xml
		private bool LoadSettings()
		{
			String FileName = Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath) + @"\" + Path.GetFileNameWithoutExtension(System.Windows.Forms.Application.ExecutablePath) + ".dat";
			try
			{
				if(File.Exists(FileName))
				{
					XmlSerializer serializer;
					StreamReader reader;
					Smile_t Data = new Smile_t();

					// Load
					serializer =  new XmlSerializer(Data.GetType());
					reader = new StreamReader(FileName);
					Data = (Smile_t) serializer.Deserialize(reader);
					reader.Close();
					Settings = Data.Settings;
					Profiles = Data.Profiles;

					foreach(CProfile profile in Profiles)
					{
						profile.fromXMLOperations();
					}

				} 
				else 
				{
					Settings.SnapSettings.Enabled = Settings.StatsSettings.Enabled = true;
					Settings.SnapSettings.SnapDir = Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath) + @"\Snaps";
					Settings.SnapSettings.Delay = 75;
					Settings.SnapSettings.SingleDisplay = true;
					Settings.SnapSettings.Quality = 85;
					Settings.SnapSettings.Encoder = "image/jpeg";
					SaveSettings();
				}
			} 
			catch ( Exception e ) 
			{
				Ex.DumpException(e);
			}

			return true;
		}


		// Save Settings to xml
		private bool SaveSettings()
		{	
			String FileName = Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath) + @"\" + Path.GetFileNameWithoutExtension(System.Windows.Forms.Application.ExecutablePath) + ".dat";
			try
			{
				Smile_t Data = new Smile_t();
				foreach(CProfile profile in Profiles)
				{
					profile.toXMLOperations();
				}
				Data.Settings = Settings;
				Data.Profiles = Profiles;

				XmlSerializer serializer;
				TextWriter writer;

				// Save 
				serializer =  new XmlSerializer(Data.GetType());
				writer = new StreamWriter(FileName);
				serializer.Serialize(writer, Data);
				writer.Close();
			} 
			catch ( Exception e ) 
			{
				Ex.DumpException(e);
			}
			return true;
		}

		public static System.Array ResizeArray(System.Array oldArray, Type type, int newSize) 
		{
			int oldSize = oldArray == null ? 0 : oldArray.Length;
			//System.Type elementType = oldArray.GetType().GetElementType();
			//System.Array newArray = System.Array.CreateInstance(elementType,newSize);
			System.Array newArray = System.Array.CreateInstance(type,newSize);
			int preserveLength = System.Math.Min(oldSize,newSize);
			if (preserveLength > 0)
				System.Array.Copy (oldArray,newArray,preserveLength);
			return newArray; 
		}

		private void AddProfiles(ref CProfile [] p)
		{
			Type [] ProfileTypes =  new Type [ ] 
			{ 
				typeof(CProfileCounterStrikeSource), 
				typeof(CProfileHalfLife2Deathmatch),
				typeof(CProfileQuakeIIIArena),
				typeof(CProfileQuakeIIITeamArena),
				typeof(CProfileQ3OSP),
				typeof(CProfileEnemyTerritory),
				typeof(CProfileCounterStrike),
				typeof(CProfileDayofDefeat)
			};
			int length = p == null ? 0 : p.Length;
			bool found;
			foreach(Type type in ProfileTypes)
			{
				found = false;
				if(p != null)
				{
					foreach(CProfile profile in p)
					{
						if(profile.GetType() == type)
						{
							found = true;
							break;
						}
					}
				}
				if(!found)
				{
					p = (CProfile[])ResizeArray(p, typeof(CProfile), length + 1);
					p[length] = (CProfile) System.Activator.CreateInstance(type);
				
					// Default options
					p[length].SnapSettings.Enabled = p[length].StatsSettings.Enabled = true;
					p[length].SnapSettings.Delay = Settings.SnapSettings.Delay;
					p[length].SnapSettings.SingleDisplay = Settings.SnapSettings.SingleDisplay;
					p[length].SnapSettings.UseGlobal = p[length].StatsSettings.UseGlobal = true;
					length++;
				}
			}
		}
	}
}
