/////////////////////////////////////////////////////////////////////////////
//
// Smile! -- CounterStrike: Source Screenshot and Statistics Utility
// v1.2
// Written by Marek Kudlacz
// Copyright (c)2005
//
/////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections;
using System.Xml.Serialization;
using System.Windows.Forms;
using System.Drawing;
using System.Management;
using Microsoft.Win32;

namespace smiletray
{
	public class CProfile_XMLGun
	{
		public String name;
		public CProfile_Gun stats;
		
		public CProfile_XMLGun()
		{
			stats = new CProfile_Gun();
		}
	}
	public class CProfile_Gun
	{
		public UInt32 kills;
		public UInt32 killed;

		public CProfile_Gun()
		{
			this.kills = new UInt32();
			this.killed = new UInt32();		
		}
		public CProfile_Gun(UInt32 kills, UInt32 killed)
		{
			this.kills = kills;
			this.killed = killed;
		}
	}
	public class CGameStatSettings
	{
		public Boolean Enabled;
		public Boolean UseGlobal;

		public CGameStatSettings()
		{
			Enabled = new Boolean();
			UseGlobal = new Boolean();
		}
	}

	public class CGameSnapSettings
	{
		public Boolean Enabled;
		public Boolean SingleDisplay;
		public Boolean SaveAnimation;
		public Int32 Delay;
		public Int32 SaveDelay;
		public Int32 NextSnapDelay;
		public Int32 MultiSnapDelay;
		public Int32 SnapCount;

		public Boolean UseGlobal;
		public String SnapDir;

		public CGameSnapSettings()
		{
			Enabled = new Boolean();
			SingleDisplay = new Boolean();
			UseGlobal = new Boolean();
			SaveAnimation = new Boolean();
			Delay = new Int32();
			SaveDelay = new Int32();
			NextSnapDelay = new Int32();
			MultiSnapDelay = new Int32();
			SnapCount = new Int32();
		}
	}
	// Simplest way since we DO have access to the src:
	[XmlInclude(typeof(CProfileDayofDefeatSource))]
	[XmlInclude(typeof(CProfileCounterStrikeSource))]
	[XmlInclude(typeof(CProfileHalfLife2Deathmatch))]
	[XmlInclude(typeof(CProfileCounterStrike))]
	[XmlInclude(typeof(CProfileDayofDefeat))]
	[XmlInclude(typeof(CProfileQuakeIIIArena))]
	[XmlInclude(typeof(CProfileQuakeIIITeamArena))]
	[XmlInclude(typeof(CProfileQ3OSP))]
	[XmlInclude(typeof(CProfileEnemyTerritory))]
	public abstract class CProfile
	{
		// Public XML attributes
		public String path;
		public CGameSnapSettings SnapSettings;
		public CGameStatSettings StatsSettings;

		// Hidden XML Attributes
		[XmlIgnoreAttribute] public String ProfileName;
		[XmlIgnoreAttribute] public String SnapName;
		[XmlIgnoreAttribute] public bool NewSnaps;
		[XmlIgnoreAttribute] public bool NewStats;
		[XmlIgnoreAttribute] public bool EnableSnaps;
		[XmlIgnoreAttribute] public bool EnableStats;
		public enum SaveTypes
		{
			HTML,
			RTF,
			TXT
		};
		protected StreamReader log;
		public abstract void Parse();
		public abstract bool Open();
		public virtual bool IsOpen()
		{
			return this.log != null;
		}
		public virtual void Close()
		{
			if(this.log != null) this.log.Close();
		}
		public abstract bool CheckActive();
		public abstract String GetDefaultPath();
		public abstract void toXMLOperations();
		public abstract void fromXMLOperations();
		public abstract void ResetStats();
		public abstract string GetStatsReport(String font, CProfile.SaveTypes format);

		public CProfile()
		{
			SnapSettings = new CGameSnapSettings();
			StatsSettings = new CGameStatSettings();

		}
	}

	//////////////////////////////////////////////////////////////////////////////////////////////
	/// Counter Strike: Source
	//////////////////////////////////////////////////////////////////////////////////////////////
	public class CProfileCounterStrikeSource_Stats
	{
		public UInt32 damage_given;
		public UInt32 damage_received;
		public UInt32 hits_received;
		public UInt32 hits_given;
		public UInt32 received_counts;
		public UInt32 given_counts;
		public UInt32 hits_team;
		public UInt32 deaths;		// misc deaths, does not include being killed by weapon
		public UInt32 suicides;
		[XmlIgnoreAttribute] public Hashtable gun;
		[XmlElement(ElementName = "gun")]public CProfile_XMLGun [] xmlgun;

		public CProfileCounterStrikeSource_Stats()
		{
			damage_given = new UInt32();
			damage_received = new UInt32();
			hits_received = new UInt32();
			hits_given = new UInt32();
			received_counts = new UInt32();
			given_counts = new UInt32();
			hits_team = new UInt32();
			deaths = new UInt32();
			suicides = new UInt32();
			gun = new Hashtable();
		}
	}

	public class CProfileCounterStrikeSource : CProfile
	{
		public CProfileCounterStrikeSource_Stats stats;

		private String alias;
		private Regex rKill;
		private Regex rKilled;
		private Regex rNickChange;
		private Regex rDamage;
		private Regex rTeamDamage; 
		private Regex rSuicide;
		private Regex rDeath;

		public CProfileCounterStrikeSource()
		{
			this.ProfileName = "Counter-Strike: Source";
			this.SnapName = "Counter-Strike Source";
			this.stats = new CProfileCounterStrikeSource_Stats();
		}

		private void PopulateRegEx()
		{
			this.rKill = new Regex("^" + Regex.Escape(this.alias) + " killed .+ with (\\w+).$");
			this.rKilled = new Regex("^.+ killed " + Regex.Escape(this.alias) + " with (\\w+).$");
			this.rNickChange = new Regex("^" + Regex.Escape(this.alias) + " is now known as (.+)$");
			this.rDamage = new Regex("^Damage (\\w+ \\w+) \".*\" - (\\d+) in (\\d+) hits?$");
			this.rTeamDamage = new Regex("^" + Regex.Escape(this.alias) + " attacked a teammate$"); 
			this.rSuicide = new Regex("^" + Regex.Escape(this.alias) + " suicided.$");
			this.rDeath =  new Regex("^" + Regex.Escape(this.alias) + " died.$");
		}

		public override void Parse()
		{
			if(this.log == null)
				return;

			Match match;
			String strLine;

			while ((strLine = this.log.ReadLine()) != null)
			{
				// Match kills given
				if(EnableSnaps || EnableStats)
				{
					match = rKill.Match(strLine);
					if(match.Success) 
					{
						if(EnableStats)
						{
							String strGun = match.Groups[1].Value;
							CProfile_Gun gun;
							if(stats.gun.Contains(strGun))
							{
								gun = (CProfile_Gun)stats.gun[strGun];
								gun.kills++;
								stats.gun[strGun] = gun;
							}
							else 
							{
								gun = new CProfile_Gun();
								gun.kills = 1;
								stats.gun.Add(strGun, gun);
							}
						}
						NewSnaps = true;
						NewStats = true;
						continue;
					}
				}

				if(EnableStats)
				{
					// Match times self has been killed
					match = rKilled.Match(strLine);
					if(match.Success)
					{
						String strGun = match.Groups[1].Value;
						CProfile_Gun gun;
						if(stats.gun.Contains(strGun)) 
						{
							gun = (CProfile_Gun)stats.gun[strGun];
							gun.killed++;
							stats.gun[strGun] = gun;
						}
						else  
						{
							gun = new CProfile_Gun();
							gun.killed = 1;
							stats.gun.Add(strGun, gun);
						}
						NewStats = true;
						continue;
					}

					// Match nickchange of self
					match = rNickChange.Match(strLine);
					if(match.Success)
					{
						this.alias = match.Groups[1].Value;
						PopulateRegEx();
						NewStats = true;
						continue;
					}

					// Match Damage given/received
					match = rDamage.Match(strLine);
					if(match.Success)
					{
						if(match.Groups[1].Value == "Given to")
						{
							stats.damage_given += Convert.ToUInt32(match.Groups[2].Value);
							stats.hits_given += Convert.ToUInt32(match.Groups[3].Value);
							stats.given_counts++;
						}
						else
						{
							stats.damage_received += Convert.ToUInt32(match.Groups[2].Value);
							stats.hits_received += Convert.ToUInt32(match.Groups[3].Value);
							stats.received_counts++;
						}
						NewStats = true;
						continue;
					}

					// Match Team Damage given
					match = rTeamDamage.Match(strLine);
					if(match.Success)
					{
						stats.hits_team++;
						NewStats = true;
						continue;
					}

					// Match Suicide
					match = rSuicide.Match(strLine);
					if(match.Success)
					{
						stats.suicides++;
						NewStats = true;
						continue;
					}

					// Match Misc Death
					match = rDeath.Match(strLine);
					if(match.Success)
					{
						stats.deaths++;
						NewStats = true;
						continue;
					}
				}
			}
		}
		public override bool Open()
		{
			try 
			{
				this.alias = GetGameAlias();
				this.log = new StreamReader(new FileStream(this.path + @"\console.log", FileMode.Open,  FileAccess.Read, FileShare.ReadWrite));
				this.log.BaseStream.Seek(0,SeekOrigin.End);		// Set to End
				PopulateRegEx();								// Create our Regex objects
				return true;
			}
			catch (Exception e)
			{
				frmMain.error += "|||" + e.Message;
				return false;
			}
		}
		public override bool CheckActive()
		{
			return NativeMethods.FindWindow("Valve001","Counter-Strike Source") != 0;
		}
		public override String GetDefaultPath()
		{
			try
			{
				RegistryKey Key = Registry.CurrentUser;
				Key = Key.OpenSubKey(@"Software\Valve\Steam", false);
				frmSearch search = new frmSearch();
				search.Show();
				string result = search.Search(ProfileName, Path.GetDirectoryName(Key.GetValue("SteamExe").ToString()) + @"\SteamApps", 
					new Regex(@"counter\-strike source\\cstrike$", RegexOptions.IgnoreCase));
				search.Close();
				if(result != null)
					return result;
				
			}
			catch(Exception e)
			{
				Ex.DumpException(e);
				return null;
			}
			return null;
		}
		// Get the game alias for said default user
		public String GetGameAlias()
		{
			String strLine;
			Match match;
			String nick = null;
			StreamReader sr = null;
			
			try
			{
				sr = new StreamReader(this.path + @"\cfg\config.cfg");
				//Continues to output one line at a time until end of file(EOF) is reached
				while ( (strLine = sr.ReadLine()) != null)
				{
					Regex rNick=new Regex("^name\\s+\"(.+)\"$");
					match = rNick.Match(strLine);
					if(match.Success)
					{
						nick = match.Groups[1].Value;
						break;
					}
				}
			}
			catch
			{
				nick = null;
			}
			finally 
			{
				// Cleanup
				if(sr != null) sr.Close();	
			}
			return nick;
		}
		public override void toXMLOperations()
		{
			// Turn gun hastable into something more useable
			if(stats.gun == null)
				return;
			stats.xmlgun = new CProfile_XMLGun [ stats.gun.Count ];
			int i = 0;
			foreach(String key in stats.gun.Keys)
			{
				stats.xmlgun[i] = new CProfile_XMLGun();
				stats.xmlgun[i].name = key;
				stats.xmlgun[i].stats = (CProfile_Gun)stats.gun[key];
				i++;
			}
		}
		public override void fromXMLOperations()
		{
			if(stats.xmlgun == null)
				return;

			if(stats.gun == null)
				stats.gun = new Hashtable();

			for(int i = 0; i < stats.xmlgun.Length; i++)
			{
				stats.gun.Add(stats.xmlgun[i].name, stats.xmlgun[i].stats);
			}
		}
		public override void ResetStats()
		{
			stats = new CProfileCounterStrikeSource_Stats();
		}
		public override string GetStatsReport(String font, CProfile.SaveTypes format)
		{
			string strStats = null;
			CProfileCounterStrikeSource_Stats stats = (CProfileCounterStrikeSource_Stats)this.stats;
			switch(format)
			{
				case CProfile.SaveTypes.HTML:
				{
					strStats = "<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.1//EN\" \"http://www.w3.org/TR/xhtml11/DTD/xhtml11.dtd\">\r\n";
					strStats += "<html xmlns=\"http://www.w3.org/1999/xhtml\" xml:lang=\"en\">\r\n";
					strStats += "\t<head>\r\n";
					strStats += "\t\t<meta http-equiv=\"Content-Type\" content=\"application/xhtml+xml; charset=iso-8859-1\" />\r\n";
					strStats += "\t\t<meta http-equiv=\"Content-Style-Type\" content=\"text/css\" />\r\n";
					strStats += "\t\t<meta http-equiv=\"Content-Language\" content=\"EN\" />\r\n";
					strStats += "\t\t<meta name=\"description\" content=\"Smile! v" + Info.version + " -- " + ProfileName + " Generator.\" />\r\n";
					strStats += "\t\t<meta name=\"copyright\" content=\"©2005 Marek Kudlacz -- Kudlacz.com\" />\r\n";
					strStats += "\t\t<title>Smile! v" + Info.version + " - " + ProfileName + " Statistics</title>\r\n";
					strStats += "\t</head>\r\n";
					strStats += "\t<body>\r\n";
					strStats += "\t\t<h2>" + ProfileName + " Statistics:</h2>\r\n";
					strStats += "\t\t<hr />\r\n";
					strStats += "\t\tCreated with Smile! v" + Info.version + "<br />\r\n";
					strStats += "\t\t©2005 Marek Kudlacz -- <a href=\"http://www.kudlacz.com\">http://www.kudlacz.com</a><br />\r\n";
					strStats += "\t\t<hr />\r\n";

					strStats += "\t\t<h3>Enagement Information</h3>\r\n";
					strStats += "\t\tYou've engaged others " + stats.given_counts + " times.<br />\r\n";
					strStats += "\t\tYou've been engaged " + stats.received_counts + " times.<br /><br />\r\n";

					strStats += "\t\t<h3>Hit Information:</h3>\r\n";
					strStats += "\t\tHits given: " + stats.hits_given + "<br />\r\n";
					strStats += "\t\tAverage hits given per engagement: " + (float)stats.hits_given/stats.given_counts + "<br />\r\n";
					strStats += "\t\tHits received: " + stats.hits_received + "<br />\r\n";
					strStats += "\t\tAverage hits received per engagement: " + (float)stats.hits_received/stats.received_counts + "<br /><br />\r\n";

					strStats += "\t\t<h3>Damage Information:</h3>\r\n";
					strStats += "\t\tDamage given: " + stats.damage_given + "<br />\r\n";
					strStats += "\t\tAverage damage given per engagement: " + (float)stats.damage_given/stats.given_counts + "<br />\r\n";
					strStats += "\t\tDamage received: " + stats.damage_received + "<br />\r\n";
					strStats += "\t\tAverage damage received per engagement: " + (float)stats.damage_received/stats.received_counts + "<br /><br />\r\n";

					strStats += "\t\t<h3>Misc Statistics:</h3>\r\n";
					strStats += "\t\tFriendly-fire Attacks: " + stats.hits_team + "<br />\r\n";
					strStats += "\t\tMiscellaneous Deaths: " + stats.deaths + "<br />\r\n";
					strStats += "\t\tSuicides: " + stats.suicides + "<br /><br />\r\n";

					strStats += "\t\t<h3>Weapon Statistics:</h3>\r\n";
					uint TotalKills = 0;
					uint TotalKilled = 0;
					foreach(String Key in stats.gun.Keys)
					{
						TotalKills += ((CProfile_Gun)stats.gun[Key]).kills;
						TotalKilled += ((CProfile_Gun)stats.gun[Key]).killed;
						strStats += "\t\t<b>"+ Key + ":</b> kills: " + ((CProfile_Gun)stats.gun[Key]).kills + " deaths: " + ((CProfile_Gun)stats.gun[Key]).killed + "<br />\r\n";
					}
					strStats += "\t\tTotal Kills: " + TotalKills + " Total Deaths: " + TotalKilled + "<br />\r\n";
					strStats += "\t\t<br /><br />\r\n";
					strStats += "\t</body>\r\n";
					strStats += "</html>";
					break;
				}
				case CProfile.SaveTypes.RTF:
				case CProfile.SaveTypes.TXT:
				{
					RichTextBox c = new RichTextBox();

					try
					{
						// Populate the rich text box
						c.SelectionStart = 0 ;
						c.SelectionFont = new Font(font, 12, FontStyle.Bold);
						c.SelectedText = ProfileName + " Statistics:\n" ;
						c.SelectionFont = new Font(font, 12, FontStyle.Bold|FontStyle.Underline);
						c.SelectedText = "                                                                \n\n";
						c.SelectedText = "Created with Smile! v" + Info.version + "\n";
						c.SelectedText = "©2005 Marek Kudlacz -- http://www.kudlacz.com\n";
						c.SelectionFont = new Font(font, 12, FontStyle.Bold|FontStyle.Underline);
						c.SelectedText = "                                                                \n\n";

						c.SelectionFont = new Font(font, 10, FontStyle.Bold);
						c.SelectedText = "Engagement Information:\n";
						c.SelectedText = "You've engaged others " + stats.given_counts + " times.\n";
						c.SelectedText = "You've been engaged " + stats.received_counts + " times.\n";
						c.SelectedText = "\n";

						c.SelectionFont = new Font(font, 10, FontStyle.Bold);
						c.SelectedText = "Hit Information:\n" ;
						c.SelectedText = "Hits given: " + stats.hits_given + "\n";
						c.SelectedText = "Average hits given per engagement: " + (float)stats.hits_given/stats.given_counts + "\n";
						c.SelectedText = "Hits received: " + stats.hits_received + "\n";
						c.SelectedText = "Average hits received per engagement: " + (float)stats.hits_received/stats.received_counts + "\n";
						c.SelectedText = "\n";

						c.SelectionFont = new Font(font, 10, FontStyle.Bold);
						c.SelectedText = "Damage Information:\n" ;
						c.SelectedText = "Damage given: " + stats.damage_given + "\n";
						c.SelectedText = "Average damage given per engagement: " + (float)stats.damage_given/stats.given_counts + "\n";
						c.SelectedText = "Damage received: " + stats.damage_received + "\n";
						c.SelectedText = "Average damage received per engagement: " + (float)stats.damage_received/stats.received_counts + "\n";
						c.SelectedText = "\n";

						c.SelectionFont = new Font(font, 10, FontStyle.Bold);
						c.SelectedText = "Misc Statistics:\n" ;
						c.SelectedText = "Friendly-fire Attacks: " + stats.hits_team + "\n";
						c.SelectedText = "Miscellaneous Deaths: " + stats.deaths + "\n";
						c.SelectedText = "Suicides: " + stats.suicides + "\n";
						c.SelectedText = "\n";

						c.SelectionFont = new Font(font, 10, FontStyle.Bold);
						c.SelectedText = "Weapon Statistics:\n" ;
						uint TotalKills = 0;
						uint TotalKilled = 0;
						foreach(String Key in stats.gun.Keys)
						{
							TotalKills += ((CProfile_Gun)stats.gun[Key]).kills;
							TotalKilled += ((CProfile_Gun)stats.gun[Key]).killed;
							c.SelectionFont = new Font(font, 9, FontStyle.Bold|FontStyle.Italic);
							c.SelectedText = Key + ": ";
							c.SelectionFont = new Font(font, 9, FontStyle.Regular);
							c.SelectedText = " kills: " + ((CProfile_Gun)stats.gun[Key]).kills;
							c.SelectedText = " deaths: " + ((CProfile_Gun)stats.gun[Key]).killed + "\n";
						}
						c.SelectedText = "Total Kills: " + TotalKills + " Total Deaths: " + TotalKilled + "\n";
						c.SelectedText = "\n\n";

						c.SelectionStart = 0 ;
					}
					catch
					{
						c.Clear();
						c.SelectionStart = 0 ;
						c.SelectedText = "There was an error writing the stats, (missing font?) Try saving it to html instead using the edit menu.\n\n";
						c.SelectionStart = 0 ;
					}

					switch(format)
					{
						case CProfile.SaveTypes.RTF:
							strStats = c.Rtf;
							break;
						case CProfile.SaveTypes.TXT:
							strStats = c.Text;
							break;
					}
					c.Dispose();
					break;
				}
			}
			return strStats;		
		}
	}

