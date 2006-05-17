using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace smiletray
{
	/// <summary>
	/// Summary description for CaptureKey.
	/// </summary>
	public class frmCaptureKey : System.Windows.Forms.Form
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public frmCaptureKey()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//
			// TODO: Add any constructor code after InitializeComponent call
			//
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
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
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			// 
			// frmCaptureKey
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(320, 53);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "frmCaptureKey";
			this.Text = "Capture Key";
			this.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.frmCaptureKey_KeyPress);

		}
		#endregion


		private void frmCaptureKey_KeyPress(object sender, System.Windows.Forms.KeyPressEventArgs e)
		{
		
		}
	}
}
