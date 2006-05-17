/////////////////////////////////////////////////////////////////////////////
//
// Smile! -- Screenshot and Statistics Utility
// Copyright (c) 2005 Marek Kudlacz
//
// http://kudlacz.com
//
/////////////////////////////////////////////////////////////////////////////

using System;
using System.ComponentModel;
using System.Collections;
using System.Diagnostics;
using System.Windows.Forms;

namespace smiletray
{
	public class SystemHotkey : System.ComponentModel.Component,IDisposable
	{
		private System.ComponentModel.Container components = null;
		protected NativeMethods.DummyWindowWithEvent m_Window=new NativeMethods.DummyWindowWithEvent();	//window for WM_Hotkey Messages
		protected Shortcut m_HotKey=Shortcut.None;
		protected bool isRegistered=false;
		public event System.EventHandler Pressed;
		public event System.EventHandler Error;

		public SystemHotkey(System.ComponentModel.IContainer container)
		{
			container.Add(this);
			InitializeComponent();
			m_Window.ProcessMessage+=new NativeMethods.MessageEventHandler(MessageEvent);
		}

		public SystemHotkey()
		{
			InitializeComponent();
			if (!DesignMode)
			{
				m_Window.ProcessMessage+=new NativeMethods.MessageEventHandler(MessageEvent);
			}
		}

		public new void Dispose()
		{
			if (isRegistered)
			{
				UnregisterHotkey();
			}
		}
		#region Component Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			components = new System.ComponentModel.Container();
		}
		#endregion

		protected void MessageEvent(object sender,ref Message m,ref bool Handled)
		{	//Handle WM_Hotkey event
			if ((m.Msg==(int)NativeMethods.Msgs.WM_HOTKEY)&&(m.WParam==(IntPtr)this.GetType().GetHashCode()))
			{
				Handled=true;
				if (Pressed!=null) Pressed(this,EventArgs.Empty);
			}
		}
	
		protected bool UnregisterHotkey()
		{	//unregister hotkey
			return NativeMethods.UnregisterHotKey(m_Window.Handle,this.GetType().GetHashCode());
		}

		protected bool RegisterHotkey(Shortcut key)
		{	//register hotkey
			int mod=0;
			Keys k2=Keys.None;
			if (((int)key & (int)Keys.Alt)==(int)Keys.Alt) {mod+=(int)NativeMethods.Modifiers.MOD_ALT;k2=Keys.Alt;}
			if (((int)key & (int)Keys.Shift)==(int)Keys.Shift) {mod+=(int)NativeMethods.Modifiers.MOD_SHIFT;k2=Keys.Shift;}
			if (((int)key & (int)Keys.Control)==(int)Keys.Control) {mod+=(int)NativeMethods.Modifiers.MOD_CONTROL;k2=Keys.Control;}
			

			return NativeMethods.RegisterHotKey(m_Window.Handle,this.GetType().GetHashCode(),(int)mod,((int)key)-((int)k2));
		}

		public bool IsRegistered
		{
			get{return isRegistered;}
		}


		[DefaultValue(Shortcut.None)]
		public Shortcut Shortcut
		{
			get { return m_HotKey; }
			set 
			{ 
				if (DesignMode) {m_HotKey=value;return;}	//Don't register in Designmode
				if ((isRegistered)&&(m_HotKey!=value))	//Unregister previous registered Hotkey
				{
					if (UnregisterHotkey())
					{
						isRegistered=false;
					}
					else 
					{
						if (Error!=null) Error(this,EventArgs.Empty);
					}
				}
				if (value==Shortcut.None) {m_HotKey=value;return;}
				if (RegisterHotkey(value))	//Register new Hotkey
				{
					isRegistered=true;
				}
				else 
				{
					if (Error!=null) Error(this,EventArgs.Empty);
				}
				m_HotKey=value;
			}
		}
	}
}