	//////////////////////////////////////////////////////////////////////////////////////////////
	/// HalfLife2: Deathmatch
	//////////////////////////////////////////////////////////////////////////////////////////////
	public class CProfileHalfLife2Deathmatch_Stats
	{
		public UInt32 deaths;		// misc deaths, does not include being killed by weapon
		public UInt32 suicides;
		[XmlIgnoreAttribute] public Hashtable gun;
		[XmlElement(ElementName = "gun")]public CProfile_XMLGun [] xmlgun;

		public CProfileHalfLife2Deathmatch_Stats()
		{
			deaths = new UInt32();
			suicides = new UInt32();
			gun = new Hashtable();
		}
	}

	public class CProfileHalfLife2Deathmatch : CProfile
	{
		public CProfileHalfLife2Deathmatch_Stats stats;

		private String alias;
		private Regex rKill;
		private Regex rKilled;
		private Regex rNickChange;
		private Regex rSuicide;
		private Regex rDeath;

		public CProfileHalfLife2Deathmatch()
		{
			this.ProfileName = "Half-Life 2: Deathmatch";
			this.SnapName = "Half-Life 2 Deathmatch";
			this.stats = new CProfileHalfLife2Deathmatch_Stats();
		}

		private void PopulateRegEx()
		{
			this.rKill = new Regex("^" + Regex.Escape(this.alias) + " killed .+ with (\\w+).$");
			this.rKilled = new Regex("^.+ killed " + Regex.Escape(this.alias) + " with (\\w+).$");
			this.rNickChange = new Regex("^" + Regex.Escape(this.alias) + " is now known as (.+)$");
			this.rSuicide = new Regex("^" + Regex.Escape(this.alias) + " suicided.$");
			this.rDeath =  new Regex("^" + Regex.Escape(this.alias) + " died.$");
		}

		public override void Parse()
		{
			if(this.log == null)
				return;

			Match match;
			String strLine;

			while ((strLine = this.log.ReadLine()) != null)
			{
				// Match kills given
				if(EnableSnaps || EnableStats)
				{
					match = rKill.Match(strLine);
					if(match.Success) 
					{
						if(EnableStats)
						{
							String strGun = match.Groups[1].Value;
							CProfile_Gun gun;
							if(stats.gun.Contains(strGun))
							{
								gun = (CProfile_Gun)stats.gun[strGun];
								gun.kills++;
								stats.gun[strGun] = gun;
							}
							else 
							{
								gun = new CProfile_Gun();
								gun.kills = 1;
								stats.gun.Add(strGun, gun);
							}
						}
						NewSnaps = true;
						NewStats = true;
						continue;
					}
				}

				if(EnableStats)
				{
					// Match times self has been killed
					match = rKilled.Match(strLine);
					if(match.Success)
					{
						String strGun = match.Groups[1].Value;
						CProfile_Gun gun;
						if(stats.gun.Contains(strGun)) 
						{
							gun = (CProfile_Gun)stats.gun[strGun];
							gun.killed++;
							stats.gun[strGun] = gun;
						}
						else  
						{
							gun = new CProfile_Gun();
							gun.killed = 1;
							stats.gun.Add(strGun, gun);
						}
						NewStats = true;
						continue;
					}

					// Match nickchange of self
					match = rNickChange.Match(strLine);
					if(match.Success)
					{
						this.alias = match.Groups[1].Value;
						PopulateRegEx();
						NewStats = true;
						continue;
					}

					// Match Suicide
					match = rSuicide.Match(strLine);
					if(match.Success)
					{
						stats.suicides++;
						NewStats = true;
						continue;
					}

					// Match Misc Death
					match = rDeath.Match(strLine);
					if(match.Success)
					{
						stats.deaths++;
						NewStats = true;
						continue;
					}
				}
			}
		}
		public override bool Open()
		{
			try 
			{
				this.alias = GetGameAlias();
				this.log = new StreamReader(new FileStream(this.path + @"\console.log", FileMode.Open,  FileAccess.Read, FileShare.ReadWrite));
				this.log.BaseStream.Seek(0,SeekOrigin.End);		// Set to End
				PopulateRegEx();								// Create our Regex objects
				return true;
			}
			catch 
			{
				return false;
			}
		}
		public override bool CheckActive()
		{
			return NativeMethods.FindWindow("Valve001","Half-Life 2 DM") != 0;
		}
		public override String GetDefaultPath()
		{
			try
			{
				RegistryKey Key = Registry.CurrentUser;
				Key = Key.OpenSubKey(@"Software\Valve\Steam", false);
				frmSearch search = new frmSearch();
				search.Show();
				string result = search.Search(ProfileName, Path.GetDirectoryName(Key.GetValue("SteamExe").ToString()), 
					new Regex(@"\\half\-life 2 deathmatch\\hl2mp$", RegexOptions.IgnoreCase));
				search.Close();
				if(result != null)
					return result;
			}
			catch
			{
				return null;
			}
			return null;
		}
		// Get the game alias for said default user
		public String GetGameAlias()
		{
			String strLine;
			Match match;
			String nick = null;
			StreamReader sr = null;
			
			try
			{
				sr = new StreamReader(this.path + @"\cfg\config.cfg");
				//Continues to output one line at a time until end of file(EOF) is reached
				while ( (strLine = sr.ReadLine()) != null)
				{
					Regex rNick=new Regex("^name\\s+\"(.+)\"$");
					match = rNick.Match(strLine);
					if(match.Success)
					{
						nick = match.Groups[1].Value;
						break;
					}
				}
			}
			catch
			{
				nick = null;
			}
			finally 
			{
				// Cleanup
				if(sr != null) sr.Close();	
			}
			return nick;
		}
		public override void toXMLOperations()
		{
			// Turn gun hastable into something more useable
			if(stats.gun == null)
				return;
			stats.xmlgun = new CProfile_XMLGun [ stats.gun.Count ];
			int i = 0;
			foreach(String key in stats.gun.Keys)
			{
				stats.xmlgun[i] = new CProfile_XMLGun();
				stats.xmlgun[i].name = key;
				stats.xmlgun[i].stats = (CProfile_Gun)stats.gun[key];
				i++;
			}
		}
		public override void fromXMLOperations()
		{
			if(stats.xmlgun == null)
				return;

			if(stats.gun == null)
				stats.gun = new Hashtable();

			for(int i = 0; i < stats.xmlgun.Length; i++)
			{
				stats.gun.Add(stats.xmlgun[i].name, stats.xmlgun[i].stats);
			}
		}
		public override void ResetStats()
		{
			stats = new CProfileHalfLife2Deathmatch_Stats();
		}
		public override string GetStatsReport(String font, CProfile.SaveTypes format)
		{
			string strStats = null;
			CProfileHalfLife2Deathmatch_Stats stats = this.stats;
			switch(format)
			{
				case CProfile.SaveTypes.HTML:
				{
					strStats = "<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.1//EN\" \"http://www.w3.org/TR/xhtml11/DTD/xhtml11.dtd\">\r\n";
					strStats += "<html xmlns=\"http://www.w3.org/1999/xhtml\" xml:lang=\"en\">\r\n";
					strStats += "\t<head>\r\n";
					strStats += "\t\t<meta http-equiv=\"Content-Type\" content=\"application/xhtml+xml; charset=iso-8859-1\" />\r\n";
					strStats += "\t\t<meta http-equiv=\"Content-Style-Type\" content=\"text/css\" />\r\n";
					strStats += "\t\t<meta http-equiv=\"Content-Language\" content=\"EN\" />\r\n";
					strStats += "\t\t<meta name=\"description\" content=\"Smile! v" + Info.version + " -- " + ProfileName + " Generator.\" />\r\n";
					strStats += "\t\t<meta name=\"copyright\" content=\"©2005 Marek Kudlacz -- Kudlacz.com\" />\r\n";
					strStats += "\t\t<title>Smile! v" + Info.version + " - " + ProfileName + " Statistics</title>\r\n";
					strStats += "\t</head>\r\n";
					strStats += "\t<body>\r\n";
					strStats += "\t\t<h2>" + ProfileName + " Statistics:</h2>\r\n";
					strStats += "\t\t<hr />\r\n";
					strStats += "\t\tCreated with Smile! v" + Info.version + "<br />\r\n";
					strStats += "\t\t©2005 Marek Kudlacz -- <a href=\"http://www.kudlacz.com\">http://www.kudlacz.com</a><br />\r\n";
					strStats += "\t\t<hr />\r\n";

					strStats += "\t\t<h3>Misc Statistics:</h3>\r\n";
					strStats += "\t\tMiscellaneous Deaths: " + stats.deaths + "<br />\r\n";
					strStats += "\t\tSuicides: " + stats.suicides + "<br /><br />\r\n";

					strStats += "\t\t<h3>Weapon Statistics:</h3>\r\n";
					uint TotalKills = 0;
					uint TotalKilled = 0;
					foreach(String Key in stats.gun.Keys)
					{
						TotalKills += ((CProfile_Gun)stats.gun[Key]).kills;
						TotalKilled += ((CProfile_Gun)stats.gun[Key]).killed;
						strStats += "\t\t<b>"+ Key + ":</b> kills: " + ((CProfile_Gun)stats.gun[Key]).kills + " deaths: " + ((CProfile_Gun)stats.gun[Key]).killed + "<br />\r\n";
					}
					strStats += "\t\tTotal Kills: " + TotalKills + " Total Deaths: " + TotalKilled + "<br />\r\n";
					strStats += "\t\t<br /><br />\r\n";
					strStats += "\t</body>\r\n";
					strStats += "</html>";
					break;
				}
				case CProfile.SaveTypes.RTF:
				case CProfile.SaveTypes.TXT:
				{
					RichTextBox c = new RichTextBox();

					try
					{
						// Populate the rich text box
						c.SelectionStart = 0 ;
						c.SelectionFont = new Font(font, 12, FontStyle.Bold);
						c.SelectedText = ProfileName + " Statistics:\n" ;
						c.SelectionFont = new Font(font, 12, FontStyle.Bold|FontStyle.Underline);
						c.SelectedText = "                                                                \n\n";
						c.SelectedText = "Created with Smile! v" + Info.version + "\n";
						c.SelectedText = "©2005 Marek Kudlacz -- http://www.kudlacz.com\n";
						c.SelectionFont = new Font(font, 12, FontStyle.Bold|FontStyle.Underline);
						c.SelectedText = "                                                                \n\n";

						c.SelectionFont = new Font(font, 10, FontStyle.Bold);
						c.SelectedText = "Misc Statistics:\n" ;
						c.SelectedText = "Miscellaneous Deaths: " + stats.deaths + "\n";
						c.SelectedText = "Suicides: " + stats.suicides + "\n";
						c.SelectedText = "\n";

						c.SelectionFont = new Font(font, 10, FontStyle.Bold);
						c.SelectedText = "Weapon Statistics:\n" ;
						uint TotalKills = 0;
						uint TotalKilled = 0;
						foreach(String Key in stats.gun.Keys)
						{
							TotalKills += ((CProfile_Gun)stats.gun[Key]).kills;
							TotalKilled += ((CProfile_Gun)stats.gun[Key]).killed;
							c.SelectionFont = new Font(font, 9, FontStyle.Bold|FontStyle.Italic);
							c.SelectedText = Key + ": ";
							c.SelectionFont = new Font(font, 9, FontStyle.Regular);
							c.SelectedText = " kills: " + ((CProfile_Gun)stats.gun[Key]).kills;
							c.SelectedText = " deaths: " + ((CProfile_Gun)stats.gun[Key]).killed + "\n";
						}
						c.SelectedText = "Total Kills: " + TotalKills + " Total Deaths: " + TotalKilled + "\n";
						c.SelectedText = "\n\n";

						c.SelectionStart = 0 ;
					}
					catch
					{
						c.Clear();
						c.SelectionStart = 0 ;
						c.SelectedText = "There was an error writing the stats, (missing font?) Try saving it to html instead using the edit menu.\n\n";
						c.SelectionStart = 0 ;
					}

					switch(format)
					{
						case CProfile.SaveTypes.RTF:
							strStats = c.Rtf;
							break;
						case CProfile.SaveTypes.TXT:
							strStats = c.Text;
							break;
					}
					c.Dispose();
					break;
				}
			}
			return strStats;
		}
	}

