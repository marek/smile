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
dying, or too busy sucking.  So here it is, an app that parses console.log (or qconsole.log for Vanilla HL, etc) 
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



Supported Games:
------------------------
Counter-Strike
Counter-Strike Source
Day of Defeat
Day of Defeat: Source
Wolfenstein: Enemy Territory
Half-Life 2: Deatchmatch
Quake III Arena
Quake III Arena: OSP
Quake III Team Arena



Installation:
------------------------
Extract Smile! to a folder of your choice. You should just have this readme.txt, and smiletray.exe.
In order to get Smile! working you need to at least configure your game to enable logs, and to enter paths for the specific games within Smile!'s options. 
Since paths include different games and their mods it may be confusing, so please refer to the "Game Paths" section of this Readme.txt for further 
details on what you should enter.




Game Configuration:
------------------------
• Steam Installation (HL, CS, DOD, DOD:S, CS:S & HL2:DM):
Open steam. Go to "Play Games". Right-Click on "Counter-Strike: Source" and/or "Half-Life 2: Deathmatch", and/or any other steam game
and click the “Launch Options" button. Add " -condebug" (without the quotes) as launch parameter.

• Quake III Arena & Enemy Territory (And Mods if supported)
Add "+set logfile 2" without quotes into the shortcut parameters of your favourite icon you use to start the game




Game Paths:
------------------------
These are the game paths Smile! looks for. Autodetect will do it's best to locate the proper folder, but if it fails use the information below. 

Legend:
X:\ being the letter of the drive steam is installed to.
<account name> being your steam login name (if supported)

• Counter-Strike
	Game Path:"X:\...\Steam\SteamApps\<account name>\counter-strike"
	Log File: "qconsole.log"

• Counter-Strike: Source
	Game Path: "X:\...\Steam\SteamApps\<account name>\counter-strike source\cstrike"
	Log File: "console.log"

• Day of Defeat
	Game Path: "X:\...\Steam\SteamApps\<account name>\day of defeat"
	Log File: "qconsole.log"

• Day of Defeat: Source
	Game Path: "X:\...\Steam\SteamApps\<account name>\day of defeat source\dod"
	Log File: "console.log"

• Wolfenstein: Enemy Territory
	Game Path: "X:\...\Enemy Territory" (or wherever et.exe is located)
	Log File: "etconsole.log"

• Half-Life 2: Deatchmatch
	Game Path: "X:\...\Steam\SteamApps\<account name>\half-life 2 deathmatch\hl2mp"
	Log File: "console.log"

• Quake III Arena
	Game Path: "X:\...\Quake III Arena" (or wherever quake3.exe is located)
	Log File: "\baseq3\qconsole.log"

• Quake III Arena: OSP
	Game Path: "X:\...\Quake III Arena" (or wherever quake3.exe is located)
	Log File: "\osp\qconsole.log"

• Quake III Team Arena
	Game Path: "X:\...\Quake III Arena" (or wherever quake3.exe is located)
	Log File: "\missionpack\qconsole.log"




Smile! Configuration:
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
	• Save Bug: Allow the program to retry saving multiple times if a file does not seem to save the first time (AntiVirus conflict)

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






Source:
------------------------
Source code is available from http://www.kudlacz.com




ShoutOuts:
------------------------
BUNG: Especially Adam for help in one thing, and pix & i[e for testing.
Military Forces Quake 3 Team
Steampowered.com forums
valve for their games
Id software for their games as well
Splash Damage for ET
SniperKil from #teamxecuter
CyberRob for suggestions
Danny for reporting bugs
Extreme_One for help in implementing DoD:S support
Iain for reporting bugs
TSW|Abaddon & Sloan for feature ideas
Everyone who has sent me a "thanks"



Known Bugs:
------------------------
I left the below bug in just in case someone may recognize it even though i *think* i fixed it in v1.3. Never know i could have failed to fix it.
-Captures "stray" screenshots. Looks as if the program didn't wait for D3D to finish rendering, and sometimes catches just the environment. 
 Investigating alternative capture methods. Anyone have suggestions?

-Not all screenshots save. Problem is probably due to the "realtime" antivirus scanners. Specificaly Norton Antivirus/Norton Internet Security. 
 A similar problem used to exist with Visual Studio itself. Make sure "Save Bug" is checked under the global Snap settings, all this does is 
 force the program to try and save multiple times if it fails the first time)


Why Smile? Why not "DEATHCAM! etc":
------------------------
like "say cheese" and you get a bullet in the face.. plus i dont want to change it once i picked it



Version Compatiblity:
------------------------
1.3: Compatible
1.2: Compatible
1.1: Compatible
1.0: NOT Compatible



Changelog:
------------------------
**v1.5**
-MultiSnap support for sequenced captures
-Save Queue with Save Delay to help decrease lag due to image saving activity
-Next Snap Delay added help tweak capture frequencies
-Animated GIF capture support


**v1.4**
-Fixed controls not updating when reopening after previously pressing "cancel" on the dialog
-Adding "Save Bug" for antivirus scanners interfering with saving images (Norton AV, Norton Internet Security)
-Added Day of Defeat:Source Profile
-Very minor adjustments to some profiles


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



