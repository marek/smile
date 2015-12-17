/////////////////////////////////////////////////////////////////////////////
//
// Smile! -- Screenshot and Statistics Utility
// Copyright (c) 2005-2015 Marek Kudlacz
//
// http://kudlacz.com
//
/////////////////////////////////////////////////////////////////////////////



using System;
using System.Collections;
using System.Xml.Serialization;
using System.Drawing;

namespace smiletray
{
	class Info
	{
		public static readonly String version = "1.10";
		public static readonly String intversion = "1.10.0";
		public static readonly String copyrightdate = "�2005-2016";
	}

	public class HotKeySettings_t
	{
		public Boolean Enabled;
		public String HKWindow;
		public String HKDesktop;
		public String HKActiveProfile;

		public HotKeySettings_t()
		{
			Enabled = new Boolean();
			HKWindow = "F10";
			HKDesktop = "F11";
			HKActiveProfile = "F12";
		}
	}

	public class MiscSettings_t
	{
		public String CheckUpdates;
		public Int64 LastCheckTime;
        public Boolean SaveBug;
        public Int32 NumSaveThreads;
        public System.Threading.ThreadPriority SavePriority;
        public System.Threading.ThreadPriority CapturePriority;
        public System.Diagnostics.ProcessPriorityClass ApplicationPriorityClass;
        public System.Threading.ThreadPriority ApplicationPriority;
		public MiscSettings_t()
		{
			CheckUpdates = "Every Day";
			LastCheckTime = new Int64();
            SaveBug = new Boolean();
            NumSaveThreads = 3;
            SavePriority = System.Threading.ThreadPriority.Normal;
            CapturePriority = System.Threading.ThreadPriority.Normal;
            ApplicationPriorityClass = System.Diagnostics.ProcessPriorityClass.Normal;
            ApplicationPriority = System.Threading.ThreadPriority.Normal;
		}
	}

	public class SnapSettings_t
	{
		public Boolean Enabled;
		public Boolean AnimOriginalDimentions;
		public Boolean AnimUseMultiSnapDelay;
		public Boolean AnimOptimizePalette;
		public Int32 AnimWidth;
		public Int32 AnimHeight;
		public Int32 AnimFrameDelay;
		public Int32 Delay;
		public Int32 SaveQueueSize;
		public Int32 NextSnapDelay;
		public String SnapDir;
		public String Encoder;
		public Int32 Quality;


		public SnapSettings_t()
		{
			Enabled = new Boolean();
			AnimOptimizePalette = new Boolean();
			AnimOriginalDimentions = new Boolean();
			AnimUseMultiSnapDelay = new Boolean();
			AnimWidth = new Int32();
			AnimHeight = new Int32();
			AnimFrameDelay = new Int32();
			Delay = new Int32();
			SaveQueueSize = new Int32();
			NextSnapDelay = new Int32();
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
		public HotKeySettings_t HotKeySettings;
		public MiscSettings_t MiscSettings;

		public Settings_t()
		{
			SnapSettings = new SnapSettings_t();
			StatsSettings = new StatsSettings_t();
			HotKeySettings = new HotKeySettings_t();
			MiscSettings = new MiscSettings_t();
		}
	}

	[XmlRootAttribute("Smile")]
	public class Smile_t
	{
		public Settings_t Settings;
		[XmlElement(ElementName = "Profile")] public CProfile [] Profiles;
	}

	public class SaveQueueItem
	{
		public CProfile p;
		public ArrayList frames;

		public SaveQueueItem(CProfile p, ArrayList frames)
		{
			this.p = p;
			this.frames = frames;
		}
	}
}