	//////////////////////////////////////////////////////////////////////////////////////////////
	/// CounterStrike 1.6
	//////////////////////////////////////////////////////////////////////////////////////////////
	public class CProfileCounterStrike_Stats
	{
		public UInt32 deaths;		// misc deaths, does not include being killed by weapon
		public UInt32 teamattacks;
		[XmlIgnoreAttribute] public Hashtable gun;
		[XmlElement(ElementName = "gun")]public CProfile_XMLGun [] xmlgun;

		public CProfileCounterStrike_Stats()
		{
			deaths = new UInt32();
			teamattacks = new UInt32();
			gun = new Hashtable();
		}
	}

	public class CProfileCounterStrike : CProfile
	{
		public CProfileCounterStrike_Stats stats;

		private String alias;
		private Regex rKill;
		private Regex rKilled;
		private Regex rNickChange;
		private Regex rTeamAttack;
		private Regex rDeath;

		public CProfileCounterStrike()
		{
			this.ProfileName = "Counter-Strike";
			this.SnapName = "Counter-Strike";
			this.stats = new CProfileCounterStrike_Stats();
		}
		private void PopulateRegEx()
		{
			rKill = new Regex("^" + Regex.Escape(this.alias) + " killed .+ with (\\w+)$");
			rKilled = new Regex("^.+ killed " + Regex.Escape(this.alias) + " with (\\w+)$");
			rNickChange = new Regex("^" + Regex.Escape(this.alias) + " is now known as (.+)$");
			rDeath = new Regex("^" + Regex.Escape(this.alias) + " died$");
			rTeamAttack = new Regex("^" + Regex.Escape(this.alias) + " attacked a teammate$");
		}

		public override void Parse()
		{
			if(this.log == null)
				return;

			Match match;
			String strLine;

			while ((strLine = this.log.ReadLine()) != null)
			{
				// Match kills given
				if(EnableSnaps || EnableStats)
				{
					match = rKill.Match(strLine);
					if(match.Success) 
					{
						if(EnableStats)
						{
							String strGun = match.Groups[1].Value;
							CProfile_Gun gun;
							if(stats.gun.Contains(strGun))
							{
								gun = (CProfile_Gun)stats.gun[strGun];
								gun.kills++;
								stats.gun[strGun] = gun;
							}
							else 
							{
								gun = new CProfile_Gun();
								gun.kills = 1;
								stats.gun.Add(strGun, gun);
							}
						}
						NewSnaps = true;
						NewStats = true;
						continue;
					}
				}

				if(EnableStats)
				{
					// Match times self has been killed
					match = rKilled.Match(strLine);
					if(match.Success)
					{
						String strGun = match.Groups[1].Value;
						CProfile_Gun gun;
						if(stats.gun.Contains(strGun)) 
						{
							gun = (CProfile_Gun)stats.gun[strGun];
							gun.killed++;
							stats.gun[strGun] = gun;
						}
						else  
						{
							gun = new CProfile_Gun();
							gun.killed = 1;
							stats.gun.Add(strGun, gun);
						}
						NewStats = true;
						continue;
					}

					// Match nickchange of self
					match = rNickChange.Match(strLine);
					if(match.Success)
					{
						this.alias = match.Groups[1].Value;
						PopulateRegEx();
						NewStats = true;
						continue;
					}

					// Match Misc Death
					match = rDeath.Match(strLine);
					if(match.Success)
					{
						stats.deaths++;
						NewStats = true;
						continue;
					}

					// Team mate attacks
					match = rTeamAttack.Match(strLine);
					if(match.Success)
					{
						stats.teamattacks++;
						NewStats = true;
						continue;
					}
				}
			}
		}
		public override bool Open()
		{
			try 
			{
				this.alias = GetGameAlias();
				this.log = new StreamReader(new FileStream(this.path + @"\qconsole.log", FileMode.Open,  FileAccess.Read, FileShare.ReadWrite));
				this.log.BaseStream.Seek(0,SeekOrigin.End);		// Set to End
				PopulateRegEx();								// Create our Regex objects
				return true;
			}
			catch 
			{
				return false;
			}
		}
		public override bool CheckActive()
		{
			return NativeMethods.FindWindow("Valve001","Counter-Strike") != 0;
		}
		public override String GetDefaultPath()
		{
			try
			{
				RegistryKey Key = Registry.CurrentUser;
				Key = Key.OpenSubKey(@"Software\Valve\Steam", false);
				frmSearch search = new frmSearch();
				search.Show();
				string result = search.Search(ProfileName, Path.GetDirectoryName(Key.GetValue("SteamExe").ToString()), 
					new Regex(@"\\counter\-strike$", RegexOptions.IgnoreCase));
				search.Close();
				if(result != null)
					return result;
			}
			catch
			{
				return null;
			}
			return null;
		}
		public String GetGameAlias()
		{
			String strLine;
			Match match;
			String nick = null;
			StreamReader sr = null;
			
			try
			{
				sr = new StreamReader(this.path + @"\cstrike\config.cfg");
				//Continues to output one line at a time until end of file(EOF) is reached
				while ( (strLine = sr.ReadLine()) != null)
				{
					Regex rNick=new Regex("^name\\s+\"(.+)\"$");
					match = rNick.Match(strLine);
					if(match.Success)
					{
						nick = match.Groups[1].Value;
						break;
					}
				}
			}
			catch
			{
				nick = null;
			}
			finally 
			{
				// Cleanup
				if(sr != null) sr.Close();	
			}
			return nick;
		}
		public override void toXMLOperations()
		{
			// Turn gun hastable into something more useable
			if(stats.gun == null)
				return;
			stats.xmlgun = new CProfile_XMLGun [ stats.gun.Count ];
			int i = 0;
			foreach(String key in stats.gun.Keys)
			{
				stats.xmlgun[i] = new CProfile_XMLGun();
				stats.xmlgun[i].name = key;
				stats.xmlgun[i].stats = (CProfile_Gun)stats.gun[key];
				i++;
			}
		}
		public override void fromXMLOperations()
		{
			if(stats.xmlgun == null)
				return;

			if(stats.gun == null)
				stats.gun = new Hashtable();

			for(int i = 0; i < stats.xmlgun.Length; i++)
			{
				stats.gun.Add(stats.xmlgun[i].name, stats.xmlgun[i].stats);
			}
		}
		public override void ResetStats()
		{
			stats = new CProfileCounterStrike_Stats();
		}
		public override string GetStatsReport(String font, CProfile.SaveTypes format)
		{
			string strStats = null;
			CProfileCounterStrike_Stats stats = this.stats;
			switch(format)
			{
				case CProfile.SaveTypes.HTML:
				{
					strStats = "<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.1//EN\" \"http://www.w3.org/TR/xhtml11/DTD/xhtml11.dtd\">\r\n";
					strStats += "<html xmlns=\"http://www.w3.org/1999/xhtml\" xml:lang=\"en\">\r\n";
					strStats += "\t<head>\r\n";
					strStats += "\t\t<meta http-equiv=\"Content-Type\" content=\"application/xhtml+xml; charset=iso-8859-1\" />\r\n";
					strStats += "\t\t<meta http-equiv=\"Content-Style-Type\" content=\"text/css\" />\r\n";
					strStats += "\t\t<meta http-equiv=\"Content-Language\" content=\"EN\" />\r\n";
					strStats += "\t\t<meta name=\"description\" content=\"Smile! v" + Info.version + " -- " + ProfileName + " Generator.\" />\r\n";
					strStats += "\t\t<meta name=\"copyright\" content=\"©2005 Marek Kudlacz -- Kudlacz.com\" />\r\n";
					strStats += "\t\t<title>Smile! v" + Info.version + " - " + ProfileName + " Statistics</title>\r\n";
					strStats += "\t</head>\r\n";
					strStats += "\t<body>\r\n";
					strStats += "\t\t<h2>" + ProfileName + " Statistics:</h2>\r\n";
					strStats += "\t\t<hr />\r\n";
					strStats += "\t\tCreated with Smile! v" + Info.version + "<br />\r\n";
					strStats += "\t\t©2005 Marek Kudlacz -- <a href=\"http://www.kudlacz.com\">http://www.kudlacz.com</a><br />\r\n";
					strStats += "\t\t<hr />\r\n";

					strStats += "\t\t<h3>Misc Statistics:</h3>\r\n";
					strStats += "\t\tFriendly-fire Attacks: " + stats.teamattacks + "<br />\r\n";
					strStats += "\t\tMiscellaneous Deaths: " + stats.deaths + "<br />\r\n";

					strStats += "\t\t<h3>Weapon Statistics:</h3>\r\n";
					uint TotalKills = 0;
					uint TotalKilled = 0;
					foreach(String Key in stats.gun.Keys)
					{
						TotalKills += ((CProfile_Gun)stats.gun[Key]).kills;
						TotalKilled += ((CProfile_Gun)stats.gun[Key]).killed;
						strStats += "\t\t<b>"+ Key + ":</b> kills: " + ((CProfile_Gun)stats.gun[Key]).kills + " deaths: " + ((CProfile_Gun)stats.gun[Key]).killed + "<br />\r\n";
					}
					strStats += "\t\tTotal Kills: " + TotalKills + " Total Deaths: " + TotalKilled + "<br />\r\n";
					strStats += "\t\t<br /><br />\r\n";
					strStats += "\t</body>\r\n";
					strStats += "</html>";
					break;
				}
				case CProfile.SaveTypes.RTF:
				case CProfile.SaveTypes.TXT:
				{
					RichTextBox c = new RichTextBox();

					try
					{
						// Populate the rich text box
						c.SelectionStart = 0 ;
						c.SelectionFont = new Font(font, 12, FontStyle.Bold);
						c.SelectedText = ProfileName + " Statistics:\n" ;
						c.SelectionFont = new Font(font, 12, FontStyle.Bold|FontStyle.Underline);
						c.SelectedText = "                                                                \n\n";
						c.SelectedText = "Created with Smile! v" + Info.version + "\n";
						c.SelectedText = "©2005 Marek Kudlacz -- http://www.kudlacz.com\n";
						c.SelectionFont = new Font(font, 12, FontStyle.Bold|FontStyle.Underline);
						c.SelectedText = "                                                                \n\n";

						c.SelectionFont = new Font(font, 10, FontStyle.Bold);
						c.SelectedText = "Misc Statistics:\n" ;
						c.SelectedText = "Friendly-fire Attacks: " + stats.teamattacks + "\n";
						c.SelectedText = "Miscellaneous Deaths: " + stats.deaths + "\n";
						c.SelectedText = "\n";

						c.SelectionFont = new Font(font, 10, FontStyle.Bold);
						c.SelectedText = "Weapon Statistics:\n" ;
						uint TotalKills = 0;
						uint TotalKilled = 0;
						foreach(String Key in stats.gun.Keys)
						{
							TotalKills += ((CProfile_Gun)stats.gun[Key]).kills;
							TotalKilled += ((CProfile_Gun)stats.gun[Key]).killed;
							c.SelectionFont = new Font(font, 9, FontStyle.Bold|FontStyle.Italic);
							c.SelectedText = Key + ": ";
							c.SelectionFont = new Font(font, 9, FontStyle.Regular);
							c.SelectedText = " kills: " + ((CProfile_Gun)stats.gun[Key]).kills;
							c.SelectedText = " deaths: " + ((CProfile_Gun)stats.gun[Key]).killed + "\n";
						}
						c.SelectedText = "Total Kills: " + TotalKills + " Total Deaths: " + TotalKilled + "\n";
						c.SelectedText = "\n\n";

						c.SelectionStart = 0 ;
					}
					catch
					{
						c.Clear();
						c.SelectionStart = 0 ;
						c.SelectedText = "There was an error writing the stats, (missing font?) Try saving it to html instead using the edit menu.\n\n";
						c.SelectionStart = 0 ;
					}

					switch(format)
					{
						case CProfile.SaveTypes.RTF:
							strStats = c.Rtf;
							break;
						case CProfile.SaveTypes.TXT:
							strStats = c.Text;
							break;
					}
					c.Dispose();
					break;
				}
			}
			return strStats;
		}
	}

	//////////////////////////////////////////////////////////////////////////////////////////////
	/// Day of Defeat
	//////////////////////////////////////////////////////////////////////////////////////////////
	public class CProfileDayofDefeat_Stats
	{
		public UInt32 deaths;		// misc deaths, does not include being killed by weapon
		public UInt32 teamattacks;
		public UInt32 suicides;
		public UInt32 alliedcaptures;
		public UInt32 axiscaptures;
		[XmlIgnoreAttribute] public Hashtable gun;
		[XmlElement(ElementName = "gun")]public CProfileDayofDefeat_XMLGun [] xmlgun;

		public CProfileDayofDefeat_Stats()
		{
			deaths = new UInt32();
			suicides = new UInt32();
			teamattacks = new UInt32();
			axiscaptures = new UInt32();
			alliedcaptures = new UInt32();
			gun = new Hashtable();
		}
	}

	public class CProfileDayofDefeat_XMLGun
	{
		public String name;
		public CProfileDayofDefeat_Gun stats;
		
		public CProfileDayofDefeat_XMLGun()
		{
			stats = new CProfileDayofDefeat_Gun();
		}
	}
	public class  CProfileDayofDefeat_Gun : CProfile_Gun
	{
		public UInt32 teamkills;
		public UInt32 teamkilled;

		public  CProfileDayofDefeat_Gun()
		{
			this.teamkills = new UInt32();	
			this.teamkilled = new UInt32();	
		}
	}

