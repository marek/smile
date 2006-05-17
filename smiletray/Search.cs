/////////////////////////////////////////////////////////////////////////////
//
// Smile! -- Screenshot and Statistics Utility
// Copyright (c) 2005 Marek Kudlacz
//
// http://kudlacz.com
//
/////////////////////////////////////////////////////////////////////////////


using System;
using System.IO;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Text.RegularExpressions;

namespace smiletray
{
	public class frmSearch : System.Windows.Forms.Form
	{
		private bool forcestop;

		private System.Windows.Forms.Label lblTitle;
		private System.Windows.Forms.Label lblCurrent;

		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public frmSearch()
		{
			InitializeComponent();
			forcestop = false;
		}

		public bool Stopped()
		{
			return forcestop;
		}

		public String Search(string title, String startpath, Regex r)
		{
			if(forcestop)
				return null;
			Match m;
			string [] entries;
			try
			{
				entries = Directory.GetFiles(startpath);
			}
			catch
			{
				return null;
			}
			lblTitle.Text = title;

			// Check self
			lblCurrent.Text = startpath;
			this.Update();
			m = r.Match(startpath);
			if(m.Success)
				return startpath;

			// Check files
			foreach(string entry in entries)
			{
				Application.DoEvents();
				lblCurrent.Text = entry;
				this.Update();
				m = r.Match(entry);
				if(m.Success)
					return entry;
			}
			
			// Recurse through dirs
			try
			{
				entries = Directory.GetDirectories(startpath);
			}
			catch
			{
				return null;
			}
			string result;
			foreach(string entry in entries)
			{
				Application.DoEvents();
				result = Search(title, entry, r);
				if(result != null)
					return result;
			}
			return null;
		}

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
			this.lblTitle = new System.Windows.Forms.Label();
			this.lblCurrent = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// lblTitle
			// 
			this.lblTitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.lblTitle.Location = new System.Drawing.Point(8, 8);
			this.lblTitle.Name = "lblTitle";
			this.lblTitle.Size = new System.Drawing.Size(296, 16);
			this.lblTitle.TabIndex = 0;
			this.lblTitle.Text = "Title";
			this.lblTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// lblCurrent
			// 
			this.lblCurrent.Location = new System.Drawing.Point(8, 32);
			this.lblCurrent.Name = "lblCurrent";
			this.lblCurrent.Size = new System.Drawing.Size(296, 32);
			this.lblCurrent.TabIndex = 1;
			this.lblCurrent.Text = "Current";
			this.lblCurrent.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// frmSearch
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(312, 71);
			this.Controls.Add(this.lblCurrent);
			this.Controls.Add(this.lblTitle);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "frmSearch";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Searching...";
			this.TopMost = true;
			this.Closing += new System.ComponentModel.CancelEventHandler(this.frmSearch_Closing);
			this.ResumeLayout(false);

		}
		#endregion

		private void frmSearch_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			forcestop = true;
		}
	}
}
