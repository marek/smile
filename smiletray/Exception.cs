/////////////////////////////////////////////////////////////////////////////
//
// Smile! -- Screenshot and Statistics Utility
// Copyright (c) 2005 Marek Kudlacz
//
// http://kudlacz.com
//
/////////////////////////////////////////////////////////////////////////////

using System;
using System.Windows.Forms;

namespace smiletray
{
	public class frmExMsgBox : System.Windows.Forms.Form
	{
		private System.Windows.Forms.Button cmdOK;
		private System.Windows.Forms.TextBox txtMsg;

		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public frmExMsgBox(string title, string message)
		{
			InitializeComponent();

			this.Text = title;
			this.txtMsg.Text = message;
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
			this.txtMsg = new System.Windows.Forms.TextBox();
			this.SuspendLayout();
			// 
			// cmdOK
			// 
			this.cmdOK.Location = new System.Drawing.Point(-8, 328);
			this.cmdOK.Name = "cmdOK";
			this.cmdOK.Size = new System.Drawing.Size(320, 24);
			this.cmdOK.TabIndex = 0;
			this.cmdOK.Text = "OK";
			this.cmdOK.Click += new System.EventHandler(this.cmdOK_Click);
			// 
			// txtMsg
			// 
			this.txtMsg.Location = new System.Drawing.Point(0, 0);
			this.txtMsg.Multiline = true;
			this.txtMsg.Name = "txtMsg";
			this.txtMsg.ReadOnly = true;
			this.txtMsg.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.txtMsg.Size = new System.Drawing.Size(312, 328);
			this.txtMsg.TabIndex = 1;
			this.txtMsg.Text = "textBox1";
			// 
			// frmExMsgBox
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(312, 351);
			this.Controls.Add(this.txtMsg);
			this.Controls.Add(this.cmdOK);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "frmExMsgBox";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "ExMessageBox";
			this.TopMost = true;
			this.ResumeLayout(false);

		}
		#endregion

		private void cmdOK_Click(object sender, System.EventArgs e)
		{
			this.Close();
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

		public static void Msg(string title, string message)
		{
			Form f = new frmExMsgBox(title, message);
			f.ShowDialog();
			f.Dispose();
		}
	}
}
