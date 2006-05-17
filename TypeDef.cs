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
using System.Drawing;

namespace smiletray
{
	class Info
	{
		public static readonly String version = "1.5 BETA 5";
		public static readonly String copyrightdate = "©2005";
	}

	public class SnapSettings_t
	{
		public Boolean Enabled;
		public Boolean SingleDisplay;
		public Boolean SaveBug;
		public Boolean AnimOriginalDimentions;
		public Boolean AnimUseMultiSnapDelay;
		public Int32 AnimWidth;
		public Int32 AnimHeight;
		public Int32 AnimFrameDelay;
		public Int32 Delay;
		public Int32 SaveDelay;
		public Int32 NextSnapDelay;
		public String SnapDir;
		public String Encoder;
		public Int32 Quality;

		public SnapSettings_t()
		{
			Enabled = new Boolean();
			SingleDisplay = new Boolean();
			SaveBug = new Boolean();
			AnimOriginalDimentions = new Boolean();
			AnimUseMultiSnapDelay = new Boolean();
			AnimWidth = new Int32();
			AnimHeight = new Int32();
			AnimFrameDelay = new Int32();
			Delay = new Int32();
			SaveDelay = new Int32();
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

	// Thread Safe ArrayList
	public class TSArrayList
	{
		private ArrayList a;

		public TSArrayList()
		{
			a = new ArrayList();
		}

		public TSArrayList(int size)
		{
			a = new ArrayList(size);
		}

		public void Add(object item)
		{
			lock(a.SyncRoot)
			{
				a.Add(item);
			}
		}

		public object ObjectAt(int index)
		{
			lock (a.SyncRoot)
			{
				return a[index];
			}
		}

		public void AddRange(ICollection c)
		{
			lock(a.SyncRoot)
			{
				a.AddRange(c);
			}
		}

		public int Capacity() 
		{
			lock (a.SyncRoot)
			{
				return	a.Capacity;
			}
		}

		public void Clear()
		{
			lock (a.SyncRoot)
			{
				a.Clear();
			}
		}

		public bool Contains(object item)
		{
			lock (a.SyncRoot)
			{
				return a.Contains(item);
			}
		}

		public int Count() 
		{
			lock (a.SyncRoot)
			{
				return a.Count;
			}
		}

		public void Insert(int index, object Value)
		{
			lock (a.SyncRoot)
			{
				a.Insert(index, Value);
			}
		}

		public void Remove(object obj)
		{
			lock (a.SyncRoot)
			{
				a.Remove(obj);
			}
		}

		public void RemoveAt(int index)
		{
			lock (a.SyncRoot)
			{
				a.RemoveAt(index);
			}
		}

		public void RemoveRange(int index, int count)
		{
			lock (a.SyncRoot)
			{
				a.RemoveRange(index, count);
			}
		}
	}


}