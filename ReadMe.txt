/////////////////////////////////////////////////////////////////////////////
//
// Smile! -- CounterStrike: Source Screenshot and Statistics Utility
// v1.2
// Written by Marek Kudlacz
// Copyright (c)2005
//
/////////////////////////////////////////////////////////////////////////////






About:
------------------------
Hmm. Well i never could get a good screenshot in CS:S. I was either too busy 
dying, or too busy sucking.  So here it is, an app that parses console.log 
quickly enough to take a screenshot when you kill someone and also keeps 
track of various statistics!

Enjoy!

-Marek





Requirements:
------------------------
• Counter Strike: Source
• Latest .NET runtime files (www.microsoft.com or windowsupdate.microsoft.com)  ********<-----IMPORTANT*******
• food
• i guess a mouse helps too





Configuration:
------------------------
Right Click on the smile icon, click open. See settings.

[General]
Options that are globally used by default.
	[Global Snap Settings]
	• Enabled: Enable or disable the snap settings (if you just want to do stats) for all games by default
	• Snap Directory: Where to save your screenshots
	• Single Display: Whether you're running on a singledisplay or not (only if you use a multi-monitor setup)
	• Snap Delay: How many x milliseconds to wait before you take a kill screenshot
	I have yet to find a good value myself. A delay of 0, is to fast, the screenshot is 
	taken before the hit is even registered on your display. Good values are 
	between 0-100ms.
	• Image Output Type: Let's you select between image formats (such as bitmap, jpeg, etc..)
	• Quality: If the selected image format supports compression, use this quality setting.

	[Global Stats Settings]
	• Enabled: Enable or disable the stats settings (if you just want to do screenshots) for all games by default
	• View: View stats.
	• Reset: Reset statistics for all games

[Profiles]
options that are specific to certain games.
	[Game Settings]
	• Path:	Path to the game's root directory.

	[Snap Settings]
	• Enabled: Enable or disable the snap settings (if you just want to do stats) for just this game
	• Snap Directory: Where to save your screenshots
	• Single Display: Whether you're running on a singledisplay or not (only if you use a multi-monitor setup)
	• Snap Delay: How many x milliseconds to wait before you take a kill screenshot
	I have yet to find a good value myself. A delay of 0, is to fast, the screenshot is 
	taken before the hit is even registered on your display. Good values are 
	between 0-100ms.

	[Stats Settings]
	• Enabled: Enable or disable the stats settings (if you just want to do screenshots) for just this game
	• View: View stats for this game.
	• Reset: Reset statistics for this game





Installation:
------------------------
Extract Smile! to a folder of your choice. You should just have this readme.txt, and smiletray.exe.

• Steam Installation (CS:S & HL2:DM):
Open steam. Go to "Play Games". Right-Click on "Counter-Strike: Source" and/or "Half-Life 2: Deathmatch" 
and click the “Launch Options" button. Add " -condebug" (without the quotes) as launch parameter.

• Quake III Arena & Enemy Territory (And Mods if supported)
Add "+set logfile 2" without quotes into the shortcut parameters of your favourite icon you use to start the game



Source:
------------------------
Source code is available from http://www.kudlacz.com




ShoutOuts:
------------------------
BUNG: Especially Adam for help in one thing, and pix & i[e for testing.
Military Forces Quake 3 Team
Steampowered.com forums
SniperKil from #teamxecuter
CyberRob for suggestions
Danny for reporting a bugs



Known Bugs:
------------------------
I left the below bug in just in case someone may recognize it even though i *think* i fixed it in v1.3. Never know i could have failed to fix it.
-Captures "stray" screenshots. Looks as if the program didn't wait for D3D to finish rendering, and sometimes catches just the environment. 
 Investigating alternative capture methods. Anyone have suggestions?



Why Smile? Why not "DEATHCAM! etc":
------------------------
like "say cheese" and you get a bullet in the face.. plus i dont want to change it once i picked it



Version Compatiblity:
------------------------
1.2: Compatible
1.1: Compatible
1.0: NOT Compatible



Changelog:
------------------------
**v1.3**
-Fixed a browse button under Profiles->Snap Settings
-Added Total Kills/Killed stats for weapons in all profiles
-Fixed "stray" screenshot bug


**v1.2**
-Added Quake III Arena Profile
-Added Quake III Team Arena Profile
-Added Quake III OSP Profile
-Added Enemy Territory Profile
-Added Counter-Strike Profile
-Added Day of Defeat Profile
-Adjusted CS:S and HL2:DM Profiles
-Now allow Search dialog to exit
-Fixed Browse Button under profiles
-Moved log to its own tab
-Add profile title to options page
-Made watermark semi transparent
-Fixed Stats and Snaps Enable Toggles
-Misc things and other bugfixes


**v1.1**
-Good portion rewritten
-New Interface
-Working on better DirectX Capturing (disabled)
-Add support for game profiles (Allows multigame support)
-Added Half-Life 2: Deathmatch profile
-Fixed singleDisplay toggle
-Added Quality slider
-Added support for various image encoders
-Log Window
-New Autodetect system

**v1.0**
- Initial Release