	public class CProfileDayofDefeat : CProfile
	{
		public CProfileDayofDefeat_Stats stats;

		private String alias;
		private Regex rKill;
		private Regex rKilled;
		private Regex rNickChange;
		private Regex rTeamAttack;
		private Regex rTeamKill;
		private Regex rTeamKilled;
		private Regex rSuicide;
		private Regex rDeath;
		private Regex rCapturedAxis;
		private Regex rCapturedAllies;

		public CProfileDayofDefeat()
		{
			this.ProfileName = "Day of Defeat";
			this.SnapName = "Day of Defeat";
			this.stats = new CProfileDayofDefeat_Stats();
		}
		public override bool CheckActive()
		{
			return NativeMethods.FindWindow("Valve001","Day of Defeat") != 0;
		}
		public override String GetDefaultPath()
		{
			try
			{
				RegistryKey Key = Registry.CurrentUser;
				Key = Key.OpenSubKey(@"Software\Valve\Steam", false);
				frmSearch search = new frmSearch();
				search.Show();
				string result = search.Search(ProfileName, Path.GetDirectoryName(Key.GetValue("SteamExe").ToString()), 
					new Regex(@"\\day of defeat$", RegexOptions.IgnoreCase));
				search.Close();
				if(result != null)
					return result;
			}
			catch
			{
				return null;
			}
			return null;
		}
		private void PopulateRegEx()
		{
			rKill = new Regex("^" + Regex.Escape(this.alias) + " killed .+ with (\\w+)$");
			rKilled = new Regex("^.+ killed " + Regex.Escape(this.alias) + " with (\\w+)$");
			rTeamKill = new Regex("^" + Regex.Escape(this.alias) + " killed his teammate .+ with (\\w+)$");
			rTeamKilled = new Regex("^.+ killed his teammate " + Regex.Escape(this.alias) + " with (\\w+)$");
			rNickChange = new Regex("^" + Regex.Escape(this.alias) + " is now known as (.+)$");
			rSuicide = new Regex("^" + Regex.Escape(this.alias) + " killed self$");
			rDeath = new Regex("^" + Regex.Escape(this.alias) + " died$");
			rTeamAttack = new Regex("^" + Regex.Escape(this.alias) + " attacked teammate .+$");
			rCapturedAxis = new Regex("^" + Regex.Escape(this.alias) + " captured .+ for the Axis$");
			rCapturedAllies = new Regex("^" + Regex.Escape(this.alias) + " captured .+ for the Allies$");
		}

		public override void Parse()
		{
			if(this.log == null)
				return;

			Match match;
			String strLine;

			while ((strLine = this.log.ReadLine()) != null)
			{
				// Match kills given
				if(EnableSnaps || EnableStats)
				{
					match = rKill.Match(strLine);
					if(match.Success) 
					{
						if(EnableStats)
						{
							String strGun = match.Groups[1].Value;
							CProfileDayofDefeat_Gun gun;
							if(stats.gun.Contains(strGun))
							{
								gun = (CProfileDayofDefeat_Gun)stats.gun[strGun];
								gun.kills++;
								stats.gun[strGun] = gun;
							}
							else 
							{
								gun = new CProfileDayofDefeat_Gun();
								gun.kills = 1;
								stats.gun.Add(strGun, gun);
							}
						}
						NewSnaps = true;
						NewStats = true;
						continue;
					}
				}

				if(EnableStats)
				{
					// Match times killed team
					match = rTeamKill.Match(strLine);
					if(match.Success)
					{
						String strGun = match.Groups[1].Value;
						CProfileDayofDefeat_Gun gun;
						if(stats.gun.Contains(strGun)) 
						{
							gun = (CProfileDayofDefeat_Gun)stats.gun[strGun];
							gun.teamkills++;
							stats.gun[strGun] = gun;
						}
						else  
						{
							gun = new CProfileDayofDefeat_Gun();
							gun.teamkills = 1;
							stats.gun.Add(strGun, gun);
						}
						NewStats = true;
						continue;
					}

					// Match times team killed
					match = rTeamKilled.Match(strLine);
					if(match.Success)
					{
						String strGun = match.Groups[1].Value;
						CProfileDayofDefeat_Gun gun;
						if(stats.gun.Contains(strGun)) 
						{
							gun = (CProfileDayofDefeat_Gun)stats.gun[strGun];
							gun.teamkilled++;
							stats.gun[strGun] = gun;
						}
						else  
						{
							gun = new CProfileDayofDefeat_Gun();
							gun.teamkilled = 1;
							stats.gun.Add(strGun, gun);
						}
						NewStats = true;
						continue;
					}

					// Match times self has been killed
					match = rKilled.Match(strLine);
					if(match.Success)
					{
						String strGun = match.Groups[1].Value;
						CProfileDayofDefeat_Gun gun;
						if(stats.gun.Contains(strGun)) 
						{
							gun = (CProfileDayofDefeat_Gun)stats.gun[strGun];
							gun.killed++;
							stats.gun[strGun] = gun;
						}
						else  
						{
							gun = new CProfileDayofDefeat_Gun();
							gun.killed = 1;
							stats.gun.Add(strGun, gun);
						}
						NewStats = true;
						continue;
					}

					// Match nickchange of self
					match = rNickChange.Match(strLine);
					if(match.Success)
					{
						this.alias = match.Groups[1].Value;
						PopulateRegEx();
						NewStats = true;
						continue;
					}

					// Match Suicide
					match = rSuicide.Match(strLine);
					if(match.Success)
					{
						stats.suicides++;
						NewStats = true;
						continue;
					}

					// Match Allied Captures
					match = rCapturedAllies.Match(strLine);
					if(match.Success)
					{
						stats.alliedcaptures++;
						NewStats = true;
						continue;
					}

					// Match Axis Captures
					match = rCapturedAxis.Match(strLine);
					if(match.Success)
					{
						stats.axiscaptures++;
						NewStats = true;
						continue;
					}

					// Match Misc Death
					match = rDeath.Match(strLine);
					if(match.Success)
					{
						stats.deaths++;
						NewStats = true;
						continue;
					}

					// Team mate attacks
					match = rTeamAttack.Match(strLine);
					if(match.Success)
					{
						stats.teamattacks++;
						NewStats = true;
						continue;
					}
				}
			}
		}
		public override bool Open()
		{
			try 
			{
				this.alias = GetGameAlias();
				this.log = new StreamReader(new FileStream(this.path + @"\qconsole.log", FileMode.Open,  FileAccess.Read, FileShare.ReadWrite));
				this.log.BaseStream.Seek(0,SeekOrigin.End);		// Set to End
				PopulateRegEx();								// Create our Regex objects
				return true;
			}
			catch 
			{
				return false;
			}
		}
		public String GetGameAlias()
		{
			String strLine;
			Match match;
			String nick = null;
			StreamReader sr = null;
			
			try
			{
				sr = new StreamReader(this.path + @"\dod\config.cfg");
				//Continues to output one line at a time until end of file(EOF) is reached
				while ( (strLine = sr.ReadLine()) != null)
				{
					Regex rNick=new Regex("^name\\s+\"(.+)\"$");
					match = rNick.Match(strLine);
					if(match.Success)
					{
						nick = match.Groups[1].Value;
						break;
					}
				}
			}
			catch
			{
				nick = null;
			}
			finally 
			{
				// Cleanup
				if(sr != null) sr.Close();	
			}
			return nick;
		}
		public override void toXMLOperations()
		{
			// Turn gun hastable into something more useable
			if(stats.gun == null)
				return;
			stats.xmlgun = new CProfileDayofDefeat_XMLGun [ stats.gun.Count ];
			int i = 0;
			foreach(String key in stats.gun.Keys)
			{
				stats.xmlgun[i] = new CProfileDayofDefeat_XMLGun();
				stats.xmlgun[i].name = key;
				stats.xmlgun[i].stats = (CProfileDayofDefeat_Gun)stats.gun[key];
				i++;
			}
		}
		public override void fromXMLOperations()
		{
			if(stats.xmlgun == null)
				return;

			if(stats.gun == null)
				stats.gun = new Hashtable();

			for(int i = 0; i < stats.xmlgun.Length; i++)
			{
				stats.gun.Add(stats.xmlgun[i].name, stats.xmlgun[i].stats);
			}
		}
		public override void ResetStats()
		{
			stats = new CProfileDayofDefeat_Stats();
		}
		public override string GetStatsReport(String font, CProfile.SaveTypes format)
		{
			string strStats = null;
			CProfileDayofDefeat_Stats stats = this.stats;
			switch(format)
			{
				case CProfile.SaveTypes.HTML:
				{
					strStats = "<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.1//EN\" \"http://www.w3.org/TR/xhtml11/DTD/xhtml11.dtd\">\r\n";
					strStats += "<html xmlns=\"http://www.w3.org/1999/xhtml\" xml:lang=\"en\">\r\n";
					strStats += "\t<head>\r\n";
					strStats += "\t\t<meta http-equiv=\"Content-Type\" content=\"application/xhtml+xml; charset=iso-8859-1\" />\r\n";
					strStats += "\t\t<meta http-equiv=\"Content-Style-Type\" content=\"text/css\" />\r\n";
					strStats += "\t\t<meta http-equiv=\"Content-Language\" content=\"EN\" />\r\n";
					strStats += "\t\t<meta name=\"description\" content=\"Smile! v" + Info.version + " -- " + ProfileName + " Generator.\" />\r\n";
					strStats += "\t\t<meta name=\"copyright\" content=\"©2005 Marek Kudlacz -- Kudlacz.com\" />\r\n";
					strStats += "\t\t<title>Smile! v" + Info.version + " - " + ProfileName + " Statistics</title>\r\n";
					strStats += "\t</head>\r\n";
					strStats += "\t<body>\r\n";
					strStats += "\t\t<h2>" + ProfileName + " Statistics:</h2>\r\n";
					strStats += "\t\t<hr />\r\n";
					strStats += "\t\tCreated with Smile! v" + Info.version + "<br />\r\n";
					strStats += "\t\t©2005 Marek Kudlacz -- <a href=\"http://www.kudlacz.com\">http://www.kudlacz.com</a><br />\r\n";
					strStats += "\t\t<hr />\r\n";

					strStats += "\t\t<h3>Misc Statistics:</h3>\r\n";
					strStats += "\t\tAllied Captures: " + stats.alliedcaptures + "<br />\r\n";
					strStats += "\t\tAxis Captures: " + stats.axiscaptures + "<br />\r\n";
					strStats += "\t\tFriendly-fire Attacks: " + stats.teamattacks + "<br />\r\n";
					strStats += "\t\tMiscellaneous Deaths: " + stats.deaths + "<br />\r\n";
					strStats += "\t\tSuicides: " + stats.suicides + "<br /><br />\r\n";

					strStats += "\t\t<h3>Weapon Statistics:</h3>\r\n";
					uint TotalKills = 0;
					uint TotalKilled = 0;
					uint TotalTeamKills = 0;
					uint TotalTeamKilled = 0;
					foreach(String Key in stats.gun.Keys)
					{
						TotalKills += ((CProfileDayofDefeat_Gun)stats.gun[Key]).kills;
						TotalKilled += ((CProfileDayofDefeat_Gun)stats.gun[Key]).killed;
						TotalTeamKills += ((CProfileDayofDefeat_Gun)stats.gun[Key]).teamkills;
						TotalTeamKilled += ((CProfileDayofDefeat_Gun)stats.gun[Key]).teamkilled;
						strStats += "\t\t<b>"+ Key + ":</b> kills: " + ((CProfileDayofDefeat_Gun)stats.gun[Key]).kills + " team kills: " + ((CProfileDayofDefeat_Gun)stats.gun[Key]).teamkills + " killed: " + ((CProfileDayofDefeat_Gun)stats.gun[Key]).killed + " team killed: " + ((CProfileDayofDefeat_Gun)stats.gun[Key]).teamkilled + "<br />\r\n";
					}
					strStats += "\t\tTotal Kills: " + TotalKills + " Total Team Kills: " + TotalTeamKills + " Total Killed: " + TotalKilled + " Total Team Killed: " + TotalTeamKilled + "<br />\r\n";
					strStats += "\t\t<br /><br />\r\n";
					strStats += "\t</body>\r\n";
					strStats += "</html>";
					break;
				}
				case CProfile.SaveTypes.RTF:
				case CProfile.SaveTypes.TXT:
				{
					RichTextBox c = new RichTextBox();

					try
					{
						// Populate the rich text box
						c.SelectionStart = 0 ;
						c.SelectionFont = new Font(font, 12, FontStyle.Bold);
						c.SelectedText = ProfileName + " Statistics:\n" ;
						c.SelectionFont = new Font(font, 12, FontStyle.Bold|FontStyle.Underline);
						c.SelectedText = "                                                                \n\n";
						c.SelectedText = "Created with Smile! v" + Info.version + "\n";
						c.SelectedText = "©2005 Marek Kudlacz -- http://www.kudlacz.com\n";
						c.SelectionFont = new Font(font, 12, FontStyle.Bold|FontStyle.Underline);
						c.SelectedText = "                                                                \n\n";

						c.SelectionFont = new Font(font, 10, FontStyle.Bold);
						c.SelectedText = "Misc Statistics:\n" ;
						c.SelectedText = "Allied Captures: " + stats.alliedcaptures + "\n";
						c.SelectedText = "Axis Captures: " + stats.axiscaptures + "\n";
						c.SelectedText = "Friendly-fire Attacks: " + stats.teamattacks + "\n";
						c.SelectedText = "Miscellaneous Deaths: " + stats.deaths + "\n";
						c.SelectedText = "Suicides: " + stats.suicides + "\n";
						c.SelectedText = "\n";

						c.SelectionFont = new Font(font, 10, FontStyle.Bold);
						c.SelectedText = "Weapon Statistics:\n" ;
						uint TotalKills = 0;
						uint TotalKilled = 0;
						uint TotalTeamKills = 0;
						uint TotalTeamKilled = 0;
						foreach(String Key in stats.gun.Keys)
						{
							TotalKills += ((CProfileDayofDefeat_Gun)stats.gun[Key]).kills;
							TotalKilled += ((CProfileDayofDefeat_Gun)stats.gun[Key]).killed;
							TotalTeamKills += ((CProfileDayofDefeat_Gun)stats.gun[Key]).teamkills;
							TotalTeamKilled += ((CProfileDayofDefeat_Gun)stats.gun[Key]).teamkilled;
							c.SelectionFont = new Font(font, 9, FontStyle.Bold|FontStyle.Italic);
							c.SelectedText = Key + ": ";
							c.SelectionFont = new Font(font, 9, FontStyle.Regular);
							c.SelectedText = " kills: " + ((CProfileDayofDefeat_Gun)stats.gun[Key]).kills;
							c.SelectedText = " team kills: " + ((CProfileDayofDefeat_Gun)stats.gun[Key]).teamkills;
							c.SelectedText = " killed: " + ((CProfileDayofDefeat_Gun)stats.gun[Key]).killed;
							c.SelectedText = " team killed: " + ((CProfileDayofDefeat_Gun)stats.gun[Key]).teamkilled + "\n";
						}
						c.SelectedText = "Total Kills: " + TotalKills + " Total Team Kills: " + TotalTeamKills +  " Total Killed: " + TotalKilled + " Total Team Killed: " + TotalTeamKilled + "\n";
						c.SelectedText = "\n\n";

						c.SelectionStart = 0 ;
					}
					catch
					{
						c.Clear();
						c.SelectionStart = 0 ;
						c.SelectedText = "There was an error writing the stats, (missing font?) Try saving it to html instead using the edit menu.\n\n";
						c.SelectionStart = 0 ;
					}

					switch(format)
					{
						case CProfile.SaveTypes.RTF:
							strStats = c.Rtf;
							break;
						case CProfile.SaveTypes.TXT:
							strStats = c.Text;
							break;
					}
					c.Dispose();
					break;
				}
			}
			return strStats;
		}
	}

