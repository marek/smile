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
		[DllImport("user32.dll")] 
		private static extern bool IsIconic(IntPtr hWnd);
		[DllImport("user32")]
		private static extern bool IsWindowVisible(IntPtr hwnd);

		private const int SW_RESTORE = 9;
		private const int SW_SHOW = 5;

		//private members 
		private Mutex _processSync;
		private bool _owned = false;

		public SingleProgramInstance()
		{   
			//Initialize a named mutex and attempt to
			// get ownership immediately 
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
			get {return _owned;}
		}

		public void RaiseOtherProcess()
		{
			Process proc = Process.GetCurrentProcess();
			// Using Process.ProcessName does not function properly when
			// the actual name exceeds 15 characters. Using the assembly 
			// name takes care of this quirk and is more accruate than 
			// other work arounds.
			string assemblyName = 
				Assembly.GetExecutingAssembly().GetName().Name;
			Process [] moo;
			moo = Process.GetProcessesByName(assemblyName);
			foreach (Process otherProc in 
				Process.GetProcessesByName(assemblyName))
			{
				//ignore "this" process
				if (proc.Id != otherProc.Id)
				{
					// Found a "same named process".
					// Assume it is the one we want brought to the foreground.
					// Use the Win32 API to bring it to the foreground.
					IntPtr hWnd = otherProc.Handle;

					if (!IsWindowVisible(hWnd)) 
					{
						ShowWindowAsync(hWnd,SW_SHOW);
					}

					if (IsIconic(hWnd))
					{
						ShowWindowAsync(hWnd,SW_RESTORE);
					}
					SetForegroundWindow(hWnd);
					break;
				}
			}
			moo = moo;
		}

		private void Release()
		{
			if (_owned)
			{
				//If we own the mutex than release it so that
				// other "same" processes can now start.
				_processSync.ReleaseMutex();
				_owned = false;
			}
		}

		public void Dispose()
		{
			//release mutex (if necessary) and notify 
			// the garbage collector to ignore the destructor
			Release();
			GC.SuppressFinalize(this);
		}
	}
}
