/////////////////////////////////////////////////////////////////////////////
//
// Smile! -- Screenshot and Statistics Utility
// Copyright (c) 2005-2006 Marek Kudlacz
//
// http://kudlacz.com
//
/////////////////////////////////////////////////////////////////////////////

using System;
using System.Windows.Forms;
using System.Diagnostics;

namespace smiletray
{
	public class frmExMsgBox : System.Windows.Forms.Form
	{
		private System.Windows.Forms.Button cmdOK;
		private System.Windows.Forms.RichTextBox rtxtMsg;

		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public frmExMsgBox(string title, string message)
		{
			InitializeComponent();

			this.Text = title;
			this.rtxtMsg.Text = message;
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
			this.cmdOK = new System.Windows.Forms.Button();
			this.rtxtMsg = new System.Windows.Forms.RichTextBox();
			this.SuspendLayout();
			// 
			// cmdOK
			// 
			this.cmdOK.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
			this.cmdOK.Location = new System.Drawing.Point(96, 376);
			this.cmdOK.Name = "cmdOK";
			this.cmdOK.Size = new System.Drawing.Size(104, 24);
			this.cmdOK.TabIndex = 0;
			this.cmdOK.Text = "OK";
			this.cmdOK.Click += new System.EventHandler(this.cmdOK_Click);
			// 
			// rtxtMsg
			// 
			this.rtxtMsg.Dock = System.Windows.Forms.DockStyle.Top;
			this.rtxtMsg.Location = new System.Drawing.Point(0, 0);
			this.rtxtMsg.Name = "rtxtMsg";
			this.rtxtMsg.ReadOnly = true;
			this.rtxtMsg.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.ForcedVertical;
			this.rtxtMsg.Size = new System.Drawing.Size(306, 376);
			this.rtxtMsg.TabIndex = 3;
			this.rtxtMsg.Text = "";
			this.rtxtMsg.LinkClicked += new System.Windows.Forms.LinkClickedEventHandler(this.rtxtMsg_LinkClicked);
			// 
			// frmExMsgBox
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(306, 399);
			this.Controls.Add(this.rtxtMsg);
			this.Controls.Add(this.cmdOK);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "frmExMsgBox";
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "ExMessageBox";
			this.TopMost = true;
			this.Resize += new System.EventHandler(this.frmExMsgBox_Resize);
			this.Load += new System.EventHandler(this.frmExMsgBox_Load);
			this.ResumeLayout(false);

		}
		#endregion

		private void cmdOK_Click(object sender, System.EventArgs e)
		{
			this.Close();
		}

		private void frmExMsgBox_Resize(object sender, System.EventArgs e)
		{
			rtxtMsg.Height = cmdOK.Top;
		}

		private void frmExMsgBox_Load(object sender, System.EventArgs e)
		{
			frmExMsgBox_Resize(this, null);
		}

		private void rtxtMsg_LinkClicked(object sender, System.Windows.Forms.LinkClickedEventArgs e)
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
	}

	public class Ex
	{

		public static void DumpException( Exception ex )

		{
			WriteExceptionInfo( ex );                     
			if( null != ex.InnerException )               
			{                                             
				WriteExceptionInfo( ex.InnerException );    
			}
		}
		public static void WriteExceptionInfo( Exception ex )
		{
			MessageBox.Show( "--------- Exception Data ---------\n" 
				+ "Message: " + ex.Message + "\n"              
				+ "Exception Type: " + ex.GetType().FullName + "\n"
				+ "Source: " + ex.Source + "\n"               
				+ "StrackTrace: " + ex.StackTrace + "\n"         
				+ "TargetSite: " + ex.TargetSite + "\n",
				"Exception");
		}

		public static void WriteExceptionInfoBox( Exception ex )
		{
			Msg("Exception", "--------- Exception Data ---------\n" 
				+ "Message: " + ex.Message + "\n"              
				+ "Exception Type: " + ex.GetType().FullName + "\n"
				+ "Source: " + ex.Source + "\n"               
				+ "StrackTrace: " + ex.StackTrace + "\n"         
				+ "TargetSite: " + ex.TargetSite + "\n");
		}

		public static void DumpExceptionInfoBox( Exception ex )
		{
			string msg;
			msg = "--------- Exception Data ---------\n" 
				+ "Message: " + ex.Message + "\n"              
				+ "Exception Type: " + ex.GetType().FullName + "\n"
				+ "Source: " + ex.Source + "\n"               
				+ "StrackTrace: " + ex.StackTrace + "\n"         
				+ "TargetSite: " + ex.TargetSite + "\n";
				if( null != ex.InnerException )               
				{      
                    Exception iex = ex.InnerException;                    
					msg += "--------- Inner Exception Data ---------\n"
					+ "Message: " + iex.Message + "\n"              
					+ "Exception Type: " + iex.GetType().FullName + "\n"
					+ "Source: " + iex.Source + "\n"               
					+ "StrackTrace: " + iex.StackTrace + "\n"         
					+ "TargetSite: " + iex.TargetSite + "\n";  
				}
			Msg("Exception", msg);
		}

		public static void Msg(string title, string message, int width, int height)
		{
			message = message.Replace("\r\n", "\n");
			message = message.Replace("\n", "\r\n");
			Form f = new frmExMsgBox(title, message);
			f.Width = width;
			f.Height = height;
			f.ShowDialog();
			f.Dispose();
		}

		public static void Msg(string title, string message)
		{
			message = message.Replace("\r\n", "\n");
			message = message.Replace("\n", "\r\n");
			Form f = new frmExMsgBox(title, message);
			f.ShowDialog();
			f.Dispose();
		}
	}
}
