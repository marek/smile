using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Reflection;

namespace smiletray
{
	public class SingleProgramInstance : IDisposable
	{

		//Win32 API calls necesary to raise an unowned processs main window
		[DllImport("user32.dll")] 
		private static extern bool SetForegroundWindow(IntPtr hWnd);
		[DllImport("user32.dll")] 
		private static extern bool ShowWindowAsync(IntPtr hWnd,int nCmdShow);
		//		[DllImport("user32.dll")] 
		//		private static extern bool ShowWindow(IntPtr hWnd,int nCmdShow);
		[DllImport("user32.dll")] 
		private static extern bool IsIconic(IntPtr hWnd);
		[DllImport("user32.dll")]
		private static extern bool IsWindowVisible(IntPtr hWnd);	

		private const int SW_RESTORE = 9;
		private const int SW_SHOW = 5;
		private const int SW_SHOWNORMAL = 1;

		//private members 
		private Mutex _processSync;
		private bool _owned = false;
		
	
		public SingleProgramInstance()
		{	
			//Initialize a named mutex and attempt to
			// get ownership immediatly 
			_processSync = new Mutex(
				true, // desire intial ownership
				Assembly.GetExecutingAssembly().GetName().Name,
				out _owned);
		}

		public SingleProgramInstance(string identifier)
		{	
			//Initialize a named mutex and attempt to
			// get ownership immediately.
			//Use an addtional identifier to lower
			// our chances of another process creating
			// a mutex with the same name.
			_processSync = new Mutex(
				true, // desire intial ownership
				Assembly.GetExecutingAssembly().GetName().Name + identifier,
				out _owned);
		}

		~SingleProgramInstance()
		{
			//Release mutex (if necessary) 
			//This should have been accomplished using Dispose() 
			Release();
		}

		public bool IsSingleInstance
		{
			//If we don't own the mutex than
			// we are not the first instance.
			get {return	_owned;}
		}
		private  IntPtr GetMainWindowHandle(int processId)
		{
			MainWindowFinder finder = new MainWindowFinder();
			return finder.FindMainWindow(processId);
		}

		public void RaiseOtherProcess()
		{
			Process proc = Process.GetCurrentProcess();
			// Using Process.ProcessName does not function properly when
			// the name exceeds 15 characters. Using the assembly name
			// takes care of this problem and is more accruate than other
			// work arounds.
			string assemblyName = Assembly.GetExecutingAssembly().GetName().Name;
			foreach (Process otherProc in Process.GetProcessesByName(assemblyName))
			{
				//ignore this process
				if (proc.Id != otherProc.Id)
				{
					// Found a "same named process".
					// Assume it is the one we want brought to the foreground.
					// Use the Win32 API to bring it to the foreground.
					
					IntPtr hWnd = GetMainWindowHandle(otherProc.Id);
					
					if (!IsWindowVisible(hWnd)) 
					{
						ShowWindowAsync(hWnd,SW_SHOW);
					}
					ShowWindowAsync(hWnd,SW_SHOWNORMAL);
					//if (IsIconic(hWnd))
					//{
						ShowWindowAsync(hWnd,SW_RESTORE);
					//}
					
					SetForegroundWindow(hWnd);
					return;
				}
			}
		}

		private void Release()
		{
			if (_owned)
			{
				//If we owne the mutex than release it so that
				// other "same" processes can now start.
				_processSync.ReleaseMutex();
				_owned = false;
			}
		}

		#region Implementation of IDisposable
		public void Dispose()
		{
			//release mutex (if necessary) and notify 
			// the garbage collector to ignore the destructor
			Release();
			GC.SuppressFinalize(this);
		}
		#endregion
	}

	class MainWindowFinder
	{	
		// Fields
		private IntPtr bestHandle;
		private int processId;
		
		public MainWindowFinder()
		{
		}

		private bool EnumWindowsCallback(IntPtr handle, IntPtr extraParameter)
		{
			int num1;
			NativeMethods.GetWindowThreadProcessId(new HandleRef(this, handle), out num1);
			if ((num1 == this.processId) && this.IsMainWindow(handle))
			{
				this.bestHandle = handle;
				return false;
			}
			return true;
		}

		public IntPtr FindMainWindow(int processId)
		{
			this.bestHandle = IntPtr.Zero;
			this.processId = processId;
			NativeMethods.EnumThreadWindowsCallback callback1 = new NativeMethods.EnumThreadWindowsCallback(this.EnumWindowsCallback);
			NativeMethods.EnumWindows(callback1, IntPtr.Zero);
			GC.KeepAlive(callback1);
			return this.bestHandle;
		}
		
		private bool IsMainWindow(IntPtr handle)
		{
			if ((NativeMethods.GetWindow(new HandleRef(this, handle), 4) == IntPtr.Zero))
			{
				return true;
			}
			return false;
		}
	}
}
