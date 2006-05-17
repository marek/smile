using System;
using System.Threading;

namespace smiletray
{
	public class SingleProgramInstance : IDisposable
	{
		//private members 
		private Mutex _processSync;
		private bool _owned = false;

		public SingleProgramInstance(string identifier)
		{	
			//Initialize a named mutex and attempt to
			// get ownership immediately.
			//Use an addtional identifier to lower
			// our chances of another process creating
			// a mutex with the same name.
			_processSync = new Mutex(
				true, // desire intial ownership
				identifier,
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


}