	//////////////////////////////////////////////////////////////////////////////////////////////
	/// Quake III Arena
	//////////////////////////////////////////////////////////////////////////////////////////////

	public class CProfileQuakeIIIArena_MOD
	{
		public string name;
		public string mod;
		public Regex r;
		public CProfileQuakeIIIArena_MOD(string name, string mod)
		{
			this.name = name;
			this.mod = mod;
			r = new Regex (mod);
		}
	}
	public class CProfileQuakeIIIArena_XMLDeath
	{
		public String name;
		public UInt32 deaths;
	
		public CProfileQuakeIIIArena_XMLDeath()
		{
			deaths = new UInt32();
		}
	}
	public class CProfileQuakeIIIArena_Stats
	{
		public UInt32 hits_team;
		[XmlIgnoreAttribute] public Hashtable gun;
		[XmlIgnoreAttribute] public Hashtable death;
		[XmlElement(ElementName = "gun")]public CProfile_XMLGun [] xmlgun;
		[XmlElement(ElementName = "death")]public CProfileQuakeIIIArena_XMLDeath [] xmldeath;

		public CProfileQuakeIIIArena_Stats()
		{
			death = new Hashtable();
			gun = new Hashtable();
		}
	}

	public class CProfileQuakeIIIArena : CProfile
	{
		public CProfileQuakeIIIArena_Stats stats;

		protected String alias;
		private CProfileQuakeIIIArena_MOD [] weapons;
		private CProfileQuakeIIIArena_MOD [] deaths;
		private Regex rNickChange;
		private Regex rStripColor;

		public CProfileQuakeIIIArena()
		{
			this.ProfileName = "Quake III Arena";
			this.SnapName = "Quake III Arena";
			this.stats = new CProfileQuakeIIIArena_Stats();
			this.deaths = new CProfileQuakeIIIArena_MOD [] 
				{
					new CProfileQuakeIIIArena_MOD("Suicide", "^(.+) suicides$"),
					new CProfileQuakeIIIArena_MOD("Falling", "^(.+) cratered$"),
					new CProfileQuakeIIIArena_MOD("Crush", "^(.+) was squished$"),
					new CProfileQuakeIIIArena_MOD("Water", "^(.+) sank like a rock$"),
					new CProfileQuakeIIIArena_MOD("Slime", "^(.+) melted$"),
					new CProfileQuakeIIIArena_MOD("Lava", "^(.+) does a back flip into the lava$"),
					new CProfileQuakeIIIArena_MOD("Target_laser", "(^.+) saw the light$"),
					new CProfileQuakeIIIArena_MOD("Trigger_hurt", "^(.+) was in the wrong place$"),
					new CProfileQuakeIIIArena_MOD("Own Grenades", "^(.+) tripped on .+ own grenade$"),
					new CProfileQuakeIIIArena_MOD("Own Rocket", "^(.+) blew .+self up$"),
					new CProfileQuakeIIIArena_MOD("Own Plasma", "^(.+) melted .+self$"),
					new CProfileQuakeIIIArena_MOD("Own BFG", "^(.+) should have used a smaller gun$"),
					new CProfileQuakeIIIArena_MOD("Own Prox Mine", "^(.+) found .+ prox mine$"),
					new CProfileQuakeIIIArena_MOD("Other Self-Inflicted Means", "^(.+) killed .+self$"),
					new CProfileQuakeIIIArena_MOD("Unknown means", "^(.+) died\\.$")
				};
			this.weapons = new CProfileQuakeIIIArena_MOD [] 
				{ 
					new CProfileQuakeIIIArena_MOD("Grapple", "^(.+) was caught by (.+)$"),
					new CProfileQuakeIIIArena_MOD("Gauntlet", "^(.+) was pummeled by (.+)$"),
					new CProfileQuakeIIIArena_MOD("Machine Gun", "^(.+) was machinegunned by (.+)$"),
					new CProfileQuakeIIIArena_MOD("Shotgun", "^(.+) was gunned down by (.+)$"),
					new CProfileQuakeIIIArena_MOD("Grenade", "^(.+) ate (.+)'s grenade$"),
					new CProfileQuakeIIIArena_MOD("Grenade Splash", "^(.+) was shredded by (.+)'s shrapnel$"),
					new CProfileQuakeIIIArena_MOD("Rocket", "^(.+) ate (.+)'s rocket$"),
					new CProfileQuakeIIIArena_MOD("Rocket Splash", "^(.+) almost dodged (.+)'s rocket$"),
					new CProfileQuakeIIIArena_MOD("Plasma", "^(.+) was melted by (.+)'s plasmagun$"),
					new CProfileQuakeIIIArena_MOD("Railgun", "^(.+) was railed by (.+)$"),
					new CProfileQuakeIIIArena_MOD("Lightning", "^(.+) was electrocuted by (.+)$"),
					new CProfileQuakeIIIArena_MOD("BFG", "^(.+) was blasted by (.+)'s BFG$"),
					new CProfileQuakeIIIArena_MOD("Nail", "^(.+) was nailed by (.+)$"),
					new CProfileQuakeIIIArena_MOD("Chaingun", "^(.+) got lead poisoning from (.+)'s Chaingun$"),
					new CProfileQuakeIIIArena_MOD("Prox Mine", "^(.+) was too close to (.+)'s Prox Mine$"),
					new CProfileQuakeIIIArena_MOD("Nail", "^(.+) falls to (.+)'s Kamikaze blast$"),
					new CProfileQuakeIIIArena_MOD("Juiced", "(^.+) was juiced by (.+)$"),
					new CProfileQuakeIIIArena_MOD("Telefrag", "^(.+) tried to invade (.+)'s personal space$"),
					new CProfileQuakeIIIArena_MOD("Other", "^(.+) was killed by (.+)$")
				};
				this.rNickChange = new Regex("^(.+) renamed to (.+)$");
				this.rStripColor = new Regex("\\^\\d");
		}

		public override void Parse()
		{
			if(this.log == null)
				return;

			Match match;
			String strLine;
			bool found;

			while ((strLine = this.log.ReadLine()) != null)
			{
				strLine = rStripColor.Replace(strLine, "");

				// Match weapon kills/deaths
				found = false;
				for(int i = 0; i < weapons.Length; i++)
				{
					match = weapons[i].r.Match(strLine);
					if(match.Success)
					{
						// Kills
						if(match.Groups[2].Value == alias)
						{
							if(EnableStats)
							{
								CProfile_Gun gun;
								if(stats.gun.Contains(weapons[i].name))
								{
									gun = (CProfile_Gun)stats.gun[weapons[i].name];
									gun.kills++;
									stats.gun[weapons[i].name] = gun;
								}
								else 
								{
									gun = new CProfile_Gun();
									gun.kills = 1;
									stats.gun.Add(weapons[i].name, gun);
								}
								NewStats = true;
							}
							NewSnaps = true;
						} 
						// Deaths
						else if(match.Groups[1].Value == alias && EnableStats)
						{
							CProfile_Gun gun;
							if(stats.gun.Contains(weapons[i].name))
							{
								gun = (CProfile_Gun)stats.gun[weapons[i].name];
								gun.killed++;
								stats.gun[weapons[i].name] = gun;
							}
							else 
							{
								gun = new CProfile_Gun();
								gun.killed = 1;
								stats.gun.Add(weapons[i].name, gun);
							}
							NewStats = true;
						}
						found = true;
					}
					if(found) break;
				}
				if(found) continue;

				if(EnableStats)
				{
					found = false;
					// Match weapon kills/deaths
					for(int i = 0; i < deaths.Length; i++)
					{
						match = deaths[i].r.Match(strLine);
						if(match.Success)
						{
							// Kills
							if(match.Groups[1].Value == alias)
							{
								uint count;
								if(stats.death.Contains(deaths[i].name))
								{
									count = (uint)stats.gun[deaths[i].name];
									count++;
									stats.death[deaths[i].name] = count;
								}
								else 
								{
									count = 1;
									stats.death.Add(deaths[i].name, count);
								}
								NewStats = true;
							} 
							found = true;
						}
						if(found) break;
					}
					if(found) continue;
				}
				match = rNickChange.Match(strLine);
				if(match.Success)
				{
					if(match.Groups[1].Value == alias)
					{
						alias = match.Groups[2].Value;
					}
					continue;
				}
			}
		}
		public override bool Open()
		{
			try 
			{
				this.alias = GetGameAlias(@"\baseq3\q3config.cfg");
				this.log = new StreamReader(new FileStream(this.path + @"\baseq3\qconsole.log", FileMode.Open,  FileAccess.Read, FileShare.ReadWrite));
				this.log.BaseStream.Seek(0,SeekOrigin.End);		// Set to End	
				return true;
			}
			catch 
			{
				return false;
			}
		}
		public override bool CheckActive()
		{
			return NativeMethods.FindWindow("Quake 3: Arena","Quake 3: Arena") != 0;
		}
		public override String GetDefaultPath()
		{
			const int LocalDisk = 3;

			frmSearch search = new frmSearch();
			try
			{
				
				search.Show();
				ManagementObjectSearcher query = new ManagementObjectSearcher("SELECT * FROM Win32_LogicalDisk WHERE DriveType=" + LocalDisk.ToString());
				ManagementObjectCollection queryCollection = query.Get();
				foreach( ManagementObject mo in queryCollection )
				{
					string result = search.Search(ProfileName, mo["Name"].ToString() + "\\", 
						new Regex(@"\\quake3.exe$", RegexOptions.IgnoreCase));
					search.Close();
					if(result != null)
						return Path.GetDirectoryName(result);	
				}
			}
			catch
			{
				search.Close();
				return null;
			}
			search.Close();
			return null;
		}
		// Get the game alias for said default user
		public String GetGameAlias(String file)
		{
			String strLine;
			Match match;
			String nick = null;
			StreamReader sr = null;
		
			try
			{
				sr = new StreamReader(this.path + file);
				//Continues to output one line at a time until end of file(EOF) is reached
				while ( (strLine = sr.ReadLine()) != null)
				{
					Regex rNick=new Regex("^seta name\\s+\"(.+)\"$");
					match = rNick.Match(strLine);
					if(match.Success)
					{
						nick = match.Groups[1].Value;
						break;
					}
				}
			}
			catch
			{
				nick = null;
			}
			finally 
			{
				// Cleanup
				if(sr != null) sr.Close();	
			}
			return rStripColor.Replace(nick, "");
		}
		public override void toXMLOperations()
		{
			// Turn gun hastable into something more useable
			if(stats.gun == null)
				return;
			stats.xmlgun = new CProfile_XMLGun [ stats.gun.Count ];
			int i = 0;
			foreach(String key in stats.gun.Keys)
			{
				stats.xmlgun[i] = new CProfile_XMLGun();
				stats.xmlgun[i].name = key;
				stats.xmlgun[i].stats = (CProfile_Gun)stats.gun[key];
				i++;
			}
			if(stats.death == null)
				return;
			stats.xmldeath = new CProfileQuakeIIIArena_XMLDeath [ stats.death.Count ];
			i = 0;
			foreach(String key in stats.death.Keys)
			{
				stats.xmldeath[i] = new CProfileQuakeIIIArena_XMLDeath();
				stats.xmldeath[i].name = key;
				stats.xmldeath[i].deaths = (uint)stats.gun[key];
				i++;
			}

		}
		public override void fromXMLOperations()
		{
			if(stats.xmlgun == null)
				return;

			if(stats.gun == null)
				stats.gun = new Hashtable();

			for(int i = 0; i < stats.xmlgun.Length; i++)
			{
				stats.gun.Add(stats.xmlgun[i].name, stats.xmlgun[i].stats);
			}

			if(stats.xmldeath == null)
				return;

			if(stats.death == null)
				stats.death = new Hashtable();

			for(int i = 0; i < stats.xmldeath.Length; i++)
			{
				stats.death.Add(stats.xmldeath[i].name, stats.xmldeath[i].deaths);
			}
		}
		public override void ResetStats()
		{
			stats = new CProfileQuakeIIIArena_Stats();
		}
		public override string GetStatsReport(String font, CProfile.SaveTypes format)
		{
			string strStats = null;
			CProfileQuakeIIIArena_Stats stats = this.stats;
			switch(format)
			{
				case CProfile.SaveTypes.HTML:
				{
					strStats = "<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.1//EN\" \"http://www.w3.org/TR/xhtml11/DTD/xhtml11.dtd\">\r\n";
					strStats += "<html xmlns=\"http://www.w3.org/1999/xhtml\" xml:lang=\"en\">\r\n";
					strStats += "\t<head>\r\n";
					strStats += "\t\t<meta http-equiv=\"Content-Type\" content=\"application/xhtml+xml; charset=iso-8859-1\" />\r\n";
					strStats += "\t\t<meta http-equiv=\"Content-Style-Type\" content=\"text/css\" />\r\n";
					strStats += "\t\t<meta http-equiv=\"Content-Language\" content=\"EN\" />\r\n";
					strStats += "\t\t<meta name=\"description\" content=\"Smile! v" + Info.version + " -- " + ProfileName + " Generator.\" />\r\n";
					strStats += "\t\t<meta name=\"copyright\" content=\"©2005 Marek Kudlacz -- Kudlacz.com\" />\r\n";
					strStats += "\t\t<title>Smile! v" + Info.version + " - " + ProfileName + " Statistics</title>\r\n";
					strStats += "\t</head>\r\n";
					strStats += "\t<body>\r\n";
					strStats += "\t\t<h2>" + ProfileName + " Statistics:</h2>\r\n";
					strStats += "\t\t<hr />\r\n";
					strStats += "\t\tCreated with Smile! v" + Info.version + "<br />\r\n";
					strStats += "\t\t©2005 Marek Kudlacz -- <a href=\"http://www.kudlacz.com\">http://www.kudlacz.com</a><br />\r\n";
					strStats += "\t\t<hr />\r\n";

					strStats += "\t\t<h3>Self-Inflicted/Unknown Death Statistics:</h3>\r\n";
					foreach(String Key in stats.death.Keys)
					{
						strStats += "\t\t<b>"+ Key + ":</b> " + (uint)stats.death[Key] + "<br />\r\n";
					}
					strStats += "\t\t<br /><br />\r\n";

					strStats += "\t\t<h3>Weapon Statistics:</h3>\r\n";
					uint TotalKills = 0;
					uint TotalKilled = 0;
					foreach(String Key in stats.gun.Keys)
					{
						TotalKills += ((CProfile_Gun)stats.gun[Key]).kills;
						TotalKilled += ((CProfile_Gun)stats.gun[Key]).killed;
						strStats += "\t\t<b>"+ Key + ":</b> kills: " + ((CProfile_Gun)stats.gun[Key]).kills + " deaths: " + ((CProfile_Gun)stats.gun[Key]).killed + "<br />\r\n";
					}
					strStats += "\t\tTotal Kills: " + TotalKills + " Total Deaths: " + TotalKilled + "<br />\r\n";
					strStats += "\t\t<br /><br />\r\n";
					strStats += "\t</body>\r\n";
					strStats += "</html>";
					break;
				}
				case CProfile.SaveTypes.RTF:
				case CProfile.SaveTypes.TXT:
				{
					RichTextBox c = new RichTextBox();

					try
					{
						// Populate the rich text box
						c.SelectionStart = 0 ;
						c.SelectionFont = new Font(font, 12, FontStyle.Bold);
						c.SelectedText = ProfileName + " Statistics:\n" ;
						c.SelectionFont = new Font(font, 12, FontStyle.Bold|FontStyle.Underline);
						c.SelectedText = "                                                                \n\n";
						c.SelectedText = "Created with Smile! v" + Info.version + "\n";
						c.SelectedText = "©2005 Marek Kudlacz -- http://www.kudlacz.com\n";
						c.SelectionFont = new Font(font, 12, FontStyle.Bold|FontStyle.Underline);
						c.SelectedText = "                                                                \n\n";

						c.SelectionFont = new Font(font, 10, FontStyle.Bold);
						c.SelectedText = "Self-Inflicted/Unknown Death Statistics:\n" ;
						foreach(String Key in stats.death.Keys)
						{
							c.SelectionFont = new Font(font, 9, FontStyle.Bold|FontStyle.Italic);
							c.SelectedText = Key + ": ";
							c.SelectionFont = new Font(font, 9, FontStyle.Regular);
							c.SelectedText = (uint)stats.death[Key] + "\n";
						}
						c.SelectedText = "\n\n";

						c.SelectionFont = new Font(font, 10, FontStyle.Bold);
						c.SelectedText = "Weapon Statistics:\n";
						uint TotalKills = 0;
						uint TotalKilled = 0;
						foreach(String Key in stats.gun.Keys)
						{
							TotalKills += ((CProfile_Gun)stats.gun[Key]).kills;
							TotalKilled += ((CProfile_Gun)stats.gun[Key]).killed;
							c.SelectionFont = new Font(font, 9, FontStyle.Bold|FontStyle.Italic);
							c.SelectedText = Key + ": ";
							c.SelectionFont = new Font(font, 9, FontStyle.Regular);
							c.SelectedText = " kills: " + ((CProfile_Gun)stats.gun[Key]).kills;
							c.SelectedText = " deaths: " + ((CProfile_Gun)stats.gun[Key]).killed + "\n";
						}
						c.SelectedText = "Total Kills: " + TotalKills + " Total Deaths: " + TotalKilled + "\n";
						c.SelectedText = "\n\n";

						c.SelectionStart = 0 ;
					}
					catch
					{
						c.Clear();
						c.SelectionStart = 0 ;
						c.SelectedText = "There was an error writing the stats, (missing font?) Try saving it to html instead using the edit menu.\n\n";
						c.SelectionStart = 0 ;
					}

					switch(format)
					{
						case CProfile.SaveTypes.RTF:
							strStats = c.Rtf;
							break;
						case CProfile.SaveTypes.TXT:
							strStats = c.Text;
							break;
					}
					c.Dispose();
					break;
				}
			}
			return strStats;
		}
	}

