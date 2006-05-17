using System;
using System.Collections;

namespace smiletray
{
	// Gun Stat
	[Serializable]
	struct gun_t
	{
		public uint kills;
		public uint killed;
	}

	// Player Stats
	[Serializable]
	struct stats_t
	{
		public uint damage_given;
		public uint damage_received;
		public uint hits_received;
		public uint hits_given;
		public uint received_counts;
		public uint given_counts;
		public uint hits_team;
		public uint deaths;		// misc deaths, does not include being killed by weapon
		public uint suicides;
		public Hashtable gun;
	}

	[Serializable]
	struct SnapSettings_t
	{
		public bool enabled;
		public uint delay;
		public String snapdir;
	}

	[Serializable]
	struct StatsSettings_t
	{
		public bool enabled;
		public stats_t stats;
	}

	[Serializable]
	struct GlobalSettings_t
	{
		public bool enabled;
		public stats_t stats;
	}

	[Serializable]
	struct Settings_t
	{
		public GlobalSettings_t GlobalSettings;
		public SnapSettings_t SnapSettings;
		public StatsSettings_t StatsSettings;
	}


}