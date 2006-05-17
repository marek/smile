using System;
using System.IO;
using System.Xml;
using System.Text.RegularExpressions;
using Microsoft.Win32;

namespace smiletray
{
	public class comlogParser
	{
		public String alias;
		public String dir;
		public String logfile;
		private StreamReader log;
	
		public comlogParser()
		{
			// Init
			this.alias = GetGameAlias();
			this.dir = GetSteamPath() + @"\SteamApps\" + GetDefaultIdentity() + @"\counter-strike source\cstrike\";
			this.logfile = "console.log";

		}

		public void Open()
		{
			this.log = new StreamReader(new FileStream(this.dir + @"\" + this.logfile, FileMode.Open,  FileAccess.Read, FileShare.ReadWrite));
			this.log.BaseStream.Seek(0,SeekOrigin.End);		// Set to End
		}

		public void Close()
		{
			this.log.Close();
		}

		public bool NewKills()
		{
			if(this.log == null)
				return false;

			Match match;
			String strLine;
			Regex rNick=new Regex(this.alias + " killed ");
			while ((strLine = this.log.ReadLine()) != null)
			{
				match = rNick.Match(strLine);
				if(match.Success)
				{
					return true;
				}
			}
			return false;
		}

		private static String GetSteamPath()
		{
			RegistryKey Key = Registry.CurrentUser;
			Key = Key.OpenSubKey(@"Software\Valve\Steam", false);
			return Path.GetDirectoryName(Key.GetValue("SteamExe").ToString());
		}

		private static String GetDefaultIdentity()
		{
			String strLine;
			Match match;
			String identity = "";
			StreamReader sr = new StreamReader(GetSteamPath()+ @"\config\SteamAppData.vdf");
			//Continues to output one line at a time until end of file(EOF) is reached
			while ((strLine = sr.ReadLine()) != null)
			{
				Regex rNick=new Regex("\"AutoLoginUser\"\\s+\"(\\w+)\"");
				match = rNick.Match(strLine);
				if(match.Success)
				{
					identity = match.Groups[1].Value;
					break;
				}
			}

			//Cleanup
			sr.Close();

			return identity;
		}

		private static String GetGameAlias()
		{
			String strLine;
			Match match;
			String nick = "";
			StreamReader sr = new StreamReader(GetSteamPath() + @"\SteamApps\" + GetDefaultIdentity() + @"\counter-strike source\cstrike\cfg\config.cfg");
			//Continues to output one line at a time until end of file(EOF) is reached
			while ( (strLine = sr.ReadLine()) != null)
			{
				Regex rNick=new Regex("^name\\s+\"(\\w+)\"$");
				match = rNick.Match(strLine);
				if(match.Success)
				{
					nick = match.Groups[1].Value;
					break;
				}
			}

			//Cleanup
			sr.Close();

			return nick;
		}
	}
}