	//////////////////////////////////////////////////////////////////////////////////////////////
	/// Quake III Team Arena
	//////////////////////////////////////////////////////////////////////////////////////////////

	public class CProfileQuakeIIITeamArena : CProfileQuakeIIIArena
	{
		public CProfileQuakeIIITeamArena()
		{
			this.ProfileName = "Quake III Team Arena";
			this.SnapName = "Quake III Team Arena";
		}
		public override bool Open()
		{
			try 
			{
				this.alias = GetGameAlias(@"\missionpack\q3config.cfg");
				this.log = new StreamReader(new FileStream(this.path + @"\missionpack\qconsole.log", FileMode.Open,  FileAccess.Read, FileShare.ReadWrite));
				this.log.BaseStream.Seek(0,SeekOrigin.End);		// Set to End	
				return true;
			}
			catch 
			{
				return false;
			}
		}
	}

	//////////////////////////////////////////////////////////////////////////////////////////////
	/// Q3: OSP
	//////////////////////////////////////////////////////////////////////////////////////////////

	public class CProfileQ3OSP : CProfileQuakeIIIArena
	{
		public CProfileQ3OSP()
		{
			this.ProfileName = "Quake III OSP";
			this.SnapName = "Quake III OSP";
		}
		public override bool Open()
		{
			try 
			{
				this.alias = GetGameAlias(@"\osp\q3config.cfg");
				this.log = new StreamReader(new FileStream(this.path + @"\osp\qconsole.log", FileMode.Open,  FileAccess.Read, FileShare.ReadWrite));
				this.log.BaseStream.Seek(0,SeekOrigin.End);		// Set to End	
				return true;
			}
			catch 
			{
				return false;
			}
		}
	}

	//////////////////////////////////////////////////////////////////////////////////////////////
	/// Enemy Territory
	//////////////////////////////////////////////////////////////////////////////////////////////

	public class CProfileEnemyTerritory_MOD
	{
		public string name;
		public string mod;
		public Regex r;
		public CProfileEnemyTerritory_MOD(string name, string mod)
		{
			this.name = name;
			this.mod = mod;
			r = new Regex (mod);
		}
	}
	public class CProfileEnemyTerritory_XMLDeath
	{
		public String name;
		public UInt32 deaths;
	
		public CProfileEnemyTerritory_XMLDeath()
		{
			deaths = new UInt32();
		}
	}
	public class CProfileEnemyTerritory_Stats
	{
		public UInt32 hits_team;
		[XmlIgnoreAttribute] public Hashtable gun;
		[XmlIgnoreAttribute] public Hashtable death;
		[XmlElement(ElementName = "gun")]public CProfile_XMLGun [] xmlgun;
		[XmlElement(ElementName = "death")]public CProfileEnemyTerritory_XMLDeath [] xmldeath;

		public CProfileEnemyTerritory_Stats()
		{
			death = new Hashtable();
			gun = new Hashtable();
		}
	}
	public class CProfileEnemyTerritory : CProfile
	{
		public CProfileEnemyTerritory_Stats stats;

		protected String alias;
		private CProfileEnemyTerritory_MOD [] weapons;
		private CProfileEnemyTerritory_MOD [] deaths;
		private Regex rNickChange;
		private Regex rStripColor;

		public CProfileEnemyTerritory()
		{
			this.ProfileName = "Enemy Territory";
			this.SnapName = "Enemy Territory";
			this.stats = new CProfileEnemyTerritory_Stats();
			this.deaths = new CProfileEnemyTerritory_MOD [] 
				{
					new CProfileEnemyTerritory_MOD("Suicide", "^-> (.+) commited suicide$"),
					new CProfileEnemyTerritory_MOD("Falling", "^-> (.+) fell to his death$"),
					new CProfileEnemyTerritory_MOD("Crush", "^-> (.+) was crushed$"),
					new CProfileEnemyTerritory_MOD("Crush Rubble", "^-> (.+) got buried under a pile of rubble$"),
					new CProfileEnemyTerritory_MOD("Water", "^-> (.+) drowned$"),
					new CProfileEnemyTerritory_MOD("Slime", "^-> (.+) died by toxic materials$"),
					new CProfileEnemyTerritory_MOD("Lava", "^-> (.+) was incinerated$"),
					new CProfileEnemyTerritory_MOD("Dynamite", "^-> (.+) dynamited himself to pieces$"),
					new CProfileEnemyTerritory_MOD("Grenade", "^-> (.+) dove on his own grenade$"),
					new CProfileEnemyTerritory_MOD("Panzerfaust", "^-> (.+) vaporized himself$"),
					new CProfileEnemyTerritory_MOD("Flamethrower", "^-> (.+) played with fire$"),
					new CProfileEnemyTerritory_MOD("Airstrike", "^-> (.+) obliterated himself$"),
					new CProfileEnemyTerritory_MOD("Artillery", "^-> (.+) fired-for-effect on himself$"),
					new CProfileEnemyTerritory_MOD("Explosive","^-> (.+) died in his own explosion$"),
					new CProfileEnemyTerritory_MOD("Rifle Grenade","^-> (.+) ate his own rifle grenade$"),
					new CProfileEnemyTerritory_MOD("Landmine","^-> (.+) failed to spot his own landmine$"),
					new CProfileEnemyTerritory_MOD("Satchel","^-> (.+) embraced his own satchel explosion$"),
					new CProfileEnemyTerritory_MOD("Tripmine","^-> (.+) forgot where his tripmine was$"),
					new CProfileEnemyTerritory_MOD("Construction Caught","^-> (.+) engineered himself into oblivion$"),
					new CProfileEnemyTerritory_MOD("Construction Bury","^-> (.+) buried himself alive$"),
					new CProfileEnemyTerritory_MOD("Mortar","^-> (.+) never saw his own mortar round coming$"),
					new CProfileEnemyTerritory_MOD("Smoke Grenade","^-> (.+) danced on his airstrike marker$"),
					new CProfileEnemyTerritory_MOD("Other","^-> (.+) killed himself$"),
					new CProfileEnemyTerritory_MOD("Unknown","^-> (.+) died\\.$")
				};
			this.weapons = new CProfileEnemyTerritory_MOD [] 
				{ 
					new CProfileEnemyTerritory_MOD("Knife", "^-> (.+) was stabbed by(.+)'s knife$"),
					new CProfileEnemyTerritory_MOD("Akimbo Colt", "^-> (.+) was killed by (.+)'s Akimbo .45ACP 1911s$"),
					new CProfileEnemyTerritory_MOD("Akimbo Lugar", "^-> (.+) was killed by (.+)'s Akimbo Lugar 9mms$"),
					new CProfileEnemyTerritory_MOD("Lugar", "^-> (.+) was killed by (.+)'s Lugar 9mm$"),
					new CProfileEnemyTerritory_MOD("Colt", "^-> (.+) was killed by (.+)'s .45ACP 1911$"),
					new CProfileEnemyTerritory_MOD("MP40", "^-> (.+) was killed by (.+)'s MP40$"),
					new CProfileEnemyTerritory_MOD("Thompson", "^-> (.+) was killed by (.+)'s Thompson$"),
					new CProfileEnemyTerritory_MOD("Sten", "^-> (.+) was killed by (.+)'s Sten$"),
					new CProfileEnemyTerritory_MOD("Dynamite", "^-> (.+) was killed by (.+)'s dynamite$"),
					new CProfileEnemyTerritory_MOD("Panzerfaust", "^-> (.+) was killed by (.+)'s Panzerfaust$"),
					new CProfileEnemyTerritory_MOD("Grenade", "^-> (.+) was exploded by (.+)'s grenade$"),
					new CProfileEnemyTerritory_MOD("Flamethrower", "^-> (.+) was cooked by (.+)'s flamethrower$"),
					new CProfileEnemyTerritory_MOD("Mortar", "^-> (.+) never saw (.+)'s mortar round coming$"),
					new CProfileEnemyTerritory_MOD("Machinegun", "^-> (.+) was perforated by (.+)'s crew-served MG$"),
					new CProfileEnemyTerritory_MOD("Browning", "^-> (.+) was perforated by (.+)'s tank-mounted browning 30cal$"),
					new CProfileEnemyTerritory_MOD("MG42", "^-> (.+) was killed by (.+)'s tank-mounted MG42$"),
					new CProfileEnemyTerritory_MOD("Airstrike", "^-> (.+) was blasted by (.+)'s support fire$"),
					new CProfileEnemyTerritory_MOD("Artillery", "^-> (.+) was shelled by (.+)'s artillery support$"),
					new CProfileEnemyTerritory_MOD("Swapped Places", "^-> (.+) swapped places with (.+)$"),
					new CProfileEnemyTerritory_MOD("K43", "^-> (.+) was killed by (.+)'s K43$"),
					new CProfileEnemyTerritory_MOD("Garand", "^-> (.+) was killed by (.+)'s Garand$"),
					new CProfileEnemyTerritory_MOD("M7", "^-> (.+) was killed by (.+)'s rifle grenade$"),
					new CProfileEnemyTerritory_MOD("Landmine", "^-> (.+) was killed by (.+)'s Landmine$"),
					new CProfileEnemyTerritory_MOD("Construction Caught", "^-> (.+) got caught in (.+)'s construction madness$"),
					new CProfileEnemyTerritory_MOD("Construction Bury", "^-> (.+) got burried under (.+)'s rubble$"),
					new CProfileEnemyTerritory_MOD("Mobile MG42", "^-> (.+) was mown down by (.+)'s Mobile MG42$"),
					new CProfileEnemyTerritory_MOD("Garand Scope", "^-> (.+) was silenced by (.+)'s Garand$"),
					new CProfileEnemyTerritory_MOD("K43 Scope", "^-> (.+) was silenced by (.+)'s K43$"),
					new CProfileEnemyTerritory_MOD("FG42", "^-> (.+) was killed by (.+)'s FG42$"),
					new CProfileEnemyTerritory_MOD("FG42 Scope", "^-> (.+) was sniped by (.+)'s FG42$"),
					new CProfileEnemyTerritory_MOD("Satchel", "^-> (.+) was blasted by (.+)'s Satchel Charge$"),
					new CProfileEnemyTerritory_MOD("Tripmine", "^-> (.+) was detonated by (.+)'s trip mine$"),
					new CProfileEnemyTerritory_MOD("Smoke Grenade", "^-> (.+) stood on (.+)'s airstrike marker$"),
					new CProfileEnemyTerritory_MOD("Other", "^-> (.+) was killed by (.+)$")
				};
			this.rNickChange = new Regex("^-> (.+) renamed to (.+)$");
			this.rStripColor = new Regex("\\^\\d");
		}

