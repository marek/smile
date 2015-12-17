/////////////////////////////////////////////////////////////////////////////
//
// HotKeys.cs --  Global HotKey Hook with C# and .NET
// v1.2
// Copyright (c) 2005-2015 Marek Kudlacz
//
// http://kudlacz.com
//
/////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Diagnostics;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace Kudlacz.Hooks
{
	public class GlobalHotKey : System.ComponentModel.Component, IDisposable
	{
		private System.ComponentModel.Container components = null;
		private keycombo hotkey;
		
		public event System.EventHandler Pressed;
		public event System.EventHandler KeyCapture;
		public bool capture;

        protected static Int64 hook;
		protected static keycombo keys = new keycombo();
		protected static LowLevelKeyboardDelegate del;
		protected static ArrayList hotkeys = new ArrayList();
		protected static readonly object Lock = new object();
		protected static bool registered;
	
		public static readonly keycombo EmptyKeyCombo = new keycombo();
		
		#region Dll Imports
		[DllImport("user32")]
        private static extern IntPtr SetWindowsHookEx(Int32 idHook, LowLevelKeyboardDelegate lpfn, IntPtr hmod, Int32 dwThreadId); 
         
		[DllImport("user32")]
        private static extern IntPtr CallNextHookEx(IntPtr hHook, Int32 nCode, IntPtr wParam, ref KBDLLHOOKSTRUCT lParam); 

		[DllImport("user32")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hHook); 
		
		#endregion
		
		// ---------------- Type definitions   ------------------
        protected delegate IntPtr LowLevelKeyboardDelegate(Int32 nCode, IntPtr wParam, ref KBDLLHOOKSTRUCT lParam); 
		private const Int32 HC_ACTION = 0; 
		private const Int32 WM_KEYDOWN = 0x0100;
		private const Int32 WM_KEYUP = 0x0101;
		private const Int32 WH_KEYBOARD_LL = 13;
         
		[StructLayout(LayoutKind.Sequential)]
		public struct KBDLLHOOKSTRUCT 
		{ 
			public int vkCode; 
			public int scanCode; 
			public int flags; 
			public int time;
            public IntPtr dwExtraInfo; 
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
		
		// --------------- Helper functions --------------- //
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
		
		
		// --------------- Constructors / Destructors --------------- //
		public GlobalHotKey(System.ComponentModel.IContainer container)
		{
            if (container == null)
                return;
			container.Add(this);
			InitializeComponent();
			lock(Lock)
			{
				hotkeys.Add(this);
			}
		}
		
		public GlobalHotKey()
		{
			InitializeComponent();
			lock(Lock)
			{
				hotkeys.Add(this);
			}
		}
		
		~GlobalHotKey()
		{
			Dispose();
		}

		public new void Dispose()
		{
			lock(Lock)
			{
				del = null;
				hotkeys.Remove(this);
				if(registered && hotkeys.Count == 0)
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
		

		// Main function proccess
        static private IntPtr LowLevelKeyboardHandler(Int32 nCode, IntPtr wParam, ref KBDLLHOOKSTRUCT lParam) 
		{ 
	
			if (nCode == HC_ACTION) 
			{
				if (wParam.ToInt64() == WM_KEYDOWN)
				{
					keys[(int)lParam.vkCode] = 1;
					for(int i = 0; i < hotkeys.Count; i++)
					{
						GlobalHotKey hk = (GlobalHotKey)hotkeys[i];
						if(hk.capture && hk.KeyCapture != null)
							hk.KeyCapture(hk, EventArgs.Empty);
					}

					for(int i = 0; i < hotkeys.Count; i++)
					{
						GlobalHotKey hk = (GlobalHotKey)hotkeys[i];
						if(keys == hk.hotkey && hk.Pressed != null)
							hk.Pressed(hk, EventArgs.Empty);
					}
				}
				else if (wParam.ToInt64() == WM_KEYUP)
					keys[(int)lParam.vkCode] = 0;
			}
			return CallNextHookEx((IntPtr)hook, nCode, wParam, ref lParam); 
		} 
	
		
		// Functions & getters and setters
		public bool EnableKeyCapture
		{
			set { capture = value; }
			get { return capture; }
		}


		public keycombo GetKeys()
		{
			return keys;
		}
		
				
		public static bool IsRegistered
		{
			get{return registered;}
		}
		
		public static bool RegisterHook()
		{	
			lock(Lock)
			{
				// register hotkey
				if(registered)
					return true;
				del = new LowLevelKeyboardDelegate(LowLevelKeyboardHandler);
				hook = (Int64)SetWindowsHookEx(WH_KEYBOARD_LL, del, Marshal.GetHINSTANCE(System.Reflection.Assembly.GetExecutingAssembly().GetModules()[0]),0); 
			
				if(hook != 0)
					return registered = true;
				else
				{
					del = null;
					return  false;
				}
			}
		}

		public static bool UnregisterHook()
		{	// unregister hotkey
			lock(Lock)
			{
				if(hotkeys.Count == 0)
				{
					return registered = UnhookWindowsHookEx((IntPtr)hook);
				}
			}
			return false;
		}

		public keycombo HotKey
		{
			get { return hotkey; }
			set 
			{ 
				hotkey = value;
			}
		}
	}
}
