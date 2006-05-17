using System;
using System.IO;
using System.Text;

namespace smiletray
{
    public class LogFile
    {
        private readonly object MsgLock = new object();
        private TSArrayList MsgQueue;
        private StreamWriter log;

        public LogFile(string path)
        {
            // Init log file
            log = new StreamWriter(path, true);
            log.WriteLine("\r\n\r\n-----Log Session Started: " + DateTime.Now.ToLongDateString() + "-----\r\n\r\n");
            log.Flush();

            // Init MsgQueue
            MsgQueue = new TSArrayList(10);
        }
        ~LogFile()
        {
            Close();
        }
        public void Close()
        {
            if (log != null)
            {
                log.WriteLine("\r\n\r\n-----Log Session Ended: " + DateTime.Now.ToLongDateString() + "-----\r\n\r\n");
                log.Flush();
                log.Close();
            }
        }

		public void Add(String msg)
		{
			lock(MsgLock)
			{
				string s = String.Format("[{0:D2}:{1:D2}:{2:D2}] ", DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second) + msg;
				MsgQueue.Add(s);
				if(log != null)
				{
					log.WriteLine(s);
					log.Flush();
				}
			}
		}

        public bool HasMessage()
        {
            return MsgQueue.Count() > 0;
        }

        public string Pop()
        {
            lock (MsgLock)
            {
                String msg = (String)MsgQueue.ObjectAt(0);
                MsgQueue.RemoveAt(0);
                return msg;
            }
        }

        public string Peek()
        {
            lock (MsgLock)
            {
                String msg = (String)MsgQueue.ObjectAt(0);
                return msg;
            }
        }
    }
}