		public override void Parse()
		{
			if(this.log == null)
				return;

			Match match;
			String strLine;
			bool found;

			while ((strLine = this.log.ReadLine()) != null)
			{
				strLine = rStripColor.Replace(strLine, "");

				// Do deaths first so it doesnt conflict with kill/killed messages
				if(EnableStats)
				{
					found = false;
					// Match deaths
					for(int i = 0; i < deaths.Length; i++)
					{
						match = deaths[i].r.Match(strLine);
						if(match.Success)
						{
							// Kills
							if(match.Groups[1].Value == alias)
							{
								uint count;
								if(stats.death.Contains(deaths[i].name))
								{
									count = (uint)stats.gun[deaths[i].name];
									count++;
									stats.death[deaths[i].name] = count;
								}
								else 
								{
									count = 1;
									stats.death.Add(deaths[i].name, count);
								}
								NewStats = true;
							} 
							found = true;
						}
						if(found) break;
					}
					if(found) continue;
				}

				// Match weapon kills/deaths
				found = false;
				for(int i = 0; i < weapons.Length; i++)
				{
					match = weapons[i].r.Match(strLine);
					if(match.Success)
					{
						// Kills
						if(match.Groups[2].Value == alias)
						{
							if(EnableStats)
							{
								CProfile_Gun gun;
								if(stats.gun.Contains(weapons[i].name))
								{
									gun = (CProfile_Gun)stats.gun[weapons[i].name];
									gun.kills++;
									stats.gun[weapons[i].name] = gun;
								}
								else 
								{
									gun = new CProfile_Gun();
									gun.kills = 1;
									stats.gun.Add(weapons[i].name, gun);
								}
								NewStats = true;
							}
							NewSnaps = true;
						} 
							// Deaths
						else if(match.Groups[1].Value == alias && EnableStats)
						{
							CProfile_Gun gun;
							if(stats.gun.Contains(weapons[i].name))
							{
								gun = (CProfile_Gun)stats.gun[weapons[i].name];
								gun.killed++;
								stats.gun[weapons[i].name] = gun;
							}
							else 
							{
								gun = new CProfile_Gun();
								gun.killed = 1;
								stats.gun.Add(weapons[i].name, gun);
							}
							NewStats = true;
						}
						found = true;
					}
					if(found) break;
				}
				if(found) continue;

				match = rNickChange.Match(strLine);
				if(match.Success)
				{
					if(match.Groups[1].Value == alias)
					{
						alias = match.Groups[2].Value;
					}
					continue;
				}
			}
		}
		public override bool Open()
		{
			try 
			{
				this.alias = GetGameAlias(@"\etmain");
				this.log = new StreamReader(new FileStream(this.path + @"\etmain\etconsole.log", FileMode.Open,  FileAccess.Read, FileShare.ReadWrite));
				this.log.BaseStream.Seek(0,SeekOrigin.End);		// Set to End	
				return true;
			}
			catch 
			{
				return false;
			}
		}
		public override bool CheckActive()
		{
			return NativeMethods.FindWindow("Enemy Territory","Enemy Territory") != 0;
		}
		public override String GetDefaultPath()
		{
			const int LocalDisk = 3;

			frmSearch search = new frmSearch();
			try
			{
				
				search.Show();
				ManagementObjectSearcher query = new ManagementObjectSearcher("SELECT * FROM Win32_LogicalDisk WHERE DriveType=" + LocalDisk.ToString());
				ManagementObjectCollection queryCollection = query.Get();
				foreach( ManagementObject mo in queryCollection )
				{
					string result = search.Search(ProfileName, mo["Name"].ToString() + "\\", 
						new Regex(@"\\et.exe$", RegexOptions.IgnoreCase));
					search.Close();
					if(result != null)
						return Path.GetDirectoryName(result);	
				}
			}
			catch
			{
				search.Close();
				return null;
			}
			search.Close();
			return null;
		}
		// Get the game alias for said default user
		public String GetGameAlias(String dir)
		{
			String strLine;
			Match match;
			String nick = null;
			StreamReader sr = null;
	
			try
			{
				// Find player by profile's last access time
				string [] direntries = Directory.GetDirectories(this.path + dir + "\\profiles");
				long [] accesstimes = new long [ direntries.Length ];
				long accesstime;
				string [] fileentries;
				string profile = null;
				for(int i = 0; i< direntries.Length; i++)
				{
					fileentries = Directory.GetFiles(direntries[i]);
					foreach(string fileentry in fileentries)
					{
						accesstime = File.GetLastAccessTime(fileentry).Ticks;
						if(accesstimes[i] < accesstime)
							accesstimes[i] = accesstime;
					}
				}
				long atime = 0;
				for(int i = 0; i < direntries.Length; i++)
				{
					if(accesstimes[i] > atime)
					{
						atime = accesstimes[i];
						profile = direntries[i];
					}
				}
			
				sr = new StreamReader(profile + "\\etconfig.cfg");
				while ( (strLine = sr.ReadLine()) != null)
				{
					Regex rNick=new Regex("^seta name\\s+\"(.+)\"$");
					match = rNick.Match(strLine);
					if(match.Success)
					{
						nick = match.Groups[1].Value;
						break;
					}
				}
			}
			catch
			{
				nick = null;
			}
			finally 
			{
				// Cleanup
				if(sr != null) sr.Close();	
			}
			return rStripColor.Replace(nick, "");
		}
		public override void toXMLOperations()
		{
			// Turn gun hastable into something more useable
			if(stats.gun == null)
				return;
			stats.xmlgun = new CProfile_XMLGun [ stats.gun.Count ];
			int i = 0;
			foreach(String key in stats.gun.Keys)
			{
				stats.xmlgun[i] = new CProfile_XMLGun();
				stats.xmlgun[i].name = key;
				stats.xmlgun[i].stats = (CProfile_Gun)stats.gun[key];
				i++;
			}
			if(stats.death == null)
				return;
			stats.xmldeath = new CProfileEnemyTerritory_XMLDeath [ stats.death.Count ];
			i = 0;
			foreach(String key in stats.death.Keys)
			{
				stats.xmldeath[i] = new CProfileEnemyTerritory_XMLDeath();
				stats.xmldeath[i].name = key;
				stats.xmldeath[i].deaths = (uint)stats.gun[key];
				i++;
			}

		}
		public override void fromXMLOperations()
		{
			if(stats.xmlgun == null)
				return;

			if(stats.gun == null)
				stats.gun = new Hashtable();

			for(int i = 0; i < stats.xmlgun.Length; i++)
			{
				stats.gun.Add(stats.xmlgun[i].name, stats.xmlgun[i].stats);
			}

			if(stats.xmldeath == null)
				return;

			if(stats.death == null)
				stats.death = new Hashtable();

			for(int i = 0; i < stats.xmldeath.Length; i++)
			{
				stats.death.Add(stats.xmldeath[i].name, stats.xmldeath[i].deaths);
			}
		}
		public override void ResetStats()
		{
			stats = new CProfileEnemyTerritory_Stats();
		}
		public override string GetStatsReport(String font, CProfile.SaveTypes format)
		{
			string strStats = null;
			CProfileEnemyTerritory_Stats stats = this.stats;
			switch(format)
			{
				case CProfile.SaveTypes.HTML:
				{
					strStats = "<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.1//EN\" \"http://www.w3.org/TR/xhtml11/DTD/xhtml11.dtd\">\r\n";
					strStats += "<html xmlns=\"http://www.w3.org/1999/xhtml\" xml:lang=\"en\">\r\n";
					strStats += "\t<head>\r\n";
					strStats += "\t\t<meta http-equiv=\"Content-Type\" content=\"application/xhtml+xml; charset=iso-8859-1\" />\r\n";
					strStats += "\t\t<meta http-equiv=\"Content-Style-Type\" content=\"text/css\" />\r\n";
					strStats += "\t\t<meta http-equiv=\"Content-Language\" content=\"EN\" />\r\n";
					strStats += "\t\t<meta name=\"description\" content=\"Smile! v" + Info.version + " -- " + ProfileName + " Generator.\" />\r\n";
					strStats += "\t\t<meta name=\"copyright\" content=\"©2005 Marek Kudlacz -- Kudlacz.com\" />\r\n";
					strStats += "\t\t<title>Smile! v" + Info.version + " - " + ProfileName + " Statistics</title>\r\n";
					strStats += "\t</head>\r\n";
					strStats += "\t<body>\r\n";
					strStats += "\t\t<h2>" + ProfileName + " Statistics:</h2>\r\n";
					strStats += "\t\t<hr />\r\n";
					strStats += "\t\tCreated with Smile! v" + Info.version + "<br />\r\n";
					strStats += "\t\t©2005 Marek Kudlacz -- <a href=\"http://www.kudlacz.com\">http://www.kudlacz.com</a><br />\r\n";
					strStats += "\t\t<hr />\r\n";

					strStats += "\t\t<h3>Self-Inflicted Statistics:</h3>\r\n";
					foreach(String Key in stats.death.Keys)
					{
						strStats += "\t\t<b>"+ Key + ":</b> " + (uint)stats.death[Key] + "<br />\r\n";
					}
					strStats += "\t\t<br /><br />\r\n";

					strStats += "\t\t<h3>Self-Inflicted/Unknown Death Statistics:</h3>\r\n";
					foreach(String Key in stats.death.Keys)
					{
						strStats += "\t\t<b>"+ Key + ":</b> " + (uint)stats.death[Key] + "<br />\r\n";
					}
					strStats += "\t\t<br /><br />\r\n";

					strStats += "\t\t<h3>Weapon Statistics:</h3>\r\n";
					uint TotalKills = 0;
					uint TotalKilled = 0;
					foreach(String Key in stats.gun.Keys)
					{
						TotalKills += ((CProfile_Gun)stats.gun[Key]).kills;
						TotalKilled += ((CProfile_Gun)stats.gun[Key]).killed;
						strStats += "\t\t<b>"+ Key + ":</b> kills: " + ((CProfile_Gun)stats.gun[Key]).kills + " deaths: " + ((CProfile_Gun)stats.gun[Key]).killed + "<br />\r\n";
					}
					strStats += "\t\tTotal Kills: " + TotalKills + " Total Deaths: " + TotalKilled + "<br />\r\n";
					strStats += "\t\t<br /><br />\r\n";
					strStats += "\t</body>\r\n";
					strStats += "</html>";
					break;
				}
				case CProfile.SaveTypes.RTF:
				case CProfile.SaveTypes.TXT:
				{
					RichTextBox c = new RichTextBox();

					try
					{
						// Populate the rich text box
						c.SelectionStart = 0 ;
						c.SelectionFont = new Font(font, 12, FontStyle.Bold);
						c.SelectedText = ProfileName + " Statistics:\n" ;
						c.SelectionFont = new Font(font, 12, FontStyle.Bold|FontStyle.Underline);
						c.SelectedText = "                                                                \n\n";
						c.SelectedText = "Created with Smile! v" + Info.version + "\n";
						c.SelectedText = "©2005 Marek Kudlacz -- http://www.kudlacz.com\n";
						c.SelectionFont = new Font(font, 12, FontStyle.Bold|FontStyle.Underline);
						c.SelectedText = "                                                                \n\n";

						c.SelectionFont = new Font(font, 10, FontStyle.Bold);
						c.SelectedText = "Self-Inflicted/Unknown Death Statistics:\n" ;
						uint TotalKills = 0;
						uint TotalKilled = 0;
						foreach(String Key in stats.death.Keys)
						{
							TotalKills += ((CProfile_Gun)stats.gun[Key]).kills;
							TotalKilled += ((CProfile_Gun)stats.gun[Key]).killed;
							c.SelectionFont = new Font(font, 9, FontStyle.Bold|FontStyle.Italic);
							c.SelectedText = Key + ": ";
							c.SelectionFont = new Font(font, 9, FontStyle.Regular);
							c.SelectedText = (uint)stats.death[Key] + "\n";
						}
						c.SelectedText = "Total Kills: " + TotalKills + " Total Deaths: " + TotalKilled + "\n";
						c.SelectedText = "\n\n";

						c.SelectionFont = new Font(font, 10, FontStyle.Bold);
						c.SelectedText = "Weapon Statistics:\n" ;
						foreach(String Key in stats.gun.Keys)
						{
							c.SelectionFont = new Font(font, 9, FontStyle.Bold|FontStyle.Italic);
							c.SelectedText = Key + ": ";
							c.SelectionFont = new Font(font, 9, FontStyle.Regular);
							c.SelectedText = " kills: " + ((CProfile_Gun)stats.gun[Key]).kills;
							c.SelectedText = " deaths: " + ((CProfile_Gun)stats.gun[Key]).killed + "\n";
						}
						c.SelectedText = "\n\n";

						c.SelectionStart = 0 ;
					}
					catch
					{
						c.Clear();
						c.SelectionStart = 0 ;
						c.SelectedText = "There was an error writing the stats, (missing font?) Try saving it to html instead using the edit menu.\n\n";
						c.SelectionStart = 0 ;
					}

					switch(format)
					{
						case CProfile.SaveTypes.RTF:
							strStats = c.Rtf;
							break;
						case CProfile.SaveTypes.TXT:
							strStats = c.Text;
							break;
					}
					c.Dispose();
					break;
				}
			}
			return strStats;
		}
	}



	//////////////////////////////////////////////////////////////////////////////////////////////
	/// Day of Defeat: Source
	//////////////////////////////////////////////////////////////////////////////////////////////
	public class CProfileDayofDefeatSource_XMLObjective
	{
		public String name;
		public CProfileDayofDefeatSource_Objective stats;
		
		public CProfileDayofDefeatSource_XMLObjective()
		{
			stats = new CProfileDayofDefeatSource_Objective();
		}
	}
	public class  CProfileDayofDefeatSource_Objective
	{
		public UInt32 captures;

		public  CProfileDayofDefeatSource_Objective()
		{
			this.captures = new UInt32();	
		}
	}
	public class CProfileDayofDefeatSource_Stats
	{
		public UInt32 damage_given;
		public UInt32 damage_received;
		public UInt32 hits_received;
		public UInt32 hits_given;
		public UInt32 received_counts;
		public UInt32 given_counts;
		public UInt32 deaths;		// misc deaths, does not include being killed by weapon
		public UInt32 suicides;
		[XmlIgnoreAttribute] public Hashtable gun;
		[XmlElement(ElementName = "gun")]public CProfile_XMLGun [] xmlgun;
		[XmlIgnoreAttribute] public Hashtable objective;
		[XmlElement(ElementName = "objective")]public CProfileDayofDefeatSource_XMLObjective [] xmlobjective;
		public CProfileDayofDefeatSource_Stats()
		{
			damage_given = new UInt32();
			damage_received = new UInt32();
			hits_received = new UInt32();
			hits_given = new UInt32();
			received_counts = new UInt32();
			given_counts = new UInt32();
			deaths = new UInt32();
			suicides = new UInt32();
			gun = new Hashtable();
			objective = new Hashtable();
		}
	}

	public class CProfileDayofDefeatSource : CProfile
	{
		public CProfileDayofDefeatSource_Stats stats;

		private String alias;
		private Regex rKill;
		private Regex rKilled;
		private Regex rNickChange;
		private Regex rDamage;
		private Regex rSuicide;
		private Regex rCapture;
		private Regex rDeath;

		public CProfileDayofDefeatSource()
		{
			this.ProfileName = "Day of Defeat: Source";
			this.SnapName = "Day of Defeat Source";
			this.stats = new CProfileDayofDefeatSource_Stats();
		}

		private void PopulateRegEx()
		{
			rKill = new Regex("^" + Regex.Escape(this.alias) + " killed .+ with (\\w+).$");
			rKilled = new Regex("^.+ killed " + Regex.Escape(this.alias) + " with (\\w+).$");
			rNickChange = new Regex("^" + Regex.Escape(this.alias) + " is now known as (.+)$");
			rDamage = new Regex("^Damage (\\w+ \\w+) \".+\" - (\\d+) in (\\d+) hits?$");
			rCapture = new Regex("^" + Regex.Escape(this.alias) + " captured .+ for the (.+)$");
			rSuicide = new Regex("^" + Regex.Escape(this.alias) + " suicided.$");
			rDeath =  new Regex("^" + Regex.Escape(this.alias) + " died.$");
		}

