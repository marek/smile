/////////////////////////////////////////////////////////////////////////////
//
// Smile! -- Screenshot and Statistics Utility
// Copyright (c) 2005 Marek Kudlacz
//
// http://kudlacz.com
//
/////////////////////////////////////////////////////////////////////////////



using System;
using System.Drawing;
using System.Collections;
using System.Windows.Forms;
using System.IO;
using System.Data;
using System.Threading;
using System.Xml;
using System.Xml.Serialization;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.Text.RegularExpressions;
using Kudlacz.Web;
using Kudlacz.Hooks;

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
		private readonly object QueueLock = new object();
		private readonly object MsgLock = new object();
		private readonly object ProfileLock = new object();
		private readonly object FrameLock = new object();
		private readonly object SaveThreadCountLock = new object();
		private readonly object FileSaveLock = new object();
		private int NumSaveThreads;
		private TSArrayList SaveQueue;
		private TSArrayList MsgQueue;
		private long LastSnapTime;
		private int NextSnapDelay;
		private int NumFrames = 0;
		private int NextSnapNo;
		private int GlobalNextSnapNo = 0;
		private bool DisableHotkeys = false;
		private ArrayList extlist;
		private System.Threading.Timer TimerCheckConsoleLog;
		private TimerCallback TimerCheckConsoleLogDelegate;
		private System.Threading.Timer TimerSave;
		private TimerCallback TimerSaveDelegate;
		private GlobalHotKey HKCaptureDesktop;
		private GlobalKotKey HKCaptureWindow;
		private GlobalKotKey HKCaptureActiveProfile;
		private StreamWriter log;
		private bool closing = false;
		private VersionCheck vc;
		private long lastVersionCheck = 0;
		private long versionCheckDelay = 0;

		private readonly Type [] ProfileTypes =  new Type [ ] 
			{ 
				typeof(CProfileCounterStrikeSource), 
				typeof(CProfileHalfLife2Deathmatch),
				typeof(CProfileQuakeIIIArena),
				typeof(CProfileEnemyTerritory),
				typeof(CProfileCounterStrike),
				typeof(CProfileDayofDefeat),
				typeof(CProfileDayofDefeatSource),
				typeof(CProfileSourceDystopia),
				typeof(CProfileJediAcademy)
			};

		// form elements
		private System.Windows.Forms.NotifyIcon notifyIcon;
		private System.Windows.Forms.ContextMenu contextMenu;
		private System.ComponentModel.IContainer components;
		private System.Windows.Forms.MenuItem menuItem4;
		private System.Windows.Forms.MenuItem menuItem6;
		private System.Windows.Forms.Timer TimerMisc;
		private System.Windows.Forms.Button cmdOK;
		private System.Windows.Forms.Button cmdCancel;
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
		private System.Windows.Forms.GroupBox grpProfiles_StatsSettingsSub;
		private System.Windows.Forms.CheckBox chkProfiles_StatsSettings_UseGlobal;
		private System.Windows.Forms.CheckBox chkProfiles_SnapSettings_Enabled;
		private System.Windows.Forms.CheckBox chkProfiles_StatsSettings_Enabled;
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
		private System.Windows.Forms.CheckBox chkGeneral_SnapSettings_SaveBug;
		private System.Windows.Forms.Label lblProfiles_SnapSettings_SnapDelay;
		private System.Windows.Forms.Label lblProfiles_SnapSettings_NextSnapDelay;
		private System.Windows.Forms.NumericUpDown udProfiles_SnapSettings_NextSnapDelay;
		private System.Windows.Forms.Label lblProfiles_SnapSettings_SnapCount;
		private System.Windows.Forms.NumericUpDown udProfiles_SnapSettings_SnapCount;
		private System.Windows.Forms.Label lblProfiles_SnapSettings_MultiSnapDelay;
		private System.Windows.Forms.NumericUpDown udProfiles_SnapSettings_MultiSnapDelay;
		private System.Windows.Forms.GroupBox grpGeneral_SnapSettings_TimingAndParameters;
		private System.Windows.Forms.GroupBox grpProfiles_SnapSettings_TimingAndParameters;
		private System.Windows.Forms.NumericUpDown udGeneral_SnapSettings_NextSnapDelay;
		private System.Windows.Forms.GroupBox grpGeneral_SnapSettings_AnimationSettings;
		private System.Windows.Forms.NumericUpDown udGeneral_SnapSettings_AnimWidth;
		private System.Windows.Forms.Label lblGeneral_SnapSettings_AnimWidth;
		private System.Windows.Forms.NumericUpDown udGeneral_SnapSettings_AnimHeight;
		private System.Windows.Forms.Label lblGeneral_SnapSettings_AnimHeight;
		private System.Windows.Forms.CheckBox chkGeneral_SnapSettings_AnimOriginalDimentions;
		private System.Windows.Forms.CheckBox chkGeneral_SnapSettings_AnimUseMultiSnapDelay;
		private System.Windows.Forms.NumericUpDown udGeneral_SnapSettings_AnimFrameDelay;
		private System.Windows.Forms.Label lblGeneral_SnapSettings_AnimFrameDelay;
		private System.Windows.Forms.ComboBox cbProfiles_SnapSettings_SaveType;
		private System.Windows.Forms.Timer TimerMsg;
		private System.Windows.Forms.Label lblGeneral_SnapSettings_NextSnapDelay;
		private System.Windows.Forms.Label lblGeneral_SnapSettings_SnapDelay;
		private System.Windows.Forms.Label lblProfiles_SnapSettings_Save;
		private System.Windows.Forms.CheckBox chkGeneral_SnapSettings_AnimOptimizePalette;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.Label lblProfiles_SnapSettings_Contrast;
		private System.Windows.Forms.TrackBar tbProfiles_SnapSettings_Contrast;
		private System.Windows.Forms.Label lblProfiles_SnapSettings_Gamma;
		private System.Windows.Forms.TrackBar tbProfiles_SnapSettings_Gamma;
		private System.Windows.Forms.Label lblProfiles_SnapSettings_Brightness;
		private System.Windows.Forms.TrackBar tbProfiles_SnapSettings_Brightness;
		private System.Windows.Forms.NumericUpDown udGeneral_SnapSettings_SaveQueueSize;
		private System.Windows.Forms.Label lblGeneral_SnapSettings_SaveQueueSize;
		private System.Windows.Forms.Button cmdApply;
		private System.Windows.Forms.TabPage tabGeneral_HotKeys;
		private System.Windows.Forms.Label lblGeneral_HotKeys_CaptureDesktop;
		private System.Windows.Forms.Label lblGeneral_HotKeys_CaptureWindow;
		private System.Windows.Forms.Label lblGeneral_HotKeys_CaptureActiveProfile;
		private System.Windows.Forms.CheckBox chkGeneral_HotKeys_Enabled;
		private System.Windows.Forms.TextBox txtGeneral_HotKeys_CaptureWindow;
		private System.Windows.Forms.TextBox txtGeneral_HotKeys_CaptureDesktop;
		private System.Windows.Forms.TextBox txtGeneral_HotKeys_CaptureActiveProfile;
		private System.Windows.Forms.TabPage tabGeneral_Misc;
		private System.Windows.Forms.GroupBox grpGeneral_Misc_Updates;
		private System.Windows.Forms.ComboBox cbGeneral_Misc_CheckUpdates;
		private System.Windows.Forms.Label lblGeneral_Misc_CheckUpdates;
		private System.Windows.Forms.CheckBox chkGeneral_StatsSettings_Enabled;

		//////////////////////////////////////////////////////////////////////////////////////////////
		/// Form Methods
		//////////////////////////////////////////////////////////////////////////////////////////////
	
		public frmMain()
		{
			// Required for Windows Form Designer support
			InitializeComponent();

			// Init log file
			try
			{
				log = new StreamWriter(Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath) + "\\" + Path.GetFileNameWithoutExtension(System.Windows.Forms.Application.ExecutablePath) + ".log", true);
				log.WriteLine("\r\n\r\n-----Log Session Started: " + DateTime.Now.ToLongDateString() + "-----\r\n\r\n");
				log.Flush();
			}
			catch
			{
				MessageBox.Show("Error: Cannot open smiletray session log for writing.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
			}

			// Init MsgQueue
			this.MsgQueue = new TSArrayList(10);

			// Read Settings
			this.Settings = new Settings_t();
			this.LoadSettings();

			// Populate profiles
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
			extlist = new ArrayList();
			ImageCodecInfo [] encoders = ImageCodecInfo.GetImageEncoders();
			foreach(ImageCodecInfo enc in encoders)
			{
				cbGeneral_SnapSettings_ImageFormat.Items.Add(enc.MimeType);
				string[] e = enc.FilenameExtension.Split(';');
				foreach(string ext in e)
				{
					extlist.Add(Path.GetExtension(ext).ToLower());
				}
			}
			UpdateEncoder();
			extlist.Add(".avi");

			// Create Save Queue
			this.SaveQueue = new TSArrayList(10);

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
			rtxtAbout.SelectedText = "©2005 Marek Kudlacz\nhttp://www.kudlacz.com\n";
			rtxtAbout.SelectionFont = new Font("Verdana", 12, FontStyle.Bold|FontStyle.Underline);
			rtxtAbout.SelectedText = "                                                                                                            \n\n";
			rtxtAbout.SelectedText = "This is a program designed in C#/.NET to keep track of statistics, and to take \"kill\" screenshots when you actually manage to kill someone. Enjoy!\n";
			rtxtAbout.SelectedText = "\n\nSmile!, Copyright (C) 2005 Marek Kudlacz. Smile! comes with ABSOLUTELY NO WARRANTY. This is free software, and you are welcome to redistribute it under certain conditions; for details see bundled LICENSE.TXT.";
			rtxtAbout.SelectionStart = 0 ;
			
			// Save queue starts here because now we can take screenshots at any moment
			NumSaveThreads = 0;
			TimerSaveDelegate = new TimerCallback(TimerSave_Tick);
			TimerSave = new System.Threading.Timer(TimerSaveDelegate, null, 10, 100);
			AddLogMessage("SaveQueue starting up.");

			// Start key gooks
			GlobalKotKey.RegisterHook();
			AddLogMessage("Registering system keyboard hook.");
			DisableHotkeys = false;
			HKCaptureWindow.Shortcut = GlobalKotKey.StringToKeyCombo(Settings.HotKeySettings.HKWindow);
			HKCaptureDesktop.Shortcut = GlobalKotKey.StringToKeyCombo(Settings.HotKeySettings.HKDesktop);
			HKCaptureActiveProfile.Shortcut = GlobalKotKey.StringToKeyCombo(Settings.HotKeySettings.HKActiveProfile);

			// Set up version checker
			lastVersionCheck = Settings.MiscSettings.LastCheckTime;
			UpdateCheckDelay();
			vc = new VersionCheck("http://www.kudlacz.com/version.xml", "Smile");

			// Start timers
			TimerMisc.Start();
			TimerMsg.Start();

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
			this.HKCaptureDesktop = new smiletray.GlobalKotKey(this.components);
			this.HKCaptureWindow = new smiletray.GlobalKotKey(this.components);
			this.HKCaptureActiveProfile = new smiletray.GlobalKotKey(this.components);
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
			this.dlgBrowseDir = new System.Windows.Forms.FolderBrowserDialog();
			this.TimerMisc = new System.Windows.Forms.Timer(this.components);
			this.cmdOK = new System.Windows.Forms.Button();
			this.cmdCancel = new System.Windows.Forms.Button();
			this.lstProfiles_Games = new System.Windows.Forms.ListView();
			this.tabOptions = new System.Windows.Forms.TabControl();
			this.tabGeneral = new System.Windows.Forms.TabPage();
			this.tabGeneralOptions = new System.Windows.Forms.TabControl();
			this.tabGeneral_GlobalSnapSettings = new System.Windows.Forms.TabPage();
			this.grpGeneral_SnapSettings_AnimationSettings = new System.Windows.Forms.GroupBox();
			this.chkGeneral_SnapSettings_AnimOptimizePalette = new System.Windows.Forms.CheckBox();
			this.udGeneral_SnapSettings_AnimFrameDelay = new System.Windows.Forms.NumericUpDown();
			this.lblGeneral_SnapSettings_AnimFrameDelay = new System.Windows.Forms.Label();
			this.chkGeneral_SnapSettings_AnimUseMultiSnapDelay = new System.Windows.Forms.CheckBox();
			this.udGeneral_SnapSettings_AnimHeight = new System.Windows.Forms.NumericUpDown();
			this.lblGeneral_SnapSettings_AnimHeight = new System.Windows.Forms.Label();
			this.udGeneral_SnapSettings_AnimWidth = new System.Windows.Forms.NumericUpDown();
			this.lblGeneral_SnapSettings_AnimWidth = new System.Windows.Forms.Label();
			this.chkGeneral_SnapSettings_AnimOriginalDimentions = new System.Windows.Forms.CheckBox();
			this.chkGeneral_SnapSettings_SaveBug = new System.Windows.Forms.CheckBox();
			this.grpGeneral_SnapSettings_ImageFormat = new System.Windows.Forms.GroupBox();
			this.lblGeneral_SnapSettings_ImageFormat = new System.Windows.Forms.Label();
			this.cbGeneral_SnapSettings_ImageFormat = new System.Windows.Forms.ComboBox();
			this.lblGeneral_SnapSettings_Quality = new System.Windows.Forms.Label();
			this.tbGeneral_SnapSettings_Quality = new System.Windows.Forms.TrackBar();
			this.grpGeneral_SnapSettings_SnapDir = new System.Windows.Forms.GroupBox();
			this.txtGeneral_SnapSettings_SnapDir = new System.Windows.Forms.TextBox();
			this.cmdGeneral_SnapSettings_BrowseSnapDir = new System.Windows.Forms.Button();
			this.grpGeneral_SnapSettings_TimingAndParameters = new System.Windows.Forms.GroupBox();
			this.udGeneral_SnapSettings_NextSnapDelay = new System.Windows.Forms.NumericUpDown();
			this.udGeneral_SnapSettings_SaveQueueSize = new System.Windows.Forms.NumericUpDown();
			this.udGeneral_SnapSettings_Delay = new System.Windows.Forms.NumericUpDown();
			this.lblGeneral_SnapSettings_NextSnapDelay = new System.Windows.Forms.Label();
			this.lblGeneral_SnapSettings_SaveQueueSize = new System.Windows.Forms.Label();
			this.lblGeneral_SnapSettings_SnapDelay = new System.Windows.Forms.Label();
			this.chkGeneral_SnapSettings_Enabled = new System.Windows.Forms.CheckBox();
			this.tabGeneral_GlobalStatsSettings = new System.Windows.Forms.TabPage();
			this.cmdGeneral_StatsSettings_ViewStats = new System.Windows.Forms.Button();
			this.cmdGeneral_StatsSettings_Reset = new System.Windows.Forms.Button();
			this.chkGeneral_StatsSettings_Enabled = new System.Windows.Forms.CheckBox();
			this.tabGeneral_HotKeys = new System.Windows.Forms.TabPage();
			this.txtGeneral_HotKeys_CaptureActiveProfile = new System.Windows.Forms.TextBox();
			this.txtGeneral_HotKeys_CaptureDesktop = new System.Windows.Forms.TextBox();
			this.txtGeneral_HotKeys_CaptureWindow = new System.Windows.Forms.TextBox();
			this.lblGeneral_HotKeys_CaptureActiveProfile = new System.Windows.Forms.Label();
			this.lblGeneral_HotKeys_CaptureWindow = new System.Windows.Forms.Label();
			this.lblGeneral_HotKeys_CaptureDesktop = new System.Windows.Forms.Label();
			this.chkGeneral_HotKeys_Enabled = new System.Windows.Forms.CheckBox();
			this.tabGeneral_Misc = new System.Windows.Forms.TabPage();
			this.grpGeneral_Misc_Updates = new System.Windows.Forms.GroupBox();
			this.lblGeneral_Misc_CheckUpdates = new System.Windows.Forms.Label();
			this.cbGeneral_Misc_CheckUpdates = new System.Windows.Forms.ComboBox();
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
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.lblProfiles_SnapSettings_Brightness = new System.Windows.Forms.Label();
			this.tbProfiles_SnapSettings_Brightness = new System.Windows.Forms.TrackBar();
			this.lblProfiles_SnapSettings_Contrast = new System.Windows.Forms.Label();
			this.tbProfiles_SnapSettings_Contrast = new System.Windows.Forms.TrackBar();
			this.lblProfiles_SnapSettings_Gamma = new System.Windows.Forms.Label();
			this.tbProfiles_SnapSettings_Gamma = new System.Windows.Forms.TrackBar();
			this.lblProfiles_SnapSettings_Save = new System.Windows.Forms.Label();
			this.cbProfiles_SnapSettings_SaveType = new System.Windows.Forms.ComboBox();
			this.grpProfiles_SnapSettings_SnapDir = new System.Windows.Forms.GroupBox();
			this.txtProfiles_SnapSettings_SnapDir = new System.Windows.Forms.TextBox();
			this.cmdProfiles_BrowseSnapDir = new System.Windows.Forms.Button();
			this.grpProfiles_SnapSettings_TimingAndParameters = new System.Windows.Forms.GroupBox();
			this.lblProfiles_SnapSettings_MultiSnapDelay = new System.Windows.Forms.Label();
			this.udProfiles_SnapSettings_MultiSnapDelay = new System.Windows.Forms.NumericUpDown();
			this.lblProfiles_SnapSettings_SnapCount = new System.Windows.Forms.Label();
			this.udProfiles_SnapSettings_SnapCount = new System.Windows.Forms.NumericUpDown();
			this.lblProfiles_SnapSettings_NextSnapDelay = new System.Windows.Forms.Label();
			this.udProfiles_SnapSettings_NextSnapDelay = new System.Windows.Forms.NumericUpDown();
			this.lblProfiles_SnapSettings_SnapDelay = new System.Windows.Forms.Label();
			this.udProfiles_SnapSettings_Delay = new System.Windows.Forms.NumericUpDown();
			this.chkProfiles_SnapSettings_Enabled = new System.Windows.Forms.CheckBox();
			this.tabProfiles_StatsSettings = new System.Windows.Forms.TabPage();
			this.chkProfiles_StatsSettings_UseGlobal = new System.Windows.Forms.CheckBox();
			this.grpProfiles_StatsSettingsSub = new System.Windows.Forms.GroupBox();
			this.chkProfiles_StatsSettings_Enabled = new System.Windows.Forms.CheckBox();
			this.cmdProfiles_StatsSettings_View = new System.Windows.Forms.Button();
			this.cmdProfiles_StatsSettings_Reset = new System.Windows.Forms.Button();
			this.tabLog = new System.Windows.Forms.TabPage();
			this.rtxtLog = new System.Windows.Forms.RichTextBox();
			this.tabAbout = new System.Windows.Forms.TabPage();
			this.rtxtAbout = new System.Windows.Forms.RichTextBox();
			this.picAboutIcon = new System.Windows.Forms.PictureBox();
			this.TimerMsg = new System.Windows.Forms.Timer(this.components);
			this.cmdApply = new System.Windows.Forms.Button();
			this.tabOptions.SuspendLayout();
			this.tabGeneral.SuspendLayout();
			this.tabGeneralOptions.SuspendLayout();
			this.tabGeneral_GlobalSnapSettings.SuspendLayout();
			this.grpGeneral_SnapSettings_AnimationSettings.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.udGeneral_SnapSettings_AnimFrameDelay)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.udGeneral_SnapSettings_AnimHeight)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.udGeneral_SnapSettings_AnimWidth)).BeginInit();
			this.grpGeneral_SnapSettings_ImageFormat.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.tbGeneral_SnapSettings_Quality)).BeginInit();
			this.grpGeneral_SnapSettings_SnapDir.SuspendLayout();
			this.grpGeneral_SnapSettings_TimingAndParameters.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.udGeneral_SnapSettings_NextSnapDelay)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.udGeneral_SnapSettings_SaveQueueSize)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.udGeneral_SnapSettings_Delay)).BeginInit();
			this.tabGeneral_GlobalStatsSettings.SuspendLayout();
			this.tabGeneral_HotKeys.SuspendLayout();
			this.tabGeneral_Misc.SuspendLayout();
			this.grpGeneral_Misc_Updates.SuspendLayout();
			this.tabProfiles.SuspendLayout();
			this.tabProfileOptions.SuspendLayout();
			this.tabProfiles_GameSettings.SuspendLayout();
			this.grpProfiles_GameSettings_Path.SuspendLayout();
			this.tabProfiles_SnapSettings.SuspendLayout();
			this.grpProfiles_SnapSettingsSub.SuspendLayout();
			this.groupBox1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.tbProfiles_SnapSettings_Brightness)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.tbProfiles_SnapSettings_Contrast)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.tbProfiles_SnapSettings_Gamma)).BeginInit();
			this.grpProfiles_SnapSettings_SnapDir.SuspendLayout();
			this.grpProfiles_SnapSettings_TimingAndParameters.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.udProfiles_SnapSettings_MultiSnapDelay)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.udProfiles_SnapSettings_SnapCount)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.udProfiles_SnapSettings_NextSnapDelay)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.udProfiles_SnapSettings_Delay)).BeginInit();
			this.tabProfiles_StatsSettings.SuspendLayout();
			this.grpProfiles_StatsSettingsSub.SuspendLayout();
			this.tabLog.SuspendLayout();
			this.tabAbout.SuspendLayout();
			this.SuspendLayout();
			// 
			// HKCaptureDesktop
			// 
			this.HKCaptureDesktop.EnableKeyCapture = false;
			this.HKCaptureDesktop.Shortcut = null;
			this.HKCaptureDesktop.Pressed += new System.EventHandler(this.HKCaptureDesktop_Pressed);
			this.HKCaptureDesktop.KeyCapture += new System.EventHandler(this.HKCaptureDesktop_KeyCapture);
			// 
			// HKCaptureWindow
			// 
			this.HKCaptureWindow.EnableKeyCapture = false;
			this.HKCaptureWindow.Shortcut = null;
			this.HKCaptureWindow.Pressed += new System.EventHandler(this.HKCaptureWindow_Pressed);
			this.HKCaptureWindow.KeyCapture += new System.EventHandler(this.HKCaptureWindow_KeyCapture);
			// 
			// HKCaptureActiveProfile
			// 
			this.HKCaptureActiveProfile.EnableKeyCapture = false;
			this.HKCaptureActiveProfile.Shortcut = null;
			this.HKCaptureActiveProfile.Pressed += new System.EventHandler(this.HKCaptureActiveProfile_Pressed);
			this.HKCaptureActiveProfile.KeyCapture += new System.EventHandler(this.HKCaptureActiveProfile_KeyCapture);
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
			// TimerMisc
			// 
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
			this.cmdCancel.Location = new System.Drawing.Point(136, 416);
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
			this.tabOptions.Controls.Add(this.tabLog);
			this.tabOptions.Controls.Add(this.tabAbout);
			this.tabOptions.ItemSize = new System.Drawing.Size(49, 21);
			this.tabOptions.Location = new System.Drawing.Point(0, 8);
			this.tabOptions.Name = "tabOptions";
			this.tabOptions.SelectedIndex = 0;
			this.tabOptions.Size = new System.Drawing.Size(448, 408);
			this.tabOptions.TabIndex = 9;
			// 
			// tabGeneral
			// 
			this.tabGeneral.Controls.Add(this.tabGeneralOptions);
			this.tabGeneral.Location = new System.Drawing.Point(4, 25);
			this.tabGeneral.Name = "tabGeneral";
			this.tabGeneral.Size = new System.Drawing.Size(440, 379);
			this.tabGeneral.TabIndex = 0;
			this.tabGeneral.Text = "General";
			// 
			// tabGeneralOptions
			// 
			this.tabGeneralOptions.Controls.Add(this.tabGeneral_GlobalSnapSettings);
			this.tabGeneralOptions.Controls.Add(this.tabGeneral_GlobalStatsSettings);
			this.tabGeneralOptions.Controls.Add(this.tabGeneral_HotKeys);
			this.tabGeneralOptions.Controls.Add(this.tabGeneral_Misc);
			this.tabGeneralOptions.Location = new System.Drawing.Point(8, 8);
			this.tabGeneralOptions.Name = "tabGeneralOptions";
			this.tabGeneralOptions.SelectedIndex = 0;
			this.tabGeneralOptions.Size = new System.Drawing.Size(424, 368);
			this.tabGeneralOptions.TabIndex = 4;
			// 
			// tabGeneral_GlobalSnapSettings
			// 
			this.tabGeneral_GlobalSnapSettings.AutoScroll = true;
			this.tabGeneral_GlobalSnapSettings.Controls.Add(this.grpGeneral_SnapSettings_AnimationSettings);
			this.tabGeneral_GlobalSnapSettings.Controls.Add(this.chkGeneral_SnapSettings_SaveBug);
			this.tabGeneral_GlobalSnapSettings.Controls.Add(this.grpGeneral_SnapSettings_ImageFormat);
			this.tabGeneral_GlobalSnapSettings.Controls.Add(this.grpGeneral_SnapSettings_SnapDir);
			this.tabGeneral_GlobalSnapSettings.Controls.Add(this.grpGeneral_SnapSettings_TimingAndParameters);
			this.tabGeneral_GlobalSnapSettings.Controls.Add(this.chkGeneral_SnapSettings_Enabled);
			this.tabGeneral_GlobalSnapSettings.Location = new System.Drawing.Point(4, 22);
			this.tabGeneral_GlobalSnapSettings.Name = "tabGeneral_GlobalSnapSettings";
			this.tabGeneral_GlobalSnapSettings.Size = new System.Drawing.Size(416, 342);
			this.tabGeneral_GlobalSnapSettings.TabIndex = 0;
			this.tabGeneral_GlobalSnapSettings.Text = "Global Snap Settings";
			// 
			// grpGeneral_SnapSettings_AnimationSettings
			// 
			this.grpGeneral_SnapSettings_AnimationSettings.Controls.Add(this.chkGeneral_SnapSettings_AnimOptimizePalette);
			this.grpGeneral_SnapSettings_AnimationSettings.Controls.Add(this.udGeneral_SnapSettings_AnimFrameDelay);
			this.grpGeneral_SnapSettings_AnimationSettings.Controls.Add(this.lblGeneral_SnapSettings_AnimFrameDelay);
			this.grpGeneral_SnapSettings_AnimationSettings.Controls.Add(this.chkGeneral_SnapSettings_AnimUseMultiSnapDelay);
			this.grpGeneral_SnapSettings_AnimationSettings.Controls.Add(this.udGeneral_SnapSettings_AnimHeight);
			this.grpGeneral_SnapSettings_AnimationSettings.Controls.Add(this.lblGeneral_SnapSettings_AnimHeight);
			this.grpGeneral_SnapSettings_AnimationSettings.Controls.Add(this.udGeneral_SnapSettings_AnimWidth);
			this.grpGeneral_SnapSettings_AnimationSettings.Controls.Add(this.lblGeneral_SnapSettings_AnimWidth);
			this.grpGeneral_SnapSettings_AnimationSettings.Controls.Add(this.chkGeneral_SnapSettings_AnimOriginalDimentions);
			this.grpGeneral_SnapSettings_AnimationSettings.Location = new System.Drawing.Point(8, 328);
			this.grpGeneral_SnapSettings_AnimationSettings.Name = "grpGeneral_SnapSettings_AnimationSettings";
			this.grpGeneral_SnapSettings_AnimationSettings.Size = new System.Drawing.Size(384, 176);
			this.grpGeneral_SnapSettings_AnimationSettings.TabIndex = 25;
			this.grpGeneral_SnapSettings_AnimationSettings.TabStop = false;
			this.grpGeneral_SnapSettings_AnimationSettings.Text = "Animation Settings";
			// 
			// chkGeneral_SnapSettings_AnimOptimizePalette
			// 
			this.chkGeneral_SnapSettings_AnimOptimizePalette.Location = new System.Drawing.Point(8, 16);
			this.chkGeneral_SnapSettings_AnimOptimizePalette.Name = "chkGeneral_SnapSettings_AnimOptimizePalette";
			this.chkGeneral_SnapSettings_AnimOptimizePalette.Size = new System.Drawing.Size(136, 24);
			this.chkGeneral_SnapSettings_AnimOptimizePalette.TabIndex = 36;
			this.chkGeneral_SnapSettings_AnimOptimizePalette.Text = "Use Optimized Palette";
			// 
			// udGeneral_SnapSettings_AnimFrameDelay
			// 
			this.udGeneral_SnapSettings_AnimFrameDelay.Location = new System.Drawing.Point(88, 152);
			this.udGeneral_SnapSettings_AnimFrameDelay.Maximum = new System.Decimal(new int[] {
																								  10000,
																								  0,
																								  0,
																								  0});
			this.udGeneral_SnapSettings_AnimFrameDelay.Minimum = new System.Decimal(new int[] {
																								  1,
																								  0,
																								  0,
																								  0});
			this.udGeneral_SnapSettings_AnimFrameDelay.Name = "udGeneral_SnapSettings_AnimFrameDelay";
			this.udGeneral_SnapSettings_AnimFrameDelay.Size = new System.Drawing.Size(288, 20);
			this.udGeneral_SnapSettings_AnimFrameDelay.TabIndex = 34;
			this.udGeneral_SnapSettings_AnimFrameDelay.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.udGeneral_SnapSettings_AnimFrameDelay.Value = new System.Decimal(new int[] {
																								1,
																								0,
																								0,
																								0});
			// 
			// lblGeneral_SnapSettings_AnimFrameDelay
			// 
			this.lblGeneral_SnapSettings_AnimFrameDelay.Location = new System.Drawing.Point(8, 152);
			this.lblGeneral_SnapSettings_AnimFrameDelay.Name = "lblGeneral_SnapSettings_AnimFrameDelay";
			this.lblGeneral_SnapSettings_AnimFrameDelay.Size = new System.Drawing.Size(72, 16);
			this.lblGeneral_SnapSettings_AnimFrameDelay.TabIndex = 35;
			this.lblGeneral_SnapSettings_AnimFrameDelay.Text = "Frame Delay:";
			this.lblGeneral_SnapSettings_AnimFrameDelay.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// chkGeneral_SnapSettings_AnimUseMultiSnapDelay
			// 
			this.chkGeneral_SnapSettings_AnimUseMultiSnapDelay.Location = new System.Drawing.Point(8, 128);
			this.chkGeneral_SnapSettings_AnimUseMultiSnapDelay.Name = "chkGeneral_SnapSettings_AnimUseMultiSnapDelay";
			this.chkGeneral_SnapSettings_AnimUseMultiSnapDelay.Size = new System.Drawing.Size(144, 16);
			this.chkGeneral_SnapSettings_AnimUseMultiSnapDelay.TabIndex = 33;
			this.chkGeneral_SnapSettings_AnimUseMultiSnapDelay.Text = "Use MultiSnap Delay";
			this.chkGeneral_SnapSettings_AnimUseMultiSnapDelay.CheckedChanged += new System.EventHandler(this.chkGeneral_SnapSettings_AnimUseMultiSnapDelay_CheckedChanged);
			// 
			// udGeneral_SnapSettings_AnimHeight
			// 
			this.udGeneral_SnapSettings_AnimHeight.Location = new System.Drawing.Point(88, 96);
			this.udGeneral_SnapSettings_AnimHeight.Maximum = new System.Decimal(new int[] {
																							  5000,
																							  0,
																							  0,
																							  0});
			this.udGeneral_SnapSettings_AnimHeight.Name = "udGeneral_SnapSettings_AnimHeight";
			this.udGeneral_SnapSettings_AnimHeight.Size = new System.Drawing.Size(288, 20);
			this.udGeneral_SnapSettings_AnimHeight.TabIndex = 31;
			this.udGeneral_SnapSettings_AnimHeight.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// lblGeneral_SnapSettings_AnimHeight
			// 
			this.lblGeneral_SnapSettings_AnimHeight.Location = new System.Drawing.Point(8, 96);
			this.lblGeneral_SnapSettings_AnimHeight.Name = "lblGeneral_SnapSettings_AnimHeight";
			this.lblGeneral_SnapSettings_AnimHeight.Size = new System.Drawing.Size(72, 16);
			this.lblGeneral_SnapSettings_AnimHeight.TabIndex = 32;
			this.lblGeneral_SnapSettings_AnimHeight.Text = "Height:";
			this.lblGeneral_SnapSettings_AnimHeight.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// udGeneral_SnapSettings_AnimWidth
			// 
			this.udGeneral_SnapSettings_AnimWidth.Location = new System.Drawing.Point(88, 72);
			this.udGeneral_SnapSettings_AnimWidth.Maximum = new System.Decimal(new int[] {
																							 5000,
																							 0,
																							 0,
																							 0});
			this.udGeneral_SnapSettings_AnimWidth.Name = "udGeneral_SnapSettings_AnimWidth";
			this.udGeneral_SnapSettings_AnimWidth.Size = new System.Drawing.Size(288, 20);
			this.udGeneral_SnapSettings_AnimWidth.TabIndex = 29;
			this.udGeneral_SnapSettings_AnimWidth.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// lblGeneral_SnapSettings_AnimWidth
			// 
			this.lblGeneral_SnapSettings_AnimWidth.Location = new System.Drawing.Point(8, 72);
			this.lblGeneral_SnapSettings_AnimWidth.Name = "lblGeneral_SnapSettings_AnimWidth";
			this.lblGeneral_SnapSettings_AnimWidth.Size = new System.Drawing.Size(72, 16);
			this.lblGeneral_SnapSettings_AnimWidth.TabIndex = 30;
			this.lblGeneral_SnapSettings_AnimWidth.Text = "Width:";
			this.lblGeneral_SnapSettings_AnimWidth.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// chkGeneral_SnapSettings_AnimOriginalDimentions
			// 
			this.chkGeneral_SnapSettings_AnimOriginalDimentions.Location = new System.Drawing.Point(8, 48);
			this.chkGeneral_SnapSettings_AnimOriginalDimentions.Name = "chkGeneral_SnapSettings_AnimOriginalDimentions";
			this.chkGeneral_SnapSettings_AnimOriginalDimentions.Size = new System.Drawing.Size(160, 24);
			this.chkGeneral_SnapSettings_AnimOriginalDimentions.TabIndex = 26;
			this.chkGeneral_SnapSettings_AnimOriginalDimentions.Text = "Use Original Dimentions";
			this.chkGeneral_SnapSettings_AnimOriginalDimentions.CheckedChanged += new System.EventHandler(this.chkGeneral_SnapSettings_AnimOriginalDimentions_CheckedChanged);
			// 
			// chkGeneral_SnapSettings_SaveBug
			// 
			this.chkGeneral_SnapSettings_SaveBug.Location = new System.Drawing.Point(128, 8);
			this.chkGeneral_SnapSettings_SaveBug.Name = "chkGeneral_SnapSettings_SaveBug";
			this.chkGeneral_SnapSettings_SaveBug.Size = new System.Drawing.Size(80, 24);
			this.chkGeneral_SnapSettings_SaveBug.TabIndex = 24;
			this.chkGeneral_SnapSettings_SaveBug.Text = "Save Bug";
			// 
			// grpGeneral_SnapSettings_ImageFormat
			// 
			this.grpGeneral_SnapSettings_ImageFormat.Controls.Add(this.lblGeneral_SnapSettings_ImageFormat);
			this.grpGeneral_SnapSettings_ImageFormat.Controls.Add(this.cbGeneral_SnapSettings_ImageFormat);
			this.grpGeneral_SnapSettings_ImageFormat.Controls.Add(this.lblGeneral_SnapSettings_Quality);
			this.grpGeneral_SnapSettings_ImageFormat.Controls.Add(this.tbGeneral_SnapSettings_Quality);
			this.grpGeneral_SnapSettings_ImageFormat.Location = new System.Drawing.Point(8, 192);
			this.grpGeneral_SnapSettings_ImageFormat.Name = "grpGeneral_SnapSettings_ImageFormat";
			this.grpGeneral_SnapSettings_ImageFormat.Size = new System.Drawing.Size(384, 120);
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
			this.lblGeneral_SnapSettings_ImageFormat.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
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
			this.lblGeneral_SnapSettings_Quality.Location = new System.Drawing.Point(16, 56);
			this.lblGeneral_SnapSettings_Quality.Name = "lblGeneral_SnapSettings_Quality";
			this.lblGeneral_SnapSettings_Quality.Size = new System.Drawing.Size(176, 16);
			this.lblGeneral_SnapSettings_Quality.TabIndex = 19;
			this.lblGeneral_SnapSettings_Quality.Text = "Quality x% (100% = Clearest):";
			// 
			// tbGeneral_SnapSettings_Quality
			// 
			this.tbGeneral_SnapSettings_Quality.Location = new System.Drawing.Point(8, 72);
			this.tbGeneral_SnapSettings_Quality.Maximum = 100;
			this.tbGeneral_SnapSettings_Quality.Name = "tbGeneral_SnapSettings_Quality";
			this.tbGeneral_SnapSettings_Quality.Size = new System.Drawing.Size(368, 40);
			this.tbGeneral_SnapSettings_Quality.TabIndex = 18;
			this.tbGeneral_SnapSettings_Quality.Value = 100;
			this.tbGeneral_SnapSettings_Quality.Scroll += new System.EventHandler(this.tbGeneral_SnapSettings_Quality_Scroll);
			// 
			// grpGeneral_SnapSettings_SnapDir
			// 
			this.grpGeneral_SnapSettings_SnapDir.Controls.Add(this.txtGeneral_SnapSettings_SnapDir);
			this.grpGeneral_SnapSettings_SnapDir.Controls.Add(this.cmdGeneral_SnapSettings_BrowseSnapDir);
			this.grpGeneral_SnapSettings_SnapDir.Location = new System.Drawing.Point(8, 136);
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
			// grpGeneral_SnapSettings_TimingAndParameters
			// 
			this.grpGeneral_SnapSettings_TimingAndParameters.Controls.Add(this.udGeneral_SnapSettings_NextSnapDelay);
			this.grpGeneral_SnapSettings_TimingAndParameters.Controls.Add(this.udGeneral_SnapSettings_SaveQueueSize);
			this.grpGeneral_SnapSettings_TimingAndParameters.Controls.Add(this.udGeneral_SnapSettings_Delay);
			this.grpGeneral_SnapSettings_TimingAndParameters.Controls.Add(this.lblGeneral_SnapSettings_NextSnapDelay);
			this.grpGeneral_SnapSettings_TimingAndParameters.Controls.Add(this.lblGeneral_SnapSettings_SaveQueueSize);
			this.grpGeneral_SnapSettings_TimingAndParameters.Controls.Add(this.lblGeneral_SnapSettings_SnapDelay);
			this.grpGeneral_SnapSettings_TimingAndParameters.Location = new System.Drawing.Point(8, 32);
			this.grpGeneral_SnapSettings_TimingAndParameters.Name = "grpGeneral_SnapSettings_TimingAndParameters";
			this.grpGeneral_SnapSettings_TimingAndParameters.Size = new System.Drawing.Size(384, 96);
			this.grpGeneral_SnapSettings_TimingAndParameters.TabIndex = 21;
			this.grpGeneral_SnapSettings_TimingAndParameters.TabStop = false;
			this.grpGeneral_SnapSettings_TimingAndParameters.Text = "Timing && Parameters";
			// 
			// udGeneral_SnapSettings_NextSnapDelay
			// 
			this.udGeneral_SnapSettings_NextSnapDelay.Location = new System.Drawing.Point(128, 40);
			this.udGeneral_SnapSettings_NextSnapDelay.Maximum = new System.Decimal(new int[] {
																								 10000,
																								 0,
																								 0,
																								 0});
			this.udGeneral_SnapSettings_NextSnapDelay.Name = "udGeneral_SnapSettings_NextSnapDelay";
			this.udGeneral_SnapSettings_NextSnapDelay.Size = new System.Drawing.Size(248, 20);
			this.udGeneral_SnapSettings_NextSnapDelay.TabIndex = 32;
			this.udGeneral_SnapSettings_NextSnapDelay.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// udGeneral_SnapSettings_SaveQueueSize
			// 
			this.udGeneral_SnapSettings_SaveQueueSize.Location = new System.Drawing.Point(128, 64);
			this.udGeneral_SnapSettings_SaveQueueSize.Maximum = new System.Decimal(new int[] {
																								 500,
																								 0,
																								 0,
																								 0});
			this.udGeneral_SnapSettings_SaveQueueSize.Minimum = new System.Decimal(new int[] {
																								 10,
																								 0,
																								 0,
																								 0});
			this.udGeneral_SnapSettings_SaveQueueSize.Name = "udGeneral_SnapSettings_SaveQueueSize";
			this.udGeneral_SnapSettings_SaveQueueSize.Size = new System.Drawing.Size(248, 20);
			this.udGeneral_SnapSettings_SaveQueueSize.TabIndex = 31;
			this.udGeneral_SnapSettings_SaveQueueSize.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.udGeneral_SnapSettings_SaveQueueSize.Value = new System.Decimal(new int[] {
																							   10,
																							   0,
																							   0,
																							   0});
			// 
			// udGeneral_SnapSettings_Delay
			// 
			this.udGeneral_SnapSettings_Delay.Location = new System.Drawing.Point(128, 16);
			this.udGeneral_SnapSettings_Delay.Maximum = new System.Decimal(new int[] {
																						 10000,
																						 0,
																						 0,
																						 0});
			this.udGeneral_SnapSettings_Delay.Name = "udGeneral_SnapSettings_Delay";
			this.udGeneral_SnapSettings_Delay.Size = new System.Drawing.Size(248, 20);
			this.udGeneral_SnapSettings_Delay.TabIndex = 21;
			this.udGeneral_SnapSettings_Delay.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// lblGeneral_SnapSettings_NextSnapDelay
			// 
			this.lblGeneral_SnapSettings_NextSnapDelay.Location = new System.Drawing.Point(16, 40);
			this.lblGeneral_SnapSettings_NextSnapDelay.Name = "lblGeneral_SnapSettings_NextSnapDelay";
			this.lblGeneral_SnapSettings_NextSnapDelay.Size = new System.Drawing.Size(96, 16);
			this.lblGeneral_SnapSettings_NextSnapDelay.TabIndex = 30;
			this.lblGeneral_SnapSettings_NextSnapDelay.Text = "Next Snap Delay:";
			this.lblGeneral_SnapSettings_NextSnapDelay.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// lblGeneral_SnapSettings_SaveQueueSize
			// 
			this.lblGeneral_SnapSettings_SaveQueueSize.Location = new System.Drawing.Point(16, 64);
			this.lblGeneral_SnapSettings_SaveQueueSize.Name = "lblGeneral_SnapSettings_SaveQueueSize";
			this.lblGeneral_SnapSettings_SaveQueueSize.Size = new System.Drawing.Size(96, 16);
			this.lblGeneral_SnapSettings_SaveQueueSize.TabIndex = 29;
			this.lblGeneral_SnapSettings_SaveQueueSize.Text = "SaveQueue Size:";
			this.lblGeneral_SnapSettings_SaveQueueSize.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// lblGeneral_SnapSettings_SnapDelay
			// 
			this.lblGeneral_SnapSettings_SnapDelay.Location = new System.Drawing.Point(16, 16);
			this.lblGeneral_SnapSettings_SnapDelay.Name = "lblGeneral_SnapSettings_SnapDelay";
			this.lblGeneral_SnapSettings_SnapDelay.Size = new System.Drawing.Size(72, 16);
			this.lblGeneral_SnapSettings_SnapDelay.TabIndex = 28;
			this.lblGeneral_SnapSettings_SnapDelay.Text = "Snap Delay:";
			this.lblGeneral_SnapSettings_SnapDelay.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
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
			this.tabGeneral_GlobalStatsSettings.Size = new System.Drawing.Size(416, 342);
			this.tabGeneral_GlobalStatsSettings.TabIndex = 1;
			this.tabGeneral_GlobalStatsSettings.Text = "Global Stats Settings";
			// 
			// cmdGeneral_StatsSettings_ViewStats
			// 
			this.cmdGeneral_StatsSettings_ViewStats.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.cmdGeneral_StatsSettings_ViewStats.Location = new System.Drawing.Point(264, 8);
			this.cmdGeneral_StatsSettings_ViewStats.Name = "cmdGeneral_StatsSettings_ViewStats";
			this.cmdGeneral_StatsSettings_ViewStats.Size = new System.Drawing.Size(72, 24);
			this.cmdGeneral_StatsSettings_ViewStats.TabIndex = 8;
			this.cmdGeneral_StatsSettings_ViewStats.Text = "View";
			this.cmdGeneral_StatsSettings_ViewStats.Click += new System.EventHandler(this.cmdGeneral_StatsSettings_ViewStats_Click);
			// 
			// cmdGeneral_StatsSettings_Reset
			// 
			this.cmdGeneral_StatsSettings_Reset.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.cmdGeneral_StatsSettings_Reset.Location = new System.Drawing.Point(336, 8);
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
			// tabGeneral_HotKeys
			// 
			this.tabGeneral_HotKeys.Controls.Add(this.txtGeneral_HotKeys_CaptureActiveProfile);
			this.tabGeneral_HotKeys.Controls.Add(this.txtGeneral_HotKeys_CaptureDesktop);
			this.tabGeneral_HotKeys.Controls.Add(this.txtGeneral_HotKeys_CaptureWindow);
			this.tabGeneral_HotKeys.Controls.Add(this.lblGeneral_HotKeys_CaptureActiveProfile);
			this.tabGeneral_HotKeys.Controls.Add(this.lblGeneral_HotKeys_CaptureWindow);
			this.tabGeneral_HotKeys.Controls.Add(this.lblGeneral_HotKeys_CaptureDesktop);
			this.tabGeneral_HotKeys.Controls.Add(this.chkGeneral_HotKeys_Enabled);
			this.tabGeneral_HotKeys.Location = new System.Drawing.Point(4, 22);
			this.tabGeneral_HotKeys.Name = "tabGeneral_HotKeys";
			this.tabGeneral_HotKeys.Size = new System.Drawing.Size(416, 342);
			this.tabGeneral_HotKeys.TabIndex = 2;
			this.tabGeneral_HotKeys.Text = "Hot Keys";
			// 
			// txtGeneral_HotKeys_CaptureActiveProfile
			// 
			this.txtGeneral_HotKeys_CaptureActiveProfile.Location = new System.Drawing.Point(160, 112);
			this.txtGeneral_HotKeys_CaptureActiveProfile.Name = "txtGeneral_HotKeys_CaptureActiveProfile";
			this.txtGeneral_HotKeys_CaptureActiveProfile.ReadOnly = true;
			this.txtGeneral_HotKeys_CaptureActiveProfile.Size = new System.Drawing.Size(248, 20);
			this.txtGeneral_HotKeys_CaptureActiveProfile.TabIndex = 10;
			this.txtGeneral_HotKeys_CaptureActiveProfile.Text = "Capture Active Profile";
			this.txtGeneral_HotKeys_CaptureActiveProfile.Leave += new System.EventHandler(this.txtGeneral_HotKeys_CaptureActiveProfile_Leave);
			this.txtGeneral_HotKeys_CaptureActiveProfile.Enter += new System.EventHandler(this.txtGeneral_HotKeys_CaptureActiveProfile_Enter);
			// 
			// txtGeneral_HotKeys_CaptureDesktop
			// 
			this.txtGeneral_HotKeys_CaptureDesktop.Location = new System.Drawing.Point(160, 88);
			this.txtGeneral_HotKeys_CaptureDesktop.Name = "txtGeneral_HotKeys_CaptureDesktop";
			this.txtGeneral_HotKeys_CaptureDesktop.ReadOnly = true;
			this.txtGeneral_HotKeys_CaptureDesktop.Size = new System.Drawing.Size(248, 20);
			this.txtGeneral_HotKeys_CaptureDesktop.TabIndex = 9;
			this.txtGeneral_HotKeys_CaptureDesktop.Text = "Capture Desktop";
			this.txtGeneral_HotKeys_CaptureDesktop.Leave += new System.EventHandler(this.txtGeneral_HotKeys_CaptureDesktop_Leave);
			this.txtGeneral_HotKeys_CaptureDesktop.Enter += new System.EventHandler(this.txtGeneral_HotKeys_CaptureDesktop_Enter);
			// 
			// txtGeneral_HotKeys_CaptureWindow
			// 
			this.txtGeneral_HotKeys_CaptureWindow.Location = new System.Drawing.Point(160, 64);
			this.txtGeneral_HotKeys_CaptureWindow.Name = "txtGeneral_HotKeys_CaptureWindow";
			this.txtGeneral_HotKeys_CaptureWindow.ReadOnly = true;
			this.txtGeneral_HotKeys_CaptureWindow.Size = new System.Drawing.Size(248, 20);
			this.txtGeneral_HotKeys_CaptureWindow.TabIndex = 8;
			this.txtGeneral_HotKeys_CaptureWindow.Text = "Capture Window";
			this.txtGeneral_HotKeys_CaptureWindow.Leave += new System.EventHandler(this.txtGeneral_HotKeys_CaptureWindow_Leave);
			this.txtGeneral_HotKeys_CaptureWindow.Enter += new System.EventHandler(this.txtGeneral_HotKeys_CaptureWindow_Enter);
			// 
			// lblGeneral_HotKeys_CaptureActiveProfile
			// 
			this.lblGeneral_HotKeys_CaptureActiveProfile.Location = new System.Drawing.Point(16, 112);
			this.lblGeneral_HotKeys_CaptureActiveProfile.Name = "lblGeneral_HotKeys_CaptureActiveProfile";
			this.lblGeneral_HotKeys_CaptureActiveProfile.Size = new System.Drawing.Size(128, 24);
			this.lblGeneral_HotKeys_CaptureActiveProfile.TabIndex = 4;
			this.lblGeneral_HotKeys_CaptureActiveProfile.Text = "Capture Active Profile:";
			this.lblGeneral_HotKeys_CaptureActiveProfile.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// lblGeneral_HotKeys_CaptureWindow
			// 
			this.lblGeneral_HotKeys_CaptureWindow.Location = new System.Drawing.Point(16, 64);
			this.lblGeneral_HotKeys_CaptureWindow.Name = "lblGeneral_HotKeys_CaptureWindow";
			this.lblGeneral_HotKeys_CaptureWindow.Size = new System.Drawing.Size(128, 24);
			this.lblGeneral_HotKeys_CaptureWindow.TabIndex = 3;
			this.lblGeneral_HotKeys_CaptureWindow.Text = "Capture Window:";
			this.lblGeneral_HotKeys_CaptureWindow.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// lblGeneral_HotKeys_CaptureDesktop
			// 
			this.lblGeneral_HotKeys_CaptureDesktop.Location = new System.Drawing.Point(16, 88);
			this.lblGeneral_HotKeys_CaptureDesktop.Name = "lblGeneral_HotKeys_CaptureDesktop";
			this.lblGeneral_HotKeys_CaptureDesktop.Size = new System.Drawing.Size(128, 24);
			this.lblGeneral_HotKeys_CaptureDesktop.TabIndex = 2;
			this.lblGeneral_HotKeys_CaptureDesktop.Text = "Capture Desktop:";
			this.lblGeneral_HotKeys_CaptureDesktop.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// chkGeneral_HotKeys_Enabled
			// 
			this.chkGeneral_HotKeys_Enabled.Location = new System.Drawing.Point(8, 8);
			this.chkGeneral_HotKeys_Enabled.Name = "chkGeneral_HotKeys_Enabled";
			this.chkGeneral_HotKeys_Enabled.Size = new System.Drawing.Size(96, 24);
			this.chkGeneral_HotKeys_Enabled.TabIndex = 1;
			this.chkGeneral_HotKeys_Enabled.Text = "Enabled";
			// 
			// tabGeneral_Misc
			// 
			this.tabGeneral_Misc.Controls.Add(this.grpGeneral_Misc_Updates);
			this.tabGeneral_Misc.Location = new System.Drawing.Point(4, 22);
			this.tabGeneral_Misc.Name = "tabGeneral_Misc";
			this.tabGeneral_Misc.Size = new System.Drawing.Size(416, 342);
			this.tabGeneral_Misc.TabIndex = 3;
			this.tabGeneral_Misc.Text = "Misc";
			// 
			// grpGeneral_Misc_Updates
			// 
			this.grpGeneral_Misc_Updates.Controls.Add(this.lblGeneral_Misc_CheckUpdates);
			this.grpGeneral_Misc_Updates.Controls.Add(this.cbGeneral_Misc_CheckUpdates);
			this.grpGeneral_Misc_Updates.Location = new System.Drawing.Point(8, 8);
			this.grpGeneral_Misc_Updates.Name = "grpGeneral_Misc_Updates";
			this.grpGeneral_Misc_Updates.Size = new System.Drawing.Size(400, 80);
			this.grpGeneral_Misc_Updates.TabIndex = 0;
			this.grpGeneral_Misc_Updates.TabStop = false;
			this.grpGeneral_Misc_Updates.Text = "Updates";
			// 
			// lblGeneral_Misc_CheckUpdates
			// 
			this.lblGeneral_Misc_CheckUpdates.Location = new System.Drawing.Point(8, 16);
			this.lblGeneral_Misc_CheckUpdates.Name = "lblGeneral_Misc_CheckUpdates";
			this.lblGeneral_Misc_CheckUpdates.Size = new System.Drawing.Size(112, 24);
			this.lblGeneral_Misc_CheckUpdates.TabIndex = 23;
			this.lblGeneral_Misc_CheckUpdates.Text = "Check for updates:";
			this.lblGeneral_Misc_CheckUpdates.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// cbGeneral_Misc_CheckUpdates
			// 
			this.cbGeneral_Misc_CheckUpdates.Items.AddRange(new object[] {
																			 "Never",
																			 "Every Hour",
																			 "Every Day",
																			 "Every Week",
																			 "Every Month"});
			this.cbGeneral_Misc_CheckUpdates.Location = new System.Drawing.Point(128, 16);
			this.cbGeneral_Misc_CheckUpdates.Name = "cbGeneral_Misc_CheckUpdates";
			this.cbGeneral_Misc_CheckUpdates.Size = new System.Drawing.Size(120, 21);
			this.cbGeneral_Misc_CheckUpdates.TabIndex = 22;
			this.cbGeneral_Misc_CheckUpdates.Text = "Update Options";
			// 
			// tabProfiles
			// 
			this.tabProfiles.Controls.Add(this.lblProfiles_ActiveProfile);
			this.tabProfiles.Controls.Add(this.tabProfileOptions);
			this.tabProfiles.Controls.Add(this.lstProfiles_Games);
			this.tabProfiles.Location = new System.Drawing.Point(4, 25);
			this.tabProfiles.Name = "tabProfiles";
			this.tabProfiles.Size = new System.Drawing.Size(440, 379);
			this.tabProfiles.TabIndex = 1;
			this.tabProfiles.Text = "Profiles";
			// 
			// lblProfiles_ActiveProfile
			// 
			this.lblProfiles_ActiveProfile.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.lblProfiles_ActiveProfile.Location = new System.Drawing.Point(152, 8);
			this.lblProfiles_ActiveProfile.Name = "lblProfiles_ActiveProfile";
			this.lblProfiles_ActiveProfile.Size = new System.Drawing.Size(280, 16);
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
			this.tabProfileOptions.Size = new System.Drawing.Size(280, 344);
			this.tabProfileOptions.TabIndex = 11;
			// 
			// tabProfiles_GameSettings
			// 
			this.tabProfiles_GameSettings.Controls.Add(this.grpProfiles_GameSettings_Path);
			this.tabProfiles_GameSettings.Location = new System.Drawing.Point(4, 22);
			this.tabProfiles_GameSettings.Name = "tabProfiles_GameSettings";
			this.tabProfiles_GameSettings.Size = new System.Drawing.Size(272, 318);
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
			this.grpProfiles_GameSettings_Path.Size = new System.Drawing.Size(256, 80);
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
			this.cmdProfiles_GameSettings_BrowseGamePath.Location = new System.Drawing.Point(192, 16);
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
			this.txtProfiles_GameSettings_Path.Size = new System.Drawing.Size(240, 20);
			this.txtProfiles_GameSettings_Path.TabIndex = 2;
			this.txtProfiles_GameSettings_Path.Text = "Path To Game";
			// 
			// tabProfiles_SnapSettings
			// 
			this.tabProfiles_SnapSettings.AutoScroll = true;
			this.tabProfiles_SnapSettings.Controls.Add(this.chkProfiles_SnapSettings_UseGlobal);
			this.tabProfiles_SnapSettings.Controls.Add(this.grpProfiles_SnapSettingsSub);
			this.tabProfiles_SnapSettings.Location = new System.Drawing.Point(4, 22);
			this.tabProfiles_SnapSettings.Name = "tabProfiles_SnapSettings";
			this.tabProfiles_SnapSettings.Size = new System.Drawing.Size(272, 318);
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
			this.grpProfiles_SnapSettingsSub.Controls.Add(this.groupBox1);
			this.grpProfiles_SnapSettingsSub.Controls.Add(this.lblProfiles_SnapSettings_Save);
			this.grpProfiles_SnapSettingsSub.Controls.Add(this.cbProfiles_SnapSettings_SaveType);
			this.grpProfiles_SnapSettingsSub.Controls.Add(this.grpProfiles_SnapSettings_SnapDir);
			this.grpProfiles_SnapSettingsSub.Controls.Add(this.grpProfiles_SnapSettings_TimingAndParameters);
			this.grpProfiles_SnapSettingsSub.Controls.Add(this.chkProfiles_SnapSettings_Enabled);
			this.grpProfiles_SnapSettingsSub.Location = new System.Drawing.Point(8, 24);
			this.grpProfiles_SnapSettingsSub.Name = "grpProfiles_SnapSettingsSub";
			this.grpProfiles_SnapSettingsSub.Size = new System.Drawing.Size(240, 464);
			this.grpProfiles_SnapSettingsSub.TabIndex = 1;
			this.grpProfiles_SnapSettingsSub.TabStop = false;
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.Add(this.lblProfiles_SnapSettings_Brightness);
			this.groupBox1.Controls.Add(this.tbProfiles_SnapSettings_Brightness);
			this.groupBox1.Controls.Add(this.lblProfiles_SnapSettings_Contrast);
			this.groupBox1.Controls.Add(this.tbProfiles_SnapSettings_Contrast);
			this.groupBox1.Controls.Add(this.lblProfiles_SnapSettings_Gamma);
			this.groupBox1.Controls.Add(this.tbProfiles_SnapSettings_Gamma);
			this.groupBox1.Location = new System.Drawing.Point(8, 248);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(224, 208);
			this.groupBox1.TabIndex = 23;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Image Adjustments";
			// 
			// lblProfiles_SnapSettings_Brightness
			// 
			this.lblProfiles_SnapSettings_Brightness.Location = new System.Drawing.Point(8, 144);
			this.lblProfiles_SnapSettings_Brightness.Name = "lblProfiles_SnapSettings_Brightness";
			this.lblProfiles_SnapSettings_Brightness.Size = new System.Drawing.Size(208, 16);
			this.lblProfiles_SnapSettings_Brightness.TabIndex = 32;
			this.lblProfiles_SnapSettings_Brightness.Text = "Brightness: x (Min: -255 Max: 255)";
			this.lblProfiles_SnapSettings_Brightness.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// tbProfiles_SnapSettings_Brightness
			// 
			this.tbProfiles_SnapSettings_Brightness.LargeChange = 10;
			this.tbProfiles_SnapSettings_Brightness.Location = new System.Drawing.Point(8, 160);
			this.tbProfiles_SnapSettings_Brightness.Maximum = 510;
			this.tbProfiles_SnapSettings_Brightness.Name = "tbProfiles_SnapSettings_Brightness";
			this.tbProfiles_SnapSettings_Brightness.Size = new System.Drawing.Size(208, 40);
			this.tbProfiles_SnapSettings_Brightness.TabIndex = 31;
			this.tbProfiles_SnapSettings_Brightness.TickFrequency = 10;
			this.tbProfiles_SnapSettings_Brightness.Scroll += new System.EventHandler(this.tbProfiles_SnapSettings_Brightness_Scroll);
			// 
			// lblProfiles_SnapSettings_Contrast
			// 
			this.lblProfiles_SnapSettings_Contrast.Location = new System.Drawing.Point(8, 80);
			this.lblProfiles_SnapSettings_Contrast.Name = "lblProfiles_SnapSettings_Contrast";
			this.lblProfiles_SnapSettings_Contrast.Size = new System.Drawing.Size(208, 16);
			this.lblProfiles_SnapSettings_Contrast.TabIndex = 30;
			this.lblProfiles_SnapSettings_Contrast.Text = "Contrast: x (Min: -100 Max: 200)";
			this.lblProfiles_SnapSettings_Contrast.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// tbProfiles_SnapSettings_Contrast
			// 
			this.tbProfiles_SnapSettings_Contrast.LargeChange = 10;
			this.tbProfiles_SnapSettings_Contrast.Location = new System.Drawing.Point(8, 96);
			this.tbProfiles_SnapSettings_Contrast.Maximum = 200;
			this.tbProfiles_SnapSettings_Contrast.Name = "tbProfiles_SnapSettings_Contrast";
			this.tbProfiles_SnapSettings_Contrast.Size = new System.Drawing.Size(208, 40);
			this.tbProfiles_SnapSettings_Contrast.TabIndex = 29;
			this.tbProfiles_SnapSettings_Contrast.TickFrequency = 5;
			this.tbProfiles_SnapSettings_Contrast.Scroll += new System.EventHandler(this.tbProfiles_SnapSettings_Contrast_Scroll);
			// 
			// lblProfiles_SnapSettings_Gamma
			// 
			this.lblProfiles_SnapSettings_Gamma.Location = new System.Drawing.Point(8, 16);
			this.lblProfiles_SnapSettings_Gamma.Name = "lblProfiles_SnapSettings_Gamma";
			this.lblProfiles_SnapSettings_Gamma.Size = new System.Drawing.Size(208, 16);
			this.lblProfiles_SnapSettings_Gamma.TabIndex = 28;
			this.lblProfiles_SnapSettings_Gamma.Text = "Gamma: x (Min: 0.2 Max: 5.0)";
			this.lblProfiles_SnapSettings_Gamma.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// tbProfiles_SnapSettings_Gamma
			// 
			this.tbProfiles_SnapSettings_Gamma.LargeChange = 10;
			this.tbProfiles_SnapSettings_Gamma.Location = new System.Drawing.Point(8, 32);
			this.tbProfiles_SnapSettings_Gamma.Maximum = 50;
			this.tbProfiles_SnapSettings_Gamma.Minimum = 2;
			this.tbProfiles_SnapSettings_Gamma.Name = "tbProfiles_SnapSettings_Gamma";
			this.tbProfiles_SnapSettings_Gamma.Size = new System.Drawing.Size(208, 40);
			this.tbProfiles_SnapSettings_Gamma.TabIndex = 27;
			this.tbProfiles_SnapSettings_Gamma.Value = 2;
			this.tbProfiles_SnapSettings_Gamma.Scroll += new System.EventHandler(this.tbProfiles_SnapSettings_Gamma_Scroll);
			// 
			// lblProfiles_SnapSettings_Save
			// 
			this.lblProfiles_SnapSettings_Save.Location = new System.Drawing.Point(8, 40);
			this.lblProfiles_SnapSettings_Save.Name = "lblProfiles_SnapSettings_Save";
			this.lblProfiles_SnapSettings_Save.Size = new System.Drawing.Size(40, 16);
			this.lblProfiles_SnapSettings_Save.TabIndex = 22;
			this.lblProfiles_SnapSettings_Save.Text = "Save: ";
			this.lblProfiles_SnapSettings_Save.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// cbProfiles_SnapSettings_SaveType
			// 
			this.cbProfiles_SnapSettings_SaveType.Items.AddRange(new object[] {
																				  "Only Snaps",
																				  "Only Animations",
																				  "Snaps & Animations"});
			this.cbProfiles_SnapSettings_SaveType.Location = new System.Drawing.Point(48, 40);
			this.cbProfiles_SnapSettings_SaveType.Name = "cbProfiles_SnapSettings_SaveType";
			this.cbProfiles_SnapSettings_SaveType.Size = new System.Drawing.Size(184, 21);
			this.cbProfiles_SnapSettings_SaveType.TabIndex = 21;
			this.cbProfiles_SnapSettings_SaveType.Text = "Save Options";
			// 
			// grpProfiles_SnapSettings_SnapDir
			// 
			this.grpProfiles_SnapSettings_SnapDir.Controls.Add(this.txtProfiles_SnapSettings_SnapDir);
			this.grpProfiles_SnapSettings_SnapDir.Controls.Add(this.cmdProfiles_BrowseSnapDir);
			this.grpProfiles_SnapSettings_SnapDir.Location = new System.Drawing.Point(8, 192);
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
			// grpProfiles_SnapSettings_TimingAndParameters
			// 
			this.grpProfiles_SnapSettings_TimingAndParameters.Controls.Add(this.lblProfiles_SnapSettings_MultiSnapDelay);
			this.grpProfiles_SnapSettings_TimingAndParameters.Controls.Add(this.udProfiles_SnapSettings_MultiSnapDelay);
			this.grpProfiles_SnapSettings_TimingAndParameters.Controls.Add(this.lblProfiles_SnapSettings_SnapCount);
			this.grpProfiles_SnapSettings_TimingAndParameters.Controls.Add(this.udProfiles_SnapSettings_SnapCount);
			this.grpProfiles_SnapSettings_TimingAndParameters.Controls.Add(this.lblProfiles_SnapSettings_NextSnapDelay);
			this.grpProfiles_SnapSettings_TimingAndParameters.Controls.Add(this.udProfiles_SnapSettings_NextSnapDelay);
			this.grpProfiles_SnapSettings_TimingAndParameters.Controls.Add(this.lblProfiles_SnapSettings_SnapDelay);
			this.grpProfiles_SnapSettings_TimingAndParameters.Controls.Add(this.udProfiles_SnapSettings_Delay);
			this.grpProfiles_SnapSettings_TimingAndParameters.Location = new System.Drawing.Point(8, 64);
			this.grpProfiles_SnapSettings_TimingAndParameters.Name = "grpProfiles_SnapSettings_TimingAndParameters";
			this.grpProfiles_SnapSettings_TimingAndParameters.Size = new System.Drawing.Size(224, 120);
			this.grpProfiles_SnapSettings_TimingAndParameters.TabIndex = 7;
			this.grpProfiles_SnapSettings_TimingAndParameters.TabStop = false;
			this.grpProfiles_SnapSettings_TimingAndParameters.Text = "Timing && Parameters";
			// 
			// lblProfiles_SnapSettings_MultiSnapDelay
			// 
			this.lblProfiles_SnapSettings_MultiSnapDelay.Location = new System.Drawing.Point(8, 88);
			this.lblProfiles_SnapSettings_MultiSnapDelay.Name = "lblProfiles_SnapSettings_MultiSnapDelay";
			this.lblProfiles_SnapSettings_MultiSnapDelay.Size = new System.Drawing.Size(96, 16);
			this.lblProfiles_SnapSettings_MultiSnapDelay.TabIndex = 31;
			this.lblProfiles_SnapSettings_MultiSnapDelay.Text = "MultiSnap Delay:";
			this.lblProfiles_SnapSettings_MultiSnapDelay.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// udProfiles_SnapSettings_MultiSnapDelay
			// 
			this.udProfiles_SnapSettings_MultiSnapDelay.Location = new System.Drawing.Point(104, 88);
			this.udProfiles_SnapSettings_MultiSnapDelay.Maximum = new System.Decimal(new int[] {
																								   10000,
																								   0,
																								   0,
																								   0});
			this.udProfiles_SnapSettings_MultiSnapDelay.Minimum = new System.Decimal(new int[] {
																								   1,
																								   0,
																								   0,
																								   0});
			this.udProfiles_SnapSettings_MultiSnapDelay.Name = "udProfiles_SnapSettings_MultiSnapDelay";
			this.udProfiles_SnapSettings_MultiSnapDelay.Size = new System.Drawing.Size(112, 20);
			this.udProfiles_SnapSettings_MultiSnapDelay.TabIndex = 30;
			this.udProfiles_SnapSettings_MultiSnapDelay.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.udProfiles_SnapSettings_MultiSnapDelay.Value = new System.Decimal(new int[] {
																								 1,
																								 0,
																								 0,
																								 0});
			// 
			// lblProfiles_SnapSettings_SnapCount
			// 
			this.lblProfiles_SnapSettings_SnapCount.Location = new System.Drawing.Point(8, 64);
			this.lblProfiles_SnapSettings_SnapCount.Name = "lblProfiles_SnapSettings_SnapCount";
			this.lblProfiles_SnapSettings_SnapCount.Size = new System.Drawing.Size(96, 16);
			this.lblProfiles_SnapSettings_SnapCount.TabIndex = 29;
			this.lblProfiles_SnapSettings_SnapCount.Text = "Snap Count:";
			this.lblProfiles_SnapSettings_SnapCount.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// udProfiles_SnapSettings_SnapCount
			// 
			this.udProfiles_SnapSettings_SnapCount.Location = new System.Drawing.Point(104, 64);
			this.udProfiles_SnapSettings_SnapCount.Maximum = new System.Decimal(new int[] {
																							  20,
																							  0,
																							  0,
																							  0});
			this.udProfiles_SnapSettings_SnapCount.Minimum = new System.Decimal(new int[] {
																							  1,
																							  0,
																							  0,
																							  0});
			this.udProfiles_SnapSettings_SnapCount.Name = "udProfiles_SnapSettings_SnapCount";
			this.udProfiles_SnapSettings_SnapCount.Size = new System.Drawing.Size(112, 20);
			this.udProfiles_SnapSettings_SnapCount.TabIndex = 28;
			this.udProfiles_SnapSettings_SnapCount.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.udProfiles_SnapSettings_SnapCount.Value = new System.Decimal(new int[] {
																							1,
																							0,
																							0,
																							0});
			// 
			// lblProfiles_SnapSettings_NextSnapDelay
			// 
			this.lblProfiles_SnapSettings_NextSnapDelay.Location = new System.Drawing.Point(8, 40);
			this.lblProfiles_SnapSettings_NextSnapDelay.Name = "lblProfiles_SnapSettings_NextSnapDelay";
			this.lblProfiles_SnapSettings_NextSnapDelay.Size = new System.Drawing.Size(96, 16);
			this.lblProfiles_SnapSettings_NextSnapDelay.TabIndex = 27;
			this.lblProfiles_SnapSettings_NextSnapDelay.Text = "Next Snap Delay:";
			this.lblProfiles_SnapSettings_NextSnapDelay.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// udProfiles_SnapSettings_NextSnapDelay
			// 
			this.udProfiles_SnapSettings_NextSnapDelay.Location = new System.Drawing.Point(104, 40);
			this.udProfiles_SnapSettings_NextSnapDelay.Maximum = new System.Decimal(new int[] {
																								  10000,
																								  0,
																								  0,
																								  0});
			this.udProfiles_SnapSettings_NextSnapDelay.Name = "udProfiles_SnapSettings_NextSnapDelay";
			this.udProfiles_SnapSettings_NextSnapDelay.Size = new System.Drawing.Size(112, 20);
			this.udProfiles_SnapSettings_NextSnapDelay.TabIndex = 26;
			this.udProfiles_SnapSettings_NextSnapDelay.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// lblProfiles_SnapSettings_SnapDelay
			// 
			this.lblProfiles_SnapSettings_SnapDelay.Location = new System.Drawing.Point(8, 16);
			this.lblProfiles_SnapSettings_SnapDelay.Name = "lblProfiles_SnapSettings_SnapDelay";
			this.lblProfiles_SnapSettings_SnapDelay.Size = new System.Drawing.Size(72, 16);
			this.lblProfiles_SnapSettings_SnapDelay.TabIndex = 23;
			this.lblProfiles_SnapSettings_SnapDelay.Text = "Snap Delay:";
			this.lblProfiles_SnapSettings_SnapDelay.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// udProfiles_SnapSettings_Delay
			// 
			this.udProfiles_SnapSettings_Delay.Location = new System.Drawing.Point(104, 16);
			this.udProfiles_SnapSettings_Delay.Maximum = new System.Decimal(new int[] {
																						  10000,
																						  0,
																						  0,
																						  0});
			this.udProfiles_SnapSettings_Delay.Name = "udProfiles_SnapSettings_Delay";
			this.udProfiles_SnapSettings_Delay.Size = new System.Drawing.Size(112, 20);
			this.udProfiles_SnapSettings_Delay.TabIndex = 22;
			this.udProfiles_SnapSettings_Delay.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
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
			this.tabProfiles_StatsSettings.Size = new System.Drawing.Size(272, 318);
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
			this.grpProfiles_StatsSettingsSub.Size = new System.Drawing.Size(256, 257);
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
			this.cmdProfiles_StatsSettings_Reset.Location = new System.Drawing.Point(192, 288);
			this.cmdProfiles_StatsSettings_Reset.Name = "cmdProfiles_StatsSettings_Reset";
			this.cmdProfiles_StatsSettings_Reset.Size = new System.Drawing.Size(72, 24);
			this.cmdProfiles_StatsSettings_Reset.TabIndex = 6;
			this.cmdProfiles_StatsSettings_Reset.Text = "Reset";
			this.cmdProfiles_StatsSettings_Reset.Click += new System.EventHandler(this.cmdProfiles_StatsSettings_Reset_Click);
			// 
			// tabLog
			// 
			this.tabLog.Controls.Add(this.rtxtLog);
			this.tabLog.Location = new System.Drawing.Point(4, 25);
			this.tabLog.Name = "tabLog";
			this.tabLog.Size = new System.Drawing.Size(440, 379);
			this.tabLog.TabIndex = 3;
			this.tabLog.Text = "Log";
			// 
			// rtxtLog
			// 
			this.rtxtLog.Location = new System.Drawing.Point(4, 8);
			this.rtxtLog.Name = "rtxtLog";
			this.rtxtLog.ReadOnly = true;
			this.rtxtLog.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.ForcedVertical;
			this.rtxtLog.Size = new System.Drawing.Size(436, 368);
			this.rtxtLog.TabIndex = 11;
			this.rtxtLog.Text = "";
			this.rtxtLog.LinkClicked += new System.Windows.Forms.LinkClickedEventHandler(this.rtxtLog_LinkClicked);
			// 
			// tabAbout
			// 
			this.tabAbout.Controls.Add(this.rtxtAbout);
			this.tabAbout.Controls.Add(this.picAboutIcon);
			this.tabAbout.Location = new System.Drawing.Point(4, 25);
			this.tabAbout.Name = "tabAbout";
			this.tabAbout.Size = new System.Drawing.Size(440, 379);
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
			this.rtxtAbout.Size = new System.Drawing.Size(360, 360);
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
			// TimerMsg
			// 
			this.TimerMsg.Interval = 500;
			this.TimerMsg.Tick += new System.EventHandler(this.TimerMsg_Tick);
			// 
			// cmdApply
			// 
			this.cmdApply.Location = new System.Drawing.Point(72, 416);
			this.cmdApply.Name = "cmdApply";
			this.cmdApply.Size = new System.Drawing.Size(64, 24);
			this.cmdApply.TabIndex = 10;
			this.cmdApply.Text = "Apply";
			this.cmdApply.Click += new System.EventHandler(this.cmdApply_Click);
			// 
			// frmMain
			// 
			this.AutoScale = false;
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(450, 447);
			this.Controls.Add(this.cmdApply);
			this.Controls.Add(this.tabOptions);
			this.Controls.Add(this.cmdCancel);
			this.Controls.Add(this.cmdOK);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "frmMain";
			this.Text = "Smile!";
			this.WindowState = System.Windows.Forms.FormWindowState.Minimized;
			this.Closing += new System.ComponentModel.CancelEventHandler(this.frmMain_Closing);
			this.Load += new System.EventHandler(this.frmMain_Load);
			this.Closed += new System.EventHandler(this.frmMain_Closed);
			this.VisibleChanged += new System.EventHandler(this.frmMain_VisibleChanged);
			this.tabOptions.ResumeLayout(false);
			this.tabGeneral.ResumeLayout(false);
			this.tabGeneralOptions.ResumeLayout(false);
			this.tabGeneral_GlobalSnapSettings.ResumeLayout(false);
			this.grpGeneral_SnapSettings_AnimationSettings.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.udGeneral_SnapSettings_AnimFrameDelay)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.udGeneral_SnapSettings_AnimHeight)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.udGeneral_SnapSettings_AnimWidth)).EndInit();
			this.grpGeneral_SnapSettings_ImageFormat.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.tbGeneral_SnapSettings_Quality)).EndInit();
			this.grpGeneral_SnapSettings_SnapDir.ResumeLayout(false);
			this.grpGeneral_SnapSettings_TimingAndParameters.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.udGeneral_SnapSettings_NextSnapDelay)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.udGeneral_SnapSettings_SaveQueueSize)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.udGeneral_SnapSettings_Delay)).EndInit();
			this.tabGeneral_GlobalStatsSettings.ResumeLayout(false);
			this.tabGeneral_HotKeys.ResumeLayout(false);
			this.tabGeneral_Misc.ResumeLayout(false);
			this.grpGeneral_Misc_Updates.ResumeLayout(false);
			this.tabProfiles.ResumeLayout(false);
			this.tabProfileOptions.ResumeLayout(false);
			this.tabProfiles_GameSettings.ResumeLayout(false);
			this.grpProfiles_GameSettings_Path.ResumeLayout(false);
			this.tabProfiles_SnapSettings.ResumeLayout(false);
			this.grpProfiles_SnapSettingsSub.ResumeLayout(false);
			this.groupBox1.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.tbProfiles_SnapSettings_Brightness)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.tbProfiles_SnapSettings_Contrast)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.tbProfiles_SnapSettings_Gamma)).EndInit();
			this.grpProfiles_SnapSettings_SnapDir.ResumeLayout(false);
			this.grpProfiles_SnapSettings_TimingAndParameters.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.udProfiles_SnapSettings_MultiSnapDelay)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.udProfiles_SnapSettings_SnapCount)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.udProfiles_SnapSettings_NextSnapDelay)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.udProfiles_SnapSettings_Delay)).EndInit();
			this.tabProfiles_StatsSettings.ResumeLayout(false);
			this.grpProfiles_StatsSettingsSub.ResumeLayout(false);
			this.tabLog.ResumeLayout(false);
			this.tabAbout.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion

		/// The main entry point for the application.
		[STAThread]
		static void Main() 
		{
			using(SingleProgramInstance spi = new SingleProgramInstance("smiletray-{F1D19AEB-8EF6-41b9-AD38-3A84AE64AF53}"))
			{
				if (spi.IsSingleInstance)
				{
					// Start Main Form
					Application.Run(new frmMain());
				}
			}

		}

		private void frmMain_Load(object sender, System.EventArgs e)
		{
			Hide();
		}

		private void frmMain_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			if(!closing)
			{
				e.Cancel = true;
				this.Hide();
			}
		}

		private void frmMain_Closed(object sender, System.EventArgs e)
		{
			HKCaptureDesktop.Dispose();
			HKCaptureWindow.Dispose();
			HKCaptureActiveProfile.Dispose();
			GlobalKotKey.UnregisterHook();
			AddLogMessage("Unregistering system keyboard hook.");
			TimerSave.Dispose();
			TimerSave = null;
			AddLogMessage("SaveQueue shutting down.");
			SaveSettings();
			if(log != null)
			{
				log.WriteLine("\r\n\r\n-----Log Session Ended: " + DateTime.Now.ToLongDateString() + "-----\r\n\r\n");
				log.Flush();
				log.Close();
			}
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
			if(ApplySettings() == -1)
				return;
			Hide();
		}

		private void cmdApply_Click(object sender, System.EventArgs e)
		{
			ApplySettings();
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
					PopulateOptions();
					CopyProfile(Profiles[i], TempProfiles[i]);
				}
			}
		}

		private void rtxtLog_LinkClicked(object sender, System.Windows.Forms.LinkClickedEventArgs e)
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

		private void cmdProfiles_GameSettings_AutoDetect_Click(object sender, System.EventArgs e)
		{
			this.Enabled = false;
			txtProfiles_GameSettings_Path.Text = TempProfiles[ActiveTempProfile].GetDefaultPath();
			this.Enabled = true;
		}

		private void chkProfiles_SnapSettings_UseGlobal_CheckedChanged(object sender, System.EventArgs e)
		{
			grpProfiles_SnapSettingsSub.Enabled = !chkProfiles_SnapSettings_UseGlobal.Checked;
		}

		private void chkProfiles_StatsSettings_UseGlobal_CheckedChanged(object sender, System.EventArgs e)
		{
			grpProfiles_StatsSettingsSub.Enabled = !chkProfiles_StatsSettings_UseGlobal.Checked;
		}

		private void chkGeneral_SnapSettings_AnimUseMultiSnapDelay_CheckedChanged(object sender, System.EventArgs e)
		{
			lblGeneral_SnapSettings_AnimFrameDelay.Enabled = udGeneral_SnapSettings_AnimFrameDelay.Enabled = !chkGeneral_SnapSettings_AnimUseMultiSnapDelay.Checked;
		}

		private void chkGeneral_SnapSettings_AnimOriginalDimentions_CheckedChanged(object sender, System.EventArgs e)
		{
			lblGeneral_SnapSettings_AnimWidth.Enabled = lblGeneral_SnapSettings_AnimHeight.Enabled = udGeneral_SnapSettings_AnimWidth.Enabled = udGeneral_SnapSettings_AnimHeight.Enabled = !chkGeneral_SnapSettings_AnimOriginalDimentions.Checked;
		}

		private void tbProfiles_SnapSettings_Gamma_Scroll(object sender, System.EventArgs e)
		{
			lblProfiles_SnapSettings_Gamma.Text = "Gamma: " + String.Format("{0:f1}", (float)tbProfiles_SnapSettings_Gamma.Value/10) + " (Min: 0.2 Max: 5.0)";
		}

		private void tbProfiles_SnapSettings_Contrast_Scroll(object sender, System.EventArgs e)
		{
			lblProfiles_SnapSettings_Contrast.Text = "Contrast: " + String.Format("{0:D}", tbProfiles_SnapSettings_Contrast.Value-100) + " (Min: -100 Max: 200)";
		}

		private void tbProfiles_SnapSettings_Brightness_Scroll(object sender, System.EventArgs e)
		{
			lblProfiles_SnapSettings_Brightness.Text = "Brightness: " + String.Format("{0:D}", tbProfiles_SnapSettings_Brightness.Value-255) + " (Min: -255 Max: 255)";
		}

		private void HKCaptureDesktop_Pressed(object sender, System.EventArgs e)
		{
			if(Settings.HotKeySettings.Enabled && !DisableHotkeys)
			{
				AddLogMessage("HotKey: capturing desktop...");
				Image img = ScreenCapture.GetDesktopImage(true);
				ArrayList frames = new ArrayList(1);
				frames.Add(img);
				AddToSaveQueue(null, frames);
			}
		}

		private void HKCaptureWindow_Pressed(object sender, System.EventArgs e)
		{
			if(Settings.HotKeySettings.Enabled && !DisableHotkeys)
			{
				AddLogMessage("HotKey: capturing window...");
				Image img = ScreenCapture.GetDesktopImage(false);
				ArrayList frames = new ArrayList(1);
				frames.Add(img);
				AddToSaveQueue(null, frames);
			}
		}

		private void HKCaptureActiveProfile_Pressed(object sender, System.EventArgs e)
		{
			if(Settings.HotKeySettings.Enabled && !DisableHotkeys)
			{
				lock(ProfileLock)
				{
					if(ActiveProfile != null)
					{
						AddLogMessage("HotKey: Capturing active profile...");
						ActiveProfile.NewSnaps = true;		// Force Profile screenshot
					}
					else
					{
						AddLogMessage("HotKey: Cannot capture active profile, no profile running...");
					}
				}
			}
		}

		private void txtGeneral_HotKeys_CaptureWindow_Enter(object sender, System.EventArgs e)
		{
			DisableHotkeys = HKCaptureWindow.EnableKeyCapture = true;
		}

		private void txtGeneral_HotKeys_CaptureWindow_Leave(object sender, System.EventArgs e)
		{
			DisableHotkeys = HKCaptureWindow.EnableKeyCapture = false;
		}

		private void txtGeneral_HotKeys_CaptureDesktop_Enter(object sender, System.EventArgs e)
		{
			DisableHotkeys = HKCaptureDesktop.EnableKeyCapture = true;
		}

		private void txtGeneral_HotKeys_CaptureDesktop_Leave(object sender, System.EventArgs e)
		{
			DisableHotkeys = HKCaptureDesktop.EnableKeyCapture = false;
		}

		private void txtGeneral_HotKeys_CaptureActiveProfile_Enter(object sender, System.EventArgs e)
		{
			DisableHotkeys = HKCaptureActiveProfile.EnableKeyCapture = true;
		}

		private void txtGeneral_HotKeys_CaptureActiveProfile_Leave(object sender, System.EventArgs e)
		{
			DisableHotkeys = HKCaptureActiveProfile.EnableKeyCapture = false;
		}

		private void HKCaptureDesktop_KeyCapture(object sender, System.EventArgs e)
		{
			txtGeneral_HotKeys_CaptureDesktop.Text = HKCaptureDesktop.GetKeys().ToString();
		}

		private void HKCaptureWindow_KeyCapture(object sender, System.EventArgs e)
		{
			txtGeneral_HotKeys_CaptureWindow.Text = HKCaptureWindow.GetKeys().ToString();
		}

		private void HKCaptureActiveProfile_KeyCapture(object sender, System.EventArgs e)
		{
			txtGeneral_HotKeys_CaptureActiveProfile.Text = HKCaptureActiveProfile.GetKeys().ToString();
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
			closing = true;
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
		private void TimerCheckConsoleLog_Tick(object stateInfo)
		{
			if(TimerCheckConsoleLog != null)
				TimerCheckConsoleLog.Change(System.Threading.Timeout.Infinite, 0);	

			ExecCheckConsoleLog();

			if(TimerCheckConsoleLog != null)
			{
				if(ActiveProfile == null)
				{
					TimerCheckConsoleLog.Dispose();
					TimerCheckConsoleLog = null;
					AddLogMessage("Scanner shutting down.");
				}
				else
					TimerCheckConsoleLog.Change(10, 10);
			}
		}

		// Timer that handles the Msg/Log for the form
		// Since no form items are thread safe
		private void TimerMsg_Tick(object sender, System.EventArgs e)
		{
			lock(MsgLock)
			{
				while(MsgQueue.Count() > 0)
				{
					String msg = (String)MsgQueue.ObjectAt(0);
					MsgQueue.RemoveAt(0);
					rtxtLog.SelectionStart = rtxtLog.Text.Length;
					rtxtLog.SelectedText = msg + "\n";
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
					AddLogMessage("Ending Session For: " + ActiveProfile.ProfileName);
					lock(ProfileLock)
					{
						ActiveProfile.Close();
						ActiveProfile = null;
					}
					SaveSettings();
					return;
				}
				if(!ActiveProfile.IsOpen())
				{
					ActiveProfile.Open();
					if(ActiveProfile.IsOpen())
					{
						AddLogMessage("Begining Session For: " + ActiveProfile.ProfileName);
						UpdateActiveProfileDependancies(false);
						NextSnapNo = 0;
					}
					else
					{
						AddLogMessage("Error Starting Session For: " + ActiveProfile.ProfileName);
					}
				}

				if(ActiveProfile.NewStats)
				{
					SaveSettings();
					lock(ProfileLock)
					{
						ActiveProfile.NewStats = false;
					}
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
						lock(ProfileLock)
						{
							ActiveProfile = profile;
						}
						CheckEnabled();

						TimerCheckConsoleLogDelegate = new TimerCallback(TimerCheckConsoleLog_Tick);
						TimerCheckConsoleLog = new System.Threading.Timer(TimerCheckConsoleLogDelegate, null, 10, 10);
						AddLogMessage("Scanner starting up.");
						break;
					}
				}

				if(versionCheckDelay > 0 && ActiveProfile == null && lastVersionCheck + versionCheckDelay < CurrentTimeInSeconds())
				{
					AddLogMessage("Checking for new version...");
					switch(vc.Check(Info.intversion))
					{
						case Error.NewVersion:
							TimerMisc.Enabled = false;
							AddLogMessage("New Version Available!: " + vc.vd.version + " -- " + vc.vd.downloadurl);
							Ex.Msg("New Version", "New Version Available!: " + vc.vd.version + "\n" + 
								"Date: " + vc.vd.date + "\n" +
								"Time: " + vc.vd.time + "\n" +
								"Download from: " + vc.vd.downloadurl + "\n\n" +
								"Additional Info:\n" + vc.vd.comments, 640, 480);
							TimerMisc.Enabled = true;
							break;
						case Error.NoNewVersion:
							AddLogMessage("Version up to date.");
							break;
						case Error.ServerError:
							AddLogMessage("Error: Cannot Connect To " + vc.URL);
							break;
						case Error.ParseError:
							AddLogMessage("Error: Failed Parsing + " + vc.URL);
							break;
						case Error.StringMissMatch:
							AddLogMessage("Error: Version string type mismatch");
							break;
						case Error.Fail:
							AddLogMessage("Error: An unknown error has occured while checking for the latest version.");
							break;
					}
					lastVersionCheck =  CurrentTimeInSeconds();
				}
			}
		}

		
		private void TimerSave_Tick(object stateInfo)
		{
			lock(SaveThreadCountLock)
			{
				
				if(NumSaveThreads >= 3)
				{
					lock(QueueLock)
					{
						if(TimerSave != null)
						{
							TimerSave.Change(System.Threading.Timeout.Infinite, 0);
							return;
						}
					}
				}
				NumSaveThreads++;
			}

			ExecSaveQueue();
			
			lock(QueueLock)
			{
				if(TimerSave != null)
				{
					lock(SaveThreadCountLock)
					{
						NumSaveThreads--;
					}
					TimerSave.Change(100, 100);
				}
			}
		}


		//////////////////////////////////////////////////////////////////////////////////////////////
		/// Helper Methods
		//////////////////////////////////////////////////////////////////////////////////////////////

		private void AddLogMessage(String msg)
		{
			lock(MsgLock)
			{
				string s = String.Format("[{0:D2}:{1:D2}:{2:D2}] ", DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second) + msg;
				MsgQueue.Add(s);
				if(log != null)
				{
					log.WriteLine(s);
					log.Flush();
				}
			}
		}

		// Populate Options Form
		private void PopulateOptions()
		{
			// Global Snap Settings
			chkGeneral_SnapSettings_Enabled.Checked = Settings.SnapSettings.Enabled;
			udGeneral_SnapSettings_Delay.Value = Settings.SnapSettings.Delay;

			if(Settings.SnapSettings.SaveQueueSize >= 10 && Settings.SnapSettings.SaveQueueSize <= 500)
				udGeneral_SnapSettings_SaveQueueSize.Value = Settings.SnapSettings.SaveQueueSize;
			else
				udGeneral_SnapSettings_SaveQueueSize.Value = 100;
			udGeneral_SnapSettings_NextSnapDelay.Value = Settings.SnapSettings.NextSnapDelay;
			txtGeneral_SnapSettings_SnapDir.Text = Settings.SnapSettings.SnapDir;
			tbGeneral_SnapSettings_Quality.Value = Settings.SnapSettings.Quality;
			tbGeneral_SnapSettings_Quality_Scroll(this, null);
			cbGeneral_SnapSettings_ImageFormat.SelectedItem = Settings.SnapSettings.Encoder;
			chkGeneral_SnapSettings_SaveBug.Checked = Settings.SnapSettings.SaveBug;
			chkGeneral_SnapSettings_AnimOriginalDimentions.Checked = Settings.SnapSettings.AnimOriginalDimentions;
			chkGeneral_SnapSettings_AnimUseMultiSnapDelay.Checked = Settings.SnapSettings.AnimUseMultiSnapDelay;
			udGeneral_SnapSettings_AnimWidth.Value = Settings.SnapSettings.AnimWidth;
			udGeneral_SnapSettings_AnimHeight.Value = Settings.SnapSettings.AnimHeight;
			udGeneral_SnapSettings_AnimFrameDelay.Value = Math.Max(1, Settings.SnapSettings.AnimFrameDelay);
			lblGeneral_SnapSettings_AnimFrameDelay.Enabled = udGeneral_SnapSettings_AnimFrameDelay.Enabled = !chkGeneral_SnapSettings_AnimUseMultiSnapDelay.Checked;
			lblGeneral_SnapSettings_AnimWidth.Enabled = lblGeneral_SnapSettings_AnimHeight.Enabled = udGeneral_SnapSettings_AnimWidth.Enabled = udGeneral_SnapSettings_AnimHeight.Enabled = !chkGeneral_SnapSettings_AnimOriginalDimentions.Checked;
			chkGeneral_SnapSettings_AnimOptimizePalette.Checked = Settings.SnapSettings.AnimOptimizePalette;

			// Global Stats Settings
			chkGeneral_StatsSettings_Enabled.Checked = Settings.StatsSettings.Enabled;

			// Hot Keys
			if(GlobalKotKey.StringToKeyCombo(Settings.HotKeySettings.HKWindow) != null)
				txtGeneral_HotKeys_CaptureWindow.Text = Settings.HotKeySettings.HKWindow;
			if(GlobalKotKey.StringToKeyCombo(Settings.HotKeySettings.HKDesktop) != null)
				txtGeneral_HotKeys_CaptureDesktop.Text = Settings.HotKeySettings.HKDesktop;
			if(GlobalKotKey.StringToKeyCombo(Settings.HotKeySettings.HKActiveProfile) != null)
				txtGeneral_HotKeys_CaptureActiveProfile.Text = Settings.HotKeySettings.HKActiveProfile;
			chkGeneral_HotKeys_Enabled.Checked = Settings.HotKeySettings.Enabled;

			// Misc options
			cbGeneral_Misc_CheckUpdates.SelectedItem  = Settings.MiscSettings.CheckUpdates;

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
			txtProfiles_SnapSettings_SnapDir.Text = p.SnapSettings.SnapDir;
			udProfiles_SnapSettings_Delay.Value = p.SnapSettings.Delay;
			udProfiles_SnapSettings_NextSnapDelay.Value = p.SnapSettings.NextSnapDelay;
			udProfiles_SnapSettings_MultiSnapDelay.Value = Math.Max(p.SnapSettings.MultiSnapDelay, 1);
			udProfiles_SnapSettings_SnapCount.Value = Math.Max(p.SnapSettings.SnapCount, 1);
			cbProfiles_SnapSettings_SaveType.SelectedItem  = p.SnapSettings.SaveType.ToString();
			// Gamma
			if(p.SnapSettings.Gamma >= 2 && p.SnapSettings.Gamma <= 50)
				tbProfiles_SnapSettings_Gamma.Value = p.SnapSettings.Gamma;
			else
				tbProfiles_SnapSettings_Gamma.Value = 10;
			tbProfiles_SnapSettings_Gamma_Scroll(this, null);
			// Contrast
			if(p.SnapSettings.Contrast >= -100 && p.SnapSettings.Contrast <= 100)
				tbProfiles_SnapSettings_Contrast.Value = p.SnapSettings.Contrast+100;
			else
				tbProfiles_SnapSettings_Contrast.Value = 100;
			tbProfiles_SnapSettings_Contrast_Scroll(this, null);
			// Brightness
			if(p.SnapSettings.Brightness >= -255 && p.SnapSettings.Brightness <= 255)
				tbProfiles_SnapSettings_Brightness.Value = p.SnapSettings.Brightness+255;
			else
				tbProfiles_SnapSettings_Brightness.Value = 255;
			tbProfiles_SnapSettings_Brightness_Scroll(this, null);

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
			p.SnapSettings.SnapDir = txtProfiles_SnapSettings_SnapDir.Text;
			p.SnapSettings.Delay = (int)udProfiles_SnapSettings_Delay.Value;
			p.SnapSettings.NextSnapDelay = (int)udProfiles_SnapSettings_NextSnapDelay.Value;
			p.SnapSettings.MultiSnapDelay = (int)udProfiles_SnapSettings_MultiSnapDelay.Value;
			p.SnapSettings.SnapCount = (int)udProfiles_SnapSettings_SnapCount.Value;
			p.SnapSettings.SaveType = cbProfiles_SnapSettings_SaveType.SelectedItem.ToString();
			p.SnapSettings.Gamma = tbProfiles_SnapSettings_Gamma.Value;
			p.SnapSettings.Contrast = tbProfiles_SnapSettings_Contrast.Value-100;
			p.SnapSettings.Brightness = tbProfiles_SnapSettings_Brightness.Value-255;

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
			b.SnapSettings.SnapDir = a.SnapSettings.SnapDir;
			b.SnapSettings.Delay = a.SnapSettings.Delay;
			b.SnapSettings.NextSnapDelay = a.SnapSettings.NextSnapDelay;
			b.SnapSettings.SnapCount = a.SnapSettings.SnapCount;
			b.SnapSettings.MultiSnapDelay = a.SnapSettings.MultiSnapDelay;
			b.SnapSettings.SaveType = a.SnapSettings.SaveType.ToString();
			b.SnapSettings.Gamma = a.SnapSettings.Gamma;
			b.SnapSettings.Contrast = a.SnapSettings.Contrast;
			b.SnapSettings.Brightness = a.SnapSettings.Brightness;

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

		private bool CleanSettings(string file)
		{
			string rProfileTag = "<\\s*Profile\\s+xsi:type=\"(\\w+)\"[^>]*>(.*?)</\\s*Profile\\s*>";	
			StreamReader sr = null;
			StreamWriter sw = null;
			MatchCollection mc;
			string strFile;
			try
			{
				sr = new StreamReader(file);
				strFile = sr.ReadToEnd();
				sr.Close();
			}
			catch
			{
				return false;
			}
			mc = Regex.Matches(strFile, rProfileTag, RegexOptions.Singleline);
			foreach(Match m in mc)
			{
				if(m.Success)
				{
					string strBlock = m.Groups[0].Value;
					bool found = false;
					foreach(Type p in ProfileTypes)
					{
						string strp = p.Name;
						string strv = m.Groups[1].Value;
						if(String.Compare(strp, strv) == 0)
							found = true;
					}
					if(!found)
					{
						strFile = strFile.Replace(strBlock, "");
						AddLogMessage("Removing Profile " + m.Groups[1].ToString() + " from " + Path.GetFileName(file));
					}
				}
			}
			if(mc.Count > 0)
			{
				try
				{
					sw = new StreamWriter(file, false);
					sw.Write(strFile);
					sw.Close();
				}
				catch
				{
					return false;
				}
			}
			return true;
		}
		private void LoadSettings_UnknownNode (object sender, XmlNodeEventArgs e)
		{
			XmlNodeType myNodeType = e.NodeType;
			AddLogMessage(String.Format("LoadSettings->UnknownNode: " + 
				"Name: {0} " + 
				"LocalName: {1} " + 
				"Namespace URI: {2}" + 
				"Text: {3} " + 
				"NodeType: {4} ",
				e.Name, e.LocalName, e.NamespaceURI, e.Text, myNodeType));											
		}

		private void LoadSettings_UnknownAttribute(object sender, XmlAttributeEventArgs e)
		{
			AddLogMessage(String.Format("LoadSettings->UnknownAttribute: " + 
				"Name: {0} " + 
				"InnerXML: {1} " + 
				"Line: {2}" + 
				"Position: {3} ",
				e.Attr.Name, e.Attr.InnerXml, e.LineNumber, e.LinePosition));
		}

		private void LoadSettings_UnknownElement(object sender, XmlElementEventArgs e)
		{
			AddLogMessage(String.Format("LoadSettings->UnknownElement: " + 
				"Name: {0} " + 
				"InnerXML: {1} " + 
				"Line: {2}" + 
				"Position: {3} ",
				e.Element.Name, e.Element.InnerXml, e.LineNumber, e.LinePosition));
		}

		private void LoadSettings_UnreferencedObject(object sender, UnreferencedObjectEventArgs e)
		{
			AddLogMessage(String.Format("LoadSettings->UnreferencedObject: " + 
				"ID: {0} " + 
				"Object: {1} ",
				e.UnreferencedId, e.UnreferencedObject));
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
					StreamReader reader = null;
					Smile_t Data = new Smile_t();

					// Load
					try
					{
						CleanSettings(FileName);
						serializer = new XmlSerializer(Data.GetType());
						//serializer.UnknownNode += new XmlNodeEventHandler(LoadSettings_UnknownNode);
						//serializer.UnknownAttribute += new XmlAttributeEventHandler(LoadSettings_UnknownAttribute);
						//serializer.UnknownElement += new XmlElementEventHandler(LoadSettings_UnknownElement);
						//serializer.UnreferencedObject += new UnreferencedObjectEventHandler(LoadSettings_UnreferencedObject);
						reader = new StreamReader(FileName);
						Data = (Smile_t) serializer.Deserialize(reader);
						reader.Close();
						Settings = Data.Settings;
						Profiles = Data.Profiles;
					} 
					catch
					{
						AddLogMessage("Error: Cannot parse " + FileName);
						if(reader != null)
							reader.Close();
						string NewPath = Path.GetDirectoryName(FileName)+ @"\~" + Path.GetFileNameWithoutExtension(FileName)+".bak";
						File.Move(FileName, NewPath);
						AddLogMessage("Moved old settings to: " + NewPath);
						
						// Now recreate the settings file, add an extra check to avoid a infinate recursion
						if(!File.Exists(FileName))
							LoadSettings();
					}

					foreach(CProfile profile in Profiles)
					{
						profile.fromXMLOperations();
					}

					AddProfiles ( ref Profiles );	// Add any new profiles

					AddLogMessage("Loaded Settings from: " + FileName);
				} 
				else 
				{
					Settings.SnapSettings.Enabled = Settings.StatsSettings.Enabled = true;
					Settings.SnapSettings.SnapDir = Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath) + @"\Snaps";
					Settings.SnapSettings.Delay = 75;
					Settings.SnapSettings.Quality = 85;
					Settings.SnapSettings.SaveBug = false;
					Settings.SnapSettings.Encoder = "image/jpeg";
					Settings.SnapSettings.SaveQueueSize = 100;
					Settings.SnapSettings.NextSnapDelay = 500;
					Settings.SnapSettings.AnimOriginalDimentions = false;
					Settings.SnapSettings.AnimUseMultiSnapDelay = true;
					Settings.SnapSettings.AnimFrameDelay = 35;
					Settings.SnapSettings.AnimWidth = 320;
					Settings.SnapSettings.AnimHeight = 240;
					Settings.SnapSettings.AnimOptimizePalette = true;
					Settings.HotKeySettings.Enabled = false;
					Settings.HotKeySettings.HKWindow = "F10";
					Settings.HotKeySettings.HKDesktop = "F11";
					Settings.HotKeySettings.HKActiveProfile = "F12";
					Settings.MiscSettings.CheckUpdates = "Every Day";

					AddProfiles ( ref Profiles );  // Just in case none where created

					SaveSettings();

					AddLogMessage(FileName + " does not exist, creating new settings.");
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
				Settings.MiscSettings.LastCheckTime = lastVersionCheck;
				Data.Settings = Settings;
				Data.Profiles = Profiles;

				XmlSerializer serializer;
				TextWriter writer;

				// Save 
				serializer =  new XmlSerializer(Data.GetType());
				writer = new StreamWriter(FileName);
				serializer.Serialize(writer, Data);

				writer.Close();

				AddLogMessage("Settings Saved.");
			} 
			catch ( Exception e ) 
			{
				Ex.DumpException(e);
			}
			return true;
		}

		public long CurrentTimeInSeconds()
		{
			return DateTime.Now.Ticks/10000000;
		}

		public long CurrentTimeInMilliseconds()
		{
			return DateTime.Now.Ticks/10000;
		}

		public static System.Array ResizeArray(System.Array oldArray, Type type, int newSize) 
		{
			int oldSize = oldArray == null ? 0 : oldArray.Length;
			System.Array newArray = System.Array.CreateInstance(type,newSize);
			int preserveLength = System.Math.Min(oldSize,newSize);
			if (preserveLength > 0)
				System.Array.Copy (oldArray,newArray,preserveLength);
			return newArray; 
		}

		private void AddProfiles(ref CProfile [] p)
		{
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
					p[length].SnapSettings.UseGlobal = p[length].StatsSettings.UseGlobal = true;
					p[length].SnapSettings.SnapCount = 1;
					p[length].SnapSettings.MultiSnapDelay = 35;
					p[length].SnapSettings.NextSnapDelay = Settings.SnapSettings.NextSnapDelay;
					p[length].SnapSettings.SnapDir = Settings.SnapSettings.SnapDir + @"\" + p[length].SnapName;
					p[length].SnapSettings.SaveType = "Only Snaps";
					p[length].SnapSettings.Gamma = 10; // Gamma*10
					p[length].SnapSettings.Contrast = 0;
					p[length].SnapSettings.Brightness = 0;
					length++;
				}
			}
		}

		private void AddToSaveQueue(CProfile p, ArrayList frames)
		{
			if(NumFrames + frames.Count > Settings.SnapSettings.SaveQueueSize)
			{
				AddLogMessage("SaveQueue full, dropping captured frames.");
				for(int i = 0; i < frames.Count; i++)
				{
					((Image)frames[i]).Dispose();
					frames[i] = null;
				}
				frames.Clear();
				frames.TrimToSize();
				frames = null;
			}
			else
			{
				lock(FrameLock)
				{
					NumFrames += frames.Count;
				}
				AddLogMessage("Adding " + frames.Count + " frames to SaveQueue (Total: " + NumFrames + ").");
				SaveQueueItem i = new SaveQueueItem(p, frames);
				lock(QueueLock)
				{
					SaveQueue.Add(i);
				}
			}
		}
		private void ExecCheckConsoleLog()
		{
			// Active Profile should not be modified at all by any other thread during this thread, 
			// others can wait as this is the most important method
			lock(ProfileLock)
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
				catch(Exception e)
				{
					AddLogMessage("Error: Parse() failed unexpectedly for " + ActiveProfile.ProfileName + " \"" + e.Message + "\"");
					// Try to reset the profile
					ActiveProfile.Close();
					ActiveProfile.Open();
				}
				if(EnableSnaps)
				{
					if(ActiveProfile.NewSnaps)
					{
						if(this.LastSnapTime + this.NextSnapDelay > CurrentTimeInMilliseconds())
						{
							ActiveProfile.NewSnaps = false;
							return;
						}

						try
						{
							Thread.Sleep(ActiveProfile.SnapSettings.UseGlobal ? Settings.SnapSettings.Delay : ActiveProfile.SnapSettings.Delay);
							Image img = ScreenCapture.GetDesktopImage(false);
							this.LastSnapTime = CurrentTimeInMilliseconds();

							// Take multiple screenshots if specified
							if(!ActiveProfile.SnapSettings.UseGlobal && ActiveProfile.SnapSettings.SnapCount > 1)
							{
								ArrayList frames = new ArrayList(ActiveProfile.SnapSettings.SnapCount);
								frames.Add(img);
								for(int i = 0; i < ActiveProfile.SnapSettings.SnapCount-1; i++)
								{
									Thread.Sleep(ActiveProfile.SnapSettings.MultiSnapDelay);
									img = ScreenCapture.GetDesktopImage(false);
									frames.Add(img);
									this.LastSnapTime = CurrentTimeInMilliseconds();
								}
								AddToSaveQueue(ActiveProfile, frames);
							} 
							else 
							{
								ArrayList frames = new ArrayList(1);
								frames.Add(img);
								AddToSaveQueue(ActiveProfile, frames);
								this.LastSnapTime = CurrentTimeInMilliseconds();
							}
							ActiveProfile.NewSnaps = false;
						}
						catch
						{
							AddLogMessage("Error Capturing/Saving Image (Check Paths/Encoders?)");
						}
					}
				}
			}
		}

		private void ExecSaveQueue()
		{
			int queuesize;
			lock(QueueLock)
			{
				queuesize = SaveQueue.Count();
			}
			while(queuesize > 0)
			{
				// estimated item count by current active profile
				if(NumFrames < 20 && LastSnapTime + 2000 > CurrentTimeInMilliseconds())
					return;

				SaveQueueItem item;
				lock(QueueLock)
				{
					item = (SaveQueueItem)SaveQueue.ObjectAt(0);
					SaveQueue.RemoveAt(0);
				}
				Thread.Sleep(1);
				ArrayList frames = item.frames;

				String dir = item.p != null ? item.p.SnapSettings.UseGlobal ? Settings.SnapSettings.SnapDir + @"\" + item.p.SnapName : item.p.SnapSettings.SnapDir : Settings.SnapSettings.SnapDir;
				if(!Directory.Exists(dir))
				{
					Directory.CreateDirectory(dir);
					AddLogMessage("Created Directory: " + dir);
				}
				Thread.Sleep(1);

				if(item.p != null)
				{
					// Do Gamma Adjust
					if(item.p.SnapSettings.Gamma != 10) // 10/10 = 1.0
					{
						float gamma = (float)item.p.SnapSettings.Gamma/10;
						foreach(Image img in frames)
						{
							Filters.Gamma((Bitmap)img, gamma, gamma, gamma);
							Thread.Sleep(10);
						}
					}

					// Do Contrast Adjust
					if(item.p.SnapSettings.Contrast != 0) // 10/10 = 1.0
					{
						foreach(Image img in frames)
						{
							Filters.Contrast((Bitmap)img, (sbyte)item.p.SnapSettings.Contrast);
							Thread.Sleep(10);
						}
					}

					// Do Brightness Adjust
					if(item.p.SnapSettings.Brightness != 0) // 10/10 = 1.0
					{
						foreach(Image img in frames)
						{
							Filters.Brightness((Bitmap)img, (sbyte)item.p.SnapSettings.Brightness);
							Thread.Sleep(10);
						}
					}

					int framedelay = Settings.SnapSettings.AnimUseMultiSnapDelay ? item.p.SnapSettings.MultiSnapDelay : Settings.SnapSettings.AnimFrameDelay;
					// Save copy of animation
					if	(
						!item.p.SnapSettings.UseGlobal && 
						(
						item.p.SnapSettings.SaveType.CompareTo("Only Animations") == 0 || 
						item.p.SnapSettings.SaveType.CompareTo("Snaps & Animations") == 0
						) 
						)
					{
						string file;
						if(!Settings.SnapSettings.AnimOriginalDimentions)
						{
							ArrayList newframes = new ArrayList(frames.Count);
							for(int i = 0; i < frames.Count; i++)
							{
								Image img = (Image)frames[i];
								Thread.Sleep(1);

								Image newimg = new Bitmap(Settings.SnapSettings.AnimWidth, Settings.SnapSettings.AnimHeight);
								Graphics g = Graphics.FromImage(newimg);
								// Resize Options
								g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
								g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
								g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
								g.DrawImage(img, new Rectangle(0, 0, Settings.SnapSettings.AnimWidth, Settings.SnapSettings.AnimHeight));

								newframes.Add(newimg);
								g.Dispose();
								g = null;
								Thread.Sleep(10);
							}
						
							lock(FileSaveLock)
							{
								file = GetNextSnapName(ref NextSnapNo, "smile", ".gif", dir);
								CGifFile.SaveAnimation(file, newframes, framedelay, Settings.SnapSettings.AnimOptimizePalette  );
							}
							Thread.Sleep(1);
							for(int i = 0; i < newframes.Count; i++)
							{
								((Image)newframes[i]).Dispose();
								newframes[i] = null;
							}
							newframes.Clear();
							newframes.TrimToSize();
							newframes = null;
						}
						else
						{
							lock(FileSaveLock)
							{
								file = GetNextSnapName(ref NextSnapNo, "smile", ".gif", dir);
								CGifFile.SaveAnimation(file, frames, framedelay, Settings.SnapSettings.AnimOptimizePalette );
							}
						}
						AddLogMessage("Saved Animation to: " + file);

						if(ActiveProfile != null)
							Thread.Sleep(100);	
					}
				}
				
				if	(	item.p == null ||
						(
							item.p.SnapSettings.UseGlobal && 
							Settings.SnapSettings.Enabled
						) || 
						(	!item.p.SnapSettings.UseGlobal && 
							(item.p.SnapSettings.SaveType.CompareTo("Only Snaps") == 0 || 
							item.p.SnapSettings.SaveType.CompareTo("Snaps & Animations") == 0) 
						)
					)
				{
					for(int i = 0; i < frames.Count; i++)
					{
						Image img = (Image)frames[i];

						Graphics g = Graphics.FromImage(img);
						g.DrawString(encoderstring, encoderfont , new SolidBrush(Color.FromArgb(75, Color.White)), img.Width - g.MeasureString(encoderstring, encoderfont).Width - 5, img.Height - g.MeasureString(encoderstring, encoderfont).Height - 5);
						
						string file;
						lock(FileSaveLock)
						{
							if(item.p != null)
								file = GetNextSnapName(ref NextSnapNo, "smile", encoderext, dir);
							else
								file = GetNextSnapName(ref GlobalNextSnapNo, "smile", encoderext, dir);
							img.Save(file, encoder, encoderParams);
							if(Settings.SnapSettings.SaveBug)
							{
								for(int j = 0; !File.Exists(file) && j < 8; j++)
								{
									img.Save(file, encoder, encoderParams);
								}
							}
						}
						AddLogMessage("Saved Image to: " + file);

						g.Dispose();
						g = null;
						img.Dispose();
						img = null;	

						if(ActiveProfile != null)
							Thread.Sleep(100);	
					}
				}
				lock(FrameLock)
				{
					NumFrames -= frames.Count;
				}
				for(int i = 0; i < frames.Count; i++)
				{
					((Image)frames[i]).Dispose();
					frames[i] = null;
				}
				item = null;
				frames.Clear();
				frames.TrimToSize();
				frames = null;
				lock(QueueLock)
				{
					queuesize = SaveQueue.Count();
				}
				AddLogMessage(NumFrames + " frames left in the Save Queue.");
			}
		}

		private void UpdateCheckDelay()
		{
			switch(Settings.MiscSettings.CheckUpdates)
			{
				case "Never":
					versionCheckDelay = -1;
					break;
				case "Every Hour":
					versionCheckDelay = 3600;
					break;
				case "Every Day":
					versionCheckDelay = 86400;
					break;
				case "Every Week":
					versionCheckDelay = 604800;
					break;
				case "Every Month":
					versionCheckDelay = 2629743;
					break;
			}
		}

		private void UpdateActiveProfileDependancies(bool restart)
		{
			// Lock profile here because once we read from it, and it becomes null we may lose data
			// TimerMisc is the only one that can assume it wont be null at a certain point
			lock(ProfileLock)
			{
				if(ActiveProfile != null)
				{
					this.NextSnapDelay = ActiveProfile.SnapSettings.UseGlobal ? Settings.SnapSettings.NextSnapDelay :ActiveProfile.SnapSettings.NextSnapDelay;

					// restart reading just in case paths have changed
					if(restart)
					{
						ActiveProfile.Close();
						ActiveProfile.Open();
					}

					if(!mnuEnableSnaps.Checked)
						EnableSnaps = false;
					else if (ActiveProfile.SnapSettings.UseGlobal && Settings.SnapSettings.Enabled)
						EnableSnaps = true;
					else if (!ActiveProfile.SnapSettings.UseGlobal && ActiveProfile.SnapSettings.Enabled)
						EnableSnaps = true;
					else 
						EnableSnaps = false;

					if(!mnuEnableStats.Checked)
						EnableStats = false;
					else if (ActiveProfile.StatsSettings.UseGlobal && Settings.StatsSettings.Enabled)
						EnableStats = true;
					else if (!ActiveProfile.StatsSettings.UseGlobal && ActiveProfile.StatsSettings.Enabled)
						EnableStats = true;
					else 
						EnableStats = false;
				}
			}
		}

		// Need to assume ext is in this->extlist!
		private string GetNextSnapName(ref int counter, string pre, string ext, string path)
		{
			bool exists;
			string name;
			do
			{
				exists = false;
				counter++;
				name = path + "\\"+ pre + String.Format("{0:D8}", counter);

				foreach(string item in extlist)
				{
					if(File.Exists(name + item))
					{
						exists = true;
						break;
					}
					Thread.Sleep(1);
				}
			}
			while(exists);
			return name + ext;
		}

		private int ApplySettings()
		{
			// Hot Keys
			GlobalKotKey.keycombo hkindexCaptureWindow;
			GlobalKotKey.keycombo hkindexCaptureDesktop;
			GlobalKotKey.keycombo hkindexCaptureActiveProfile;
			if(txtGeneral_HotKeys_CaptureWindow.Text == null || (hkindexCaptureWindow = GlobalKotKey.StringToKeyCombo(txtGeneral_HotKeys_CaptureWindow.Text)) == null)
			{
				MessageBox.Show("Error: Invalid HotKey assigned to Capture Window.", "HotKey Error", MessageBoxButtons.OK);
				return -1;
			}
			if(txtGeneral_HotKeys_CaptureDesktop.Text == null || (hkindexCaptureDesktop = GlobalKotKey.StringToKeyCombo(txtGeneral_HotKeys_CaptureDesktop.Text)) == null)
			{
				MessageBox.Show("Error: Invalid HotKey assigned to Capture Desktop.", "HotKey Error", MessageBoxButtons.OK);
				return -1;
			}
			if(txtGeneral_HotKeys_CaptureActiveProfile.Text == null || (hkindexCaptureActiveProfile = GlobalKotKey.StringToKeyCombo(txtGeneral_HotKeys_CaptureActiveProfile.Text)) == null)
			{
				MessageBox.Show("Error: Invalid HotKey assigned to Capture Active Profile");
				return -1;
			}

			if(	String.Compare(txtGeneral_HotKeys_CaptureWindow.Text, txtGeneral_HotKeys_CaptureDesktop.Text, true) == 0 ||
				String.Compare(txtGeneral_HotKeys_CaptureWindow.Text, txtGeneral_HotKeys_CaptureActiveProfile.Text, true) == 0 ||
				String.Compare(txtGeneral_HotKeys_CaptureDesktop.Text, txtGeneral_HotKeys_CaptureActiveProfile.Text, true) == 0)
			{
				MessageBox.Show("Error: HotKey conflict, a key combination is assigned to more than one hotkey.", "HotKey Error", MessageBoxButtons.OK);
				return -1;
			}


			// Global Snap Settings
			Settings.SnapSettings.Enabled = chkGeneral_SnapSettings_Enabled.Checked;
			Settings.SnapSettings.Delay = (int)udGeneral_SnapSettings_Delay.Value;
			Settings.SnapSettings.SaveQueueSize = (int)udGeneral_SnapSettings_SaveQueueSize.Value;
			Settings.SnapSettings.NextSnapDelay = (int)udGeneral_SnapSettings_NextSnapDelay.Value;
			Settings.SnapSettings.SnapDir = txtGeneral_SnapSettings_SnapDir.Text;
			Settings.SnapSettings.Quality = tbGeneral_SnapSettings_Quality.Value;
			Settings.SnapSettings.Encoder = cbGeneral_SnapSettings_ImageFormat.SelectedItem.ToString();
			Settings.SnapSettings.SaveBug = chkGeneral_SnapSettings_SaveBug.Checked;
			Settings.SnapSettings.AnimOriginalDimentions = chkGeneral_SnapSettings_AnimOriginalDimentions.Checked;
			Settings.SnapSettings.AnimUseMultiSnapDelay = chkGeneral_SnapSettings_AnimUseMultiSnapDelay.Checked;
			Settings.SnapSettings.AnimWidth = (int)udGeneral_SnapSettings_AnimWidth.Value;
			Settings.SnapSettings.AnimHeight = (int)udGeneral_SnapSettings_AnimHeight.Value;
			Settings.SnapSettings.AnimFrameDelay = (int)udGeneral_SnapSettings_AnimFrameDelay.Value;
			Settings.SnapSettings.AnimOptimizePalette = chkGeneral_SnapSettings_AnimOptimizePalette.Checked;

			// Global Stats Settings
			Settings.StatsSettings.Enabled = chkGeneral_StatsSettings_Enabled.Checked;

			// Global Misc Settings
			Settings.MiscSettings.CheckUpdates = cbGeneral_Misc_CheckUpdates.SelectedItem.ToString();

			// Copy profiles
			// Copy last active profile
			SaveProfile(TempProfiles[ActiveTempProfile]);
			for(int i = 0; i < Profiles.Length; i++)
			{
				CopyProfile(TempProfiles[i], Profiles[i]);
			}
			// If this far assign hotkeys
			Settings.HotKeySettings.Enabled = chkGeneral_HotKeys_Enabled.Checked;
			Settings.HotKeySettings.HKWindow = txtGeneral_HotKeys_CaptureWindow.Text;
			HKCaptureWindow.Shortcut = hkindexCaptureWindow;
			Settings.HotKeySettings.HKDesktop = txtGeneral_HotKeys_CaptureDesktop.Text;
			HKCaptureDesktop.Shortcut = hkindexCaptureDesktop;
			Settings.HotKeySettings.HKActiveProfile = txtGeneral_HotKeys_CaptureActiveProfile.Text;
			HKCaptureActiveProfile.Shortcut = hkindexCaptureActiveProfile;

			SaveSettings();

			UpdateEncoder();

			UpdateActiveProfileDependancies(false);

			UpdateCheckDelay();

			CheckEnabled();

			return 0;
		}


	}
}
