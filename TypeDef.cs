/////////////////////////////////////////////////////////////////////////////
//
// Smile! -- CounterStrike: Source Screenshot and Statistics Utility
// v1.2
// Written by Marek Kudlacz
// Copyright (c)2005
//
/////////////////////////////////////////////////////////////////////////////


using System;
using System.Collections;
using System.Xml.Serialization;

namespace smiletray
{
	class Info
	{
		public static readonly String version = "1.3";
	}

	public class SnapSettings_t
	{
		public Boolean Enabled;
		public Boolean SingleDisplay;
		public Int32 Delay;
		public String SnapDir;
		public String Encoder;
		public Int32 Quality;

		public SnapSettings_t()
		{
			Enabled = new Boolean();
			SingleDisplay = new Boolean();
			Delay = new Int32();
			Quality = new Int32();
		}
	}

	public class StatsSettings_t
	{
		public Boolean Enabled;

		public StatsSettings_t()
		{
			Enabled = new Boolean();
		}
	}

	public class Settings_t
	{
		public SnapSettings_t SnapSettings;
		public StatsSettings_t StatsSettings;

		public Settings_t()
		{
			SnapSettings = new SnapSettings_t();
			StatsSettings = new StatsSettings_t();
		}
	}

	[XmlRootAttribute("Smile")]
	public class Smile_t
	{
		public Settings_t Settings;
		[XmlElement(ElementName = "Profile")] public CProfile [] Profiles;
	}





}