		public override void Parse()
		{
			if(this.log == null)
				return;

			Match match;
			String strLine;

			while ((strLine = this.log.ReadLine()) != null)
			{
				// Match kills given
				if(EnableSnaps || EnableStats)
				{
					match = rKill.Match(strLine);
					if(match.Success) 
					{
						if(EnableStats)
						{
							String strGun = match.Groups[1].Value;
							CProfile_Gun gun;
							if(stats.gun.Contains(strGun))
							{
								gun = (CProfile_Gun)stats.gun[strGun];
								gun.kills++;
								stats.gun[strGun] = gun;
							}
							else 
							{
								gun = new CProfile_Gun();
								gun.kills = 1;
								stats.gun.Add(strGun, gun);
							}
						}
						NewSnaps = true;
						NewStats = true;
						continue;
					}
				}

				if(EnableStats)
				{
					// Match times self has been killed
					match = rKilled.Match(strLine);
					if(match.Success)
					{
						String strGun = match.Groups[1].Value;
						CProfile_Gun gun;
						if(stats.gun.Contains(strGun)) 
						{
							gun = (CProfile_Gun)stats.gun[strGun];
							gun.killed++;
							stats.gun[strGun] = gun;
						}
						else  
						{
							gun = new CProfile_Gun();
							gun.killed = 1;
							stats.gun.Add(strGun, gun);
						}
						NewStats = true;
						continue;
					}

					// Match nickchange of self
					match = rNickChange.Match(strLine);
					if(match.Success)
					{
						this.alias = match.Groups[1].Value;
						PopulateRegEx();
						NewStats = true;
						continue;
					}

					// Match Damage given/received
					match = rDamage.Match(strLine);
					if(match.Success)
					{
						if(match.Groups[1].Value == "Given to")
						{
							stats.damage_given += Convert.ToUInt32(match.Groups[2].Value);
							stats.hits_given += Convert.ToUInt32(match.Groups[3].Value);
							stats.given_counts++;
						}
						else
						{
							stats.damage_received += Convert.ToUInt32(match.Groups[2].Value);
							stats.hits_received += Convert.ToUInt32(match.Groups[3].Value);
							stats.received_counts++;
						}
						NewStats = true;
						continue;
					}

					// Match Captures
					match = rCapture.Match(strLine);
					if(match.Success)
					{
						String strTeam = match.Groups[2].Value;
						CProfileDayofDefeatSource_Objective objective;

						if(stats.objective.Contains(strTeam))
						{
							objective = (CProfileDayofDefeatSource_Objective)stats.objective[strTeam];
							objective.captures++;
							stats.objective[strTeam] = objective;
						}
						else 
						{
							objective = new CProfileDayofDefeatSource_Objective();
							objective.captures = 1;
							stats.objective.Add(strTeam, objective);
						}
						NewStats = true;
						continue;
					}

					// Match Suicide
					match = rSuicide.Match(strLine);
					if(match.Success)
					{
						stats.suicides++;
						NewStats = true;
						continue;
					}

					// Match Misc Death
					match = rDeath.Match(strLine);
					if(match.Success)
					{
						stats.deaths++;
						NewStats = true;
						continue;
					}
				}
			}
		}
		public override bool Open()
		{
			try 
			{
				this.alias = GetGameAlias();
				this.log = new StreamReader(new FileStream(this.path + @"\console.log", FileMode.Open,  FileAccess.Read, FileShare.ReadWrite));
				this.log.BaseStream.Seek(0,SeekOrigin.End);		// Set to End
				PopulateRegEx();								// Create our Regex objects
				return true;
			}
			catch (Exception e)
			{
				frmMain.error += "|||" + e.Message;
				return false;
			}
		}
		public override bool CheckActive()
		{
			return NativeMethods.FindWindow("Valve001","Day of Defeat Source") != 0;
		}
		public override String GetDefaultPath()
		{
			try
			{
				RegistryKey Key = Registry.CurrentUser;
				Key = Key.OpenSubKey(@"Software\Valve\Steam", false);
				frmSearch search = new frmSearch();
				search.Show();
				string result = search.Search(ProfileName, Path.GetDirectoryName(Key.GetValue("SteamExe").ToString()) + @"\SteamApps", 
					new Regex(@"day of defeat source\\dod$", RegexOptions.IgnoreCase));
				search.Close();
				if(result != null)
					return result;
				
			}
			catch(Exception e)
			{
				Ex.DumpException(e);
				return null;
			}
			return null;
		}
		// Get the game alias for said default user
		public String GetGameAlias()
		{
			String strLine;
			Match match;
			String nick = null;
			StreamReader sr = null;
			
			try
			{
				sr = new StreamReader(this.path + @"\cfg\config.cfg");
				//Continues to output one line at a time until end of file(EOF) is reached
				while ( (strLine = sr.ReadLine()) != null)
				{
					Regex rNick=new Regex("^name\\s+\"(.+)\"$");
					match = rNick.Match(strLine);
					if(match.Success)
					{
						nick = match.Groups[1].Value;
						break;
					}
				}
			}
			catch
			{
				nick = null;
			}
			finally 
			{
				// Cleanup
				if(sr != null) sr.Close();	
			}
			return nick;
		}
		public override void toXMLOperations()
		{
			// Turn gun hastable into something more useable
			if(stats.gun == null || stats.objective == null)
				return;
			stats.xmlgun = new CProfile_XMLGun [ stats.gun.Count ];
			int i = 0;
			foreach(String key in stats.gun.Keys)
			{
				stats.xmlgun[i] = new CProfile_XMLGun();
				stats.xmlgun[i].name = key;
				stats.xmlgun[i].stats = (CProfile_Gun)stats.gun[key];
				i++;
			}

			stats.xmlobjective = new CProfileDayofDefeatSource_XMLObjective [ stats.objective.Count ];
			i = 0;
			foreach(String key in stats.objective.Keys)
			{
				stats.xmlobjective[i] = new CProfileDayofDefeatSource_XMLObjective();
				stats.xmlobjective[i].name = key;
				stats.xmlobjective[i].stats = (CProfileDayofDefeatSource_Objective)stats.objective[key];
				i++;
			}

		}
		public override void fromXMLOperations()
		{
			if(stats.xmlgun == null || stats.xmlobjective == null)
				return;

			if(stats.gun == null)
				stats.gun = new Hashtable();

			if(stats.objective == null)
				stats.objective = new Hashtable();

			for(int i = 0; i < stats.xmlgun.Length; i++)
			{
				stats.gun.Add(stats.xmlgun[i].name, stats.xmlgun[i].stats);
			}

			for(int i = 0; i < stats.xmlobjective.Length; i++)
			{
				stats.objective.Add(stats.xmlobjective[i].name, stats.xmlobjective[i].stats);
			}
		}
		public override void ResetStats()
		{
			stats = new CProfileDayofDefeatSource_Stats();
		}
		public override string GetStatsReport(String font, CProfile.SaveTypes format)
		{
			string strStats = null;
			CProfileDayofDefeatSource_Stats stats = (CProfileDayofDefeatSource_Stats)this.stats;
			switch(format)
			{
				case CProfile.SaveTypes.HTML:
				{
					strStats = "<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.1//EN\" \"http://www.w3.org/TR/xhtml11/DTD/xhtml11.dtd\">\r\n";
					strStats += "<html xmlns=\"http://www.w3.org/1999/xhtml\" xml:lang=\"en\">\r\n";
					strStats += "\t<head>\r\n";
					strStats += "\t\t<meta http-equiv=\"Content-Type\" content=\"application/xhtml+xml; charset=iso-8859-1\" />\r\n";
					strStats += "\t\t<meta http-equiv=\"Content-Style-Type\" content=\"text/css\" />\r\n";
					strStats += "\t\t<meta http-equiv=\"Content-Language\" content=\"EN\" />\r\n";
					strStats += "\t\t<meta name=\"description\" content=\"Smile! v" + Info.version + " -- " + ProfileName + " Generator.\" />\r\n";
					strStats += "\t\t<meta name=\"copyright\" content=\"©2005 Marek Kudlacz -- Kudlacz.com\" />\r\n";
					strStats += "\t\t<title>Smile! v" + Info.version + " - " + ProfileName + " Statistics</title>\r\n";
					strStats += "\t</head>\r\n";
					strStats += "\t<body>\r\n";
					strStats += "\t\t<h2>" + ProfileName + " Statistics:</h2>\r\n";
					strStats += "\t\t<hr />\r\n";
					strStats += "\t\tCreated with Smile! v" + Info.version + "<br />\r\n";
					strStats += "\t\t©2005 Marek Kudlacz -- <a href=\"http://www.kudlacz.com\">http://www.kudlacz.com</a><br />\r\n";
					strStats += "\t\t<hr />\r\n";

					strStats += "\t\t<h3>Enagement Information</h3>\r\n";
					strStats += "\t\tYou've engaged others " + stats.given_counts + " times.<br />\r\n";
					strStats += "\t\tYou've been engaged " + stats.received_counts + " times.<br /><br />\r\n";

					strStats += "\t\t<h3>Hit Information:</h3>\r\n";
					strStats += "\t\tHits given: " + stats.hits_given + "<br />\r\n";
					strStats += "\t\tAverage hits given per engagement: " + (float)stats.hits_given/stats.given_counts + "<br />\r\n";
					strStats += "\t\tHits received: " + stats.hits_received + "<br />\r\n";
					strStats += "\t\tAverage hits received per engagement: " + (float)stats.hits_received/stats.received_counts + "<br /><br />\r\n";

					strStats += "\t\t<h3>Damage Information:</h3>\r\n";
					strStats += "\t\tDamage given: " + stats.damage_given + "<br />\r\n";
					strStats += "\t\tAverage damage given per engagement: " + (float)stats.damage_given/stats.given_counts + "<br />\r\n";
					strStats += "\t\tDamage received: " + stats.damage_received + "<br />\r\n";
					strStats += "\t\tAverage damage received per engagement: " + (float)stats.damage_received/stats.received_counts + "<br /><br />\r\n";

					strStats += "\t\t<h3>Misc Statistics:</h3>\r\n";
					strStats += "\t\tMiscellaneous Deaths: " + stats.deaths + "<br />\r\n";
					strStats += "\t\tSuicides: " + stats.suicides + "<br /><br />\r\n";

					strStats += "\t\t<h3>Capture Statistics:</h3>\r\n";
					foreach(String Key in stats.objective.Keys)
					{
						strStats += "\t\t<b>"+ Key + "captures:</b> " + ((CProfileDayofDefeatSource_Objective)stats.objective[Key]).captures + "<br />\r\n";
					}
					strStats += "\t\t<br />\r\n";

					strStats += "\t\t<h3>Weapon Statistics:</h3>\r\n";
					uint TotalKills = 0;
					uint TotalKilled = 0;
					foreach(String Key in stats.gun.Keys)
					{
						TotalKills += ((CProfile_Gun)stats.gun[Key]).kills;
						TotalKilled += ((CProfile_Gun)stats.gun[Key]).killed;
						strStats += "\t\t<b>"+ Key + ":</b> kills: " + ((CProfile_Gun)stats.gun[Key]).kills + " deaths: " + ((CProfile_Gun)stats.gun[Key]).killed + "<br />\r\n";
					}
					strStats += "\t\tTotal Kills: " + TotalKills + " Total Deaths: " + TotalKilled + "<br />\r\n";
					strStats += "\t\t<br /><br />\r\n";
					strStats += "\t</body>\r\n";
					strStats += "</html>";
					break;
				}
				case CProfile.SaveTypes.RTF:
				case CProfile.SaveTypes.TXT:
				{
					RichTextBox c = new RichTextBox();

					try
					{
						// Populate the rich text box
						c.SelectionStart = 0 ;
						c.SelectionFont = new Font(font, 12, FontStyle.Bold);
						c.SelectedText = ProfileName + " Statistics:\n" ;
						c.SelectionFont = new Font(font, 12, FontStyle.Bold|FontStyle.Underline);
						c.SelectedText = "                                                                \n\n";
						c.SelectedText = "Created with Smile! v" + Info.version + "\n";
						c.SelectedText = "©2005 Marek Kudlacz -- http://www.kudlacz.com\n";
						c.SelectionFont = new Font(font, 12, FontStyle.Bold|FontStyle.Underline);
						c.SelectedText = "                                                                \n\n";

						c.SelectionFont = new Font(font, 10, FontStyle.Bold);
						c.SelectedText = "Engagement Information:\n";
						c.SelectedText = "You've engaged others " + stats.given_counts + " times.\n";
						c.SelectedText = "You've been engaged " + stats.received_counts + " times.\n";
						c.SelectedText = "\n";

						c.SelectionFont = new Font(font, 10, FontStyle.Bold);
						c.SelectedText = "Hit Information:\n" ;
						c.SelectedText = "Hits given: " + stats.hits_given + "\n";
						c.SelectedText = "Average hits given per engagement: " + (float)stats.hits_given/stats.given_counts + "\n";
						c.SelectedText = "Hits received: " + stats.hits_received + "\n";
						c.SelectedText = "Average hits received per engagement: " + (float)stats.hits_received/stats.received_counts + "\n";
						c.SelectedText = "\n";

						c.SelectionFont = new Font(font, 10, FontStyle.Bold);
						c.SelectedText = "Damage Information:\n" ;
						c.SelectedText = "Damage given: " + stats.damage_given + "\n";
						c.SelectedText = "Average damage given per engagement: " + (float)stats.damage_given/stats.given_counts + "\n";
						c.SelectedText = "Damage received: " + stats.damage_received + "\n";
						c.SelectedText = "Average damage received per engagement: " + (float)stats.damage_received/stats.received_counts + "\n";
						c.SelectedText = "\n";

						c.SelectionFont = new Font(font, 10, FontStyle.Bold);
						c.SelectedText = "Misc Statistics:\n" ;
						c.SelectedText = "Miscellaneous Deaths: " + stats.deaths + "\n";
						c.SelectedText = "Suicides: " + stats.suicides + "\n";
						c.SelectedText = "\n";

						c.SelectionFont = new Font(font, 10, FontStyle.Bold);
						c.SelectedText = "Capture Statistics:\n" ;
						foreach(String Key in stats.objective.Keys)
						{
							c.SelectionFont = new Font(font, 9, FontStyle.Bold|FontStyle.Italic);
							c.SelectedText = Key + " captures: ";
							c.SelectionFont = new Font(font, 9, FontStyle.Regular);
							c.SelectedText = ((CProfileDayofDefeatSource_Objective)stats.objective[Key]).captures + "\n";
						}
						c.SelectedText = "\n";

						c.SelectionFont = new Font(font, 10, FontStyle.Bold);
						c.SelectedText = "Weapon Statistics:\n" ;
						uint TotalKills = 0;
						uint TotalKilled = 0;
						foreach(String Key in stats.gun.Keys)
						{
							TotalKills += ((CProfile_Gun)stats.gun[Key]).kills;
							TotalKilled += ((CProfile_Gun)stats.gun[Key]).killed;
							c.SelectionFont = new Font(font, 9, FontStyle.Bold|FontStyle.Italic);
							c.SelectedText = Key + ": ";
							c.SelectionFont = new Font(font, 9, FontStyle.Regular);
							c.SelectedText = " kills: " + ((CProfile_Gun)stats.gun[Key]).kills;
							c.SelectedText = " deaths: " + ((CProfile_Gun)stats.gun[Key]).killed + "\n";
						}
						c.SelectedText = "Total Kills: " + TotalKills + " Total Deaths: " + TotalKilled + "\n";
						c.SelectedText = "\n\n";

						c.SelectionStart = 0 ;
					}
					catch
					{
						c.Clear();
						c.SelectionStart = 0 ;
						c.SelectedText = "There was an error writing the stats, (missing font?) Try saving it to html instead using the edit menu.\n\n";
						c.SelectionStart = 0 ;
					}

					switch(format)
					{
						case CProfile.SaveTypes.RTF:
							strStats = c.Rtf;
							break;
						case CProfile.SaveTypes.TXT:
							strStats = c.Text;
							break;
					}
					c.Dispose();
					break;
				}
			}
			return strStats;		
		}
	}
}

