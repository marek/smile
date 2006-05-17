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
using System.Diagnostics;
using System.IO;

namespace smiletray
{
	public class frmViewStats : System.Windows.Forms.Form
	{
		// private members
		private int SelectedProfile;
		private CProfile [] profiles;
		// Form members
		private System.Windows.Forms.StatusBar statusBar;
		private System.Windows.Forms.RichTextBox rtxtStats;
		private System.Windows.Forms.MainMenu mainMenu;
		private System.Windows.Forms.MenuItem mnuFile;
		private System.Windows.Forms.MenuItem mnuFile_Close;
		private System.Windows.Forms.MenuItem menuItem1;
		private System.Windows.Forms.MenuItem mnuEdit_SaveClip;
		private System.Windows.Forms.MenuItem mnuEdit_SaveFile;
		private System.Windows.Forms.SaveFileDialog saveStatsDialog;
		private System.Windows.Forms.MenuItem mnuSelectGame;
		private System.ComponentModel.Container components = null;

		public void MenuHandler(object sender, EventArgs e)
		{ 
			MenuItem item = (MenuItem)sender;
			SelectedProfile = item.Index;
			rtxtStats.Rtf = profiles[SelectedProfile].GetStatsReport("Verdana", CProfile.SaveTypes.RTF);
		}

		public frmViewStats(CProfile [] profiles, int index)
		{
			this.profiles = profiles;
            Array.Sort(this.profiles);

			InitializeComponent();

			for(int i = 0; i < this.profiles.Length; i++)
			{
                MenuItem item = new MenuItem(this.profiles[i].ProfileName, new System.EventHandler(this.MenuHandler));
				mnuSelectGame.MenuItems.Add(i,item);
			}
			
			// Load first profile
			SelectedProfile = index;
            rtxtStats.Rtf = this.profiles[SelectedProfile].GetStatsReport("Verdana", CProfile.SaveTypes.RTF);
		}

		/// Clean up any resources being used.
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		private void InitializeComponent()
		{
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(frmViewStats));
			this.statusBar = new System.Windows.Forms.StatusBar();
			this.rtxtStats = new System.Windows.Forms.RichTextBox();
			this.mainMenu = new System.Windows.Forms.MainMenu();
			this.mnuFile = new System.Windows.Forms.MenuItem();
			this.mnuFile_Close = new System.Windows.Forms.MenuItem();
			this.menuItem1 = new System.Windows.Forms.MenuItem();
			this.mnuEdit_SaveClip = new System.Windows.Forms.MenuItem();
			this.mnuEdit_SaveFile = new System.Windows.Forms.MenuItem();
			this.saveStatsDialog = new System.Windows.Forms.SaveFileDialog();
			this.mnuSelectGame = new System.Windows.Forms.MenuItem();
			this.SuspendLayout();
			// 
			// statusBar
			// 
			this.statusBar.Location = new System.Drawing.Point(0, 427);
			this.statusBar.Name = "statusBar";
			this.statusBar.Size = new System.Drawing.Size(480, 22);
			this.statusBar.TabIndex = 0;
			// 
			// rtxtStats
			// 
			this.rtxtStats.AutoSize = true;
			this.rtxtStats.Dock = System.Windows.Forms.DockStyle.Fill;
			this.rtxtStats.Location = new System.Drawing.Point(0, 0);
			this.rtxtStats.Name = "rtxtStats";
			this.rtxtStats.ReadOnly = true;
			this.rtxtStats.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.ForcedVertical;
			this.rtxtStats.Size = new System.Drawing.Size(480, 427);
			this.rtxtStats.TabIndex = 2;
			this.rtxtStats.Text = "";
			this.rtxtStats.LinkClicked += new System.Windows.Forms.LinkClickedEventHandler(this.rtxtStats_LinkClicked);
			// 
			// mainMenu
			// 
			this.mainMenu.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																					 this.mnuFile,
																					 this.menuItem1,
																					 this.mnuSelectGame});
			// 
			// mnuFile
			// 
			this.mnuFile.Index = 0;
			this.mnuFile.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																					this.mnuFile_Close});
			this.mnuFile.Text = "File";
			// 
			// mnuFile_Close
			// 
			this.mnuFile_Close.Index = 0;
			this.mnuFile_Close.Text = "Close";
			this.mnuFile_Close.Click += new System.EventHandler(this.mnuFile_Close_Click);
			// 
			// menuItem1
			// 
			this.menuItem1.Index = 1;
			this.menuItem1.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																					  this.mnuEdit_SaveClip,
																					  this.mnuEdit_SaveFile});
			this.menuItem1.Text = "Edit";
			// 
			// mnuEdit_SaveClip
			// 
			this.mnuEdit_SaveClip.Index = 0;
			this.mnuEdit_SaveClip.Text = "Save To Clipboard";
			this.mnuEdit_SaveClip.Click += new System.EventHandler(this.mnuEdit_SaveClip_Click);
			// 
			// mnuEdit_SaveFile
			// 
			this.mnuEdit_SaveFile.Index = 1;
			this.mnuEdit_SaveFile.Text = "Save To File";
			this.mnuEdit_SaveFile.Click += new System.EventHandler(this.mnuEdit_SaveFile_Click);
			// 
			// saveStatsDialog
			// 
			this.saveStatsDialog.Filter = "HTML Document (*.html)|*.html|Rich Text Format (*.rtf)|*.rtf|Plain Text  Document" +
				" (*.txt)|*.txt";
			this.saveStatsDialog.Title = "Save Smile! Statistics:";
			// 
			// mnuSelectGame
			// 
			this.mnuSelectGame.Index = 2;
			this.mnuSelectGame.Text = "Select Game";
			// 
			// frmViewStats
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(480, 449);
			this.Controls.Add(this.rtxtStats);
			this.Controls.Add(this.statusBar);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.Menu = this.mainMenu;
			this.MinimizeBox = false;
			this.Name = "frmViewStats";
			this.ShowInTaskbar = false;
			this.Text = "Smile!: ViewStats";
			this.TopMost = true;
			this.ResumeLayout(false);

		}
		#endregion

		private void rtxtStats_LinkClicked(object sender, System.Windows.Forms.LinkClickedEventArgs e)
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

		private void mnuEdit_SaveClip_Click(object sender, System.EventArgs e)
		{
			Clipboard.SetDataObject(rtxtStats.Text);
		}

		private void mnuFile_Close_Click(object sender, System.EventArgs e)
		{
			Close();
		}

		private void mnuEdit_SaveFile_Click(object sender, System.EventArgs e)
		{
			if(saveStatsDialog.ShowDialog() == DialogResult.Cancel)
				return;
			
			try
			{
				switch(saveStatsDialog.FilterIndex)
				{
					case 1:
					{
						Stream stream = saveStatsDialog.OpenFile();
						StreamWriter html = new StreamWriter(stream, System.Text.Encoding.ASCII);
						html.Write(profiles[SelectedProfile].GetStatsReport("Verdana", CProfile.SaveTypes.HTML));
						html.Close();
						stream.Close();
						break;
					}
					case 2:
					{
						rtxtStats.SaveFile(saveStatsDialog.FileName, System.Windows.Forms.RichTextBoxStreamType.RichText);
						break;
					}
					case 3:
					{
						rtxtStats.SaveFile(saveStatsDialog.FileName, System.Windows.Forms.RichTextBoxStreamType.PlainText);
						break;
					}
				}
			}
			catch
			{
				MessageBox.Show("Error: ViewStats->Save To File: Cannot Save File");
			}
			
		}

	}
}
