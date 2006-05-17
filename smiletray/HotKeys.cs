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
using System.Runtime.InteropServices;

namespace smiletray
{

	public class SystemHotkey : System.ComponentModel.Component, IDisposable
	{
		private System.ComponentModel.Container components = null;
		
		public event System.EventHandler Pressed;
		public event System.EventHandler KeyCapture;
		private keycombo hotkey;
		bool keycapture;

		protected static bool isRegistered = false;
		private	static int hook;
		private static keycombo keys = new keycombo();
		private static NativeMethods.LowLevelKeyboardDelegate del;
		public static readonly keycombo EmptyKeyCombo = new keycombo();
		private static ArrayList hotkeys = new ArrayList();
		private static readonly object Lock = new object();

		static public string KeyComboToString(keycombo kc)
		{
			string s = "";
			bool one = false;
			for(int i = 0; i < 256; i++)
			{
				if(kc[i] == 1) 
				{
					if(one)
						s += " + " + Enum.GetName(typeof(Keys), i);
					else
					{
						one = true;
						s = Enum.GetName(typeof(Keys), i);
					}
				}
			}
			return s;
		}

		static public keycombo StringToKeyCombo(string s)
		{
			Array keys = Enum.GetValues(typeof(Keys));
			string [] split = s.Split(new char [] {' ','+'});
			keycombo kb = new keycombo();
			foreach(string key in split)
			{
				
				for(int i = 0; i < keys.Length; i++)
				{
					if(key.Length == 0) { break; }
					if(String.Compare(keys.GetValue(i).ToString(), key, true) == 0) { kb[(int)keys.GetValue(i)] = 1; break; }
					else if(i == keys.Length - 1) { return null; }
				}

			}
			return kb;
		}

		public class keycombo
		{
			private int [] keys = new int [256];	
			public int this[int index]
			{
				get { return keys[index]; }
				set { keys[index] = value; }
			}
			public override string ToString()
			{
				return KeyComboToString(this);
			}
			public static bool operator ==(keycombo x,keycombo y) 
			{
				if((object)x == null && (object)y == null)
					return true;
				else if ((object)x == null || (object)y == null)
					return false;
				for(int i = 0; i < 256; i ++)
					if(x.keys[i] != y.keys[i]) { return false; }
				return true;
			}
			public static bool operator !=(keycombo x,keycombo y) 
			{
				return !(x == y);
			}

			public override bool Equals(object obj)
			{
				return this == (keycombo)obj;
			}

			public override int GetHashCode()
			{
				return base.GetHashCode ();
			}


		}

		public SystemHotkey(System.ComponentModel.IContainer container)
		{
			container.Add(this);
			InitializeComponent();
			lock(Lock)
			{
				hotkeys.Add(this);
			}
		}
		public bool EnableKeyCapture
		{
			set { keycapture = value; }
			get { return keycapture; }
		}

		public SystemHotkey()
		{
			InitializeComponent();
			lock(Lock)
			{
				hotkeys.Add(this);
			}
		}

		public keycombo GetKeys()
		{
			return keys;
		}

		public new void Dispose()
		{
			lock(Lock)
			{
				del = null;
				hotkeys.Remove(this);
				if(isRegistered && hotkeys.Count == 0)
				{
					UnregisterHook();
				}
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
		

	
		static private Int32 LowLevelKeyboardHandler(Int32 nCode, Int32 wParam, ref NativeMethods.KBDLLHOOKSTRUCT lParam) 
		{ 
	
			if (nCode == NativeMethods.HC_ACTION) 
			{
				if (wParam == (int)NativeMethods.Msgs.WM_KEYDOWN)
				{
					keys[(int)lParam.vkCode] = 1;
					for(int i = 0; i < hotkeys.Count; i++)
					{
						SystemHotkey hk = (SystemHotkey)hotkeys[i];
						if(hk.keycapture && hk.KeyCapture != null)
							hk.KeyCapture(hk, EventArgs.Empty);
					}
				}
				else if (wParam == (int)NativeMethods.Msgs.WM_KEYUP)
					keys[(int)lParam.vkCode] = 0;

				for(int i = 0; i < hotkeys.Count; i++)
				{
					SystemHotkey hk = (SystemHotkey)hotkeys[i];
					if(keys == hk.hotkey && hk.Pressed != null)
						hk.Pressed(hk, EventArgs.Empty);
				}
			}
			return NativeMethods.CallNextHookEx(hook, nCode, wParam, lParam); 
			
		} 

	
		public static bool UnregisterHook()
		{	//unregister hotkey
			lock(Lock)
			{
				if(hotkeys.Count == 0)
				{
					return isRegistered = (NativeMethods.UnhookWindowsHookEx(hook) != 0);
				}
			}
			return false;
		}

		public static bool RegisterHook()
		{	
			lock(Lock)
			{
				//register hotkey
				if(isRegistered)
					return true;
				del = new NativeMethods.LowLevelKeyboardDelegate(LowLevelKeyboardHandler);
				hook = NativeMethods.SetWindowsHookEx((int)NativeMethods.HookType.WH_KEYBOARD_LL, del, Marshal.GetHINSTANCE(System.Reflection.Assembly.GetExecutingAssembly().GetModules()[0]).ToInt32(),0); 
			
				if(hook != 0)
					return isRegistered = true;
				else
				{
					del = null;
					return  false;
				}
			}
		}

		public static bool IsRegistered
		{
			get{return isRegistered;}
		}


		public keycombo Shortcut
		{
			get { return hotkey; }
			set 
			{ 
				hotkey = value;
			}
		}
	}
}
