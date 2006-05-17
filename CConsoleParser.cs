using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections;
using Microsoft.Win32;

namespace smiletray
{
	//
	// Class: Console log Parser:
	// Responsible for parsing the console.log
	//
	public class CConsoleParser
	{
		public String alias;
		public String user;
		public String dir;
		public String logfile;
		private StreamReader log;
		private bool newkills;
		private stats_t stats;
	
		// Default contructor
		public CConsoleParser()
		{
			// Init
			this.alias = GetGameAlias();
			this.user = GetDefaultIdentity();
			this.dir = GetSteamPath() + @"\SteamApps\" + GetDefaultIdentity() + @"\counter-strike source\cstrike\";
			this.logfile = "console.log";
			this.newkills = false;

			this.stats = new stats_t();
			this.stats.gun = new Hashtable();
									
		}

		// Open log file and shift to the end
		public void Open()
		{
			this.log = new StreamReader(new FileStream(this.dir + @"\" + this.logfile, FileMode.Open,  FileAccess.Read, FileShare.ReadWrite));
			this.log.BaseStream.Seek(0,SeekOrigin.End);		// Set to End
		}

		// Close Log file
		public void Close()
		{
			this.log.Close();
		}

		public bool NewKills()
		{
			return this.newkills;
		}
		// Check for New Kills
		public void Parse()
		{
			if(this.log == null)
				return;

			Match match;
			String strLine;
			Regex rKill = new Regex("^" + this.alias + " killed .* with (\\w+).$");
			Regex rKilled = new Regex("^.* killed " + this.alias + " with (\\w+).$");
			Regex rNickChange = new Regex("^" + this.alias + " is now known as (\\w+)$");
			Regex rDamage = new Regex("^Damage (\\w+ \\w+) \".*\" - (\\d+) in (\\d+) hits?$");
			Regex rTeamDamage = new Regex("^" + this.alias + " Attacked A Teammate$"); 
			Regex rSuicide = new Regex("^" + this.alias + " suicided.$");
			Regex rDeath =  new Regex("^" + this.alias + " died.$");

			while ((strLine = this.log.ReadLine()) != null)
			{
				// Match kills given
				match = rKill.Match(strLine);
				if(match.Success) 
				{
					String strGun = match.Groups[1].Value;
					gun_t gun;
					if(this.stats.gun.Contains(strGun))
						gun = (gun_t)this.stats.gun[strGun];
					else 
						gun = new gun_t();
					gun.kills += 1;
					this.stats.gun.Add(strGun, gun);

					newkills = true;
					continue;
				}

				// Match times self has been killed
				match = rKilled.Match(strLine);
				if(match.Success)
				{
					String strGun = match.Groups[1].Value;
					gun_t gun;
					if(this.stats.gun.Contains(strGun))
						gun = (gun_t)this.stats.gun[strGun];
					else 
						gun = new gun_t();
					gun.killed += 1;
					this.stats.gun.Add(strGun, gun);
					continue;
				}

				// Match nickchange of self
				match = rNickChange.Match(strLine);
				if(match.Success)
				{
					this.alias = match.Groups[1].Value;
				}

				// Match Damage given/received
				match = rDamage.Match(strLine);
				if(match.Success)
				{
					if(match.Groups[1].Value == "Given to")
					{
						this.stats.damage_given += Convert.ToUInt32(match.Groups[2].Value);
						this.stats.hits_given += Convert.ToUInt32(match.Groups[3].Value);
						this.stats.given_counts++;
					}
					else
					{
						this.stats.damage_received += Convert.ToUInt32(match.Groups[2].Value);
						this.stats.hits_received += Convert.ToUInt32(match.Groups[3].Value);
						this.stats.received_counts++;
					}
				}

				// Match Team Damage given
				match = rTeamDamage.Match(strLine);
				if(match.Success)
				{
					this.stats.hits_team++;
				}

				// Match Suicide
				match = rSuicide.Match(strLine);
				if(match.Success)
				{
					this.stats.suicides++;
				}

				// Match Misc Death
				match = rSuicide.Match(strLine);
				if(match.Success)
				{
					this.stats.deaths++;
				}
			}
		}

		// Get the path of the steam install dir
		private static String GetSteamPath()
		{
			RegistryKey Key = Registry.CurrentUser;
			Key = Key.OpenSubKey(@"Software\Valve\Steam", false);
			return Path.GetDirectoryName(Key.GetValue("SteamExe").ToString());
		}

		// Get the default login user
		private static String GetDefaultIdentity()
		{
			String strLine;
			Match match;
			String identity = "";
			StreamReader sr = new StreamReader(GetSteamPath()+ @"\config\SteamAppData.vdf");
			//Continues to output one line at a time until end of file(EOF) is reached
			while ((strLine = sr.ReadLine()) != null)
			{
				Regex rNick = new Regex("\"AutoLoginUser\"\\s+\"(\\w+)\"");
				match = rNick.Match(strLine);
				if(match.Success)
				{
					identity = match.Groups[1].Value;
					break;
				}
			}

			// Cleanup
			sr.Close();

			return identity;
		}

		// Get the game alias for said default user
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

			// Cleanup
			sr.Close();

			return nick;
		}
	}
}
