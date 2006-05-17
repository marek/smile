/////////////////////////////////////////////////////////////////////////////
//
// Smile! -- Screenshot and Statistics Utility
// Copyright (c) 2005 Marek Kudlacz
//
// http://kudlacz.com
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


License:
------------------------
See LICENSE.TXT for more details. As of v1.5, Smile! is no longer under GPL. Please see the 
little blurb under the changelog for details.


Requirements:
------------------------
• Any of the Supported Games listed
• AMD 2000+ or Intel 2Ghz equivalent
• 512mb Ram (Animations require a lot more, or you start crapping out after 2)
• Latest .NET runtime files (www.microsoft.com or windowsupdate.microsoft.com)  ********<-----IMPORTANT*******
• food
• i guess a mouse helps too



Supported Games:
------------------------
Counter-Strike
Counter-Strike Source
Day of Defeat
Day of Defeat: Source
Dystopia (HL2/Source Mod)
Half-Life 2: Deatchmatch
Jedi Academy
Quake III Arena (and some mods)
Wolfenstein: Enemy Territory


Installation:
------------------------
Extract Smile! to a folder of your choice. You should just have this readme.txt, and smiletray.exe.
In order to get Smile! working you need to at least configure your game to enable logs, and to enter paths for the specific games within Smile!'s options. 
Since paths include different games and their mods it may be confusing, so please refer to the "Game Paths" section of this Readme.txt for further 
details on what you should enter.




Game Configuration:
------------------------
• Steam Installation (HL, CS, DOD, DOD:S, CS:S, HL2:DM & Dystopia):
Open steam. Go to "Play Games". Right-Click on "Counter-Strike: Source" and/or "Half-Life 2: Deathmatch", and/or any other steam game
and click the “Launch Options" button. Add " -condebug" (without the quotes) as launch parameter.

• Quake III Arena, Enemy Territory & Jedi Academy (And Mods if supported)
Add "+set logfile 2" without quotes into the shortcut parameters of your favourite icon you use to start the game
Note: A gamma value of 2.2 and contrast value of +30 is recommended for this profile in fullscreen.
Note 2: Any mod that keeps the kill messages intact from Vanilla Quake III Arena is supported, Just make sure to change the Path to include the mod folder



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

• Dystopia (HL2/Source Mod)
	Game Path: "X:\...\Steam\SteamApps\SourceMods\dystopia"
	Log File: "console.log"	

• Half-Life 2: Deatchmatch
	Game Path: "X:\...\Steam\SteamApps\<account name>\half-life 2 deathmatch\hl2mp"
	Log File: "console.log"

• Jedi Academy
	Game Path: "X:\...\Jedia Academy" (or wherever JediAcademy.exe is located)
	Log File: "\GameData\base\qconsole.log"

• Quake III Arena (And Mods like Team Arena and OSP)
	Game Path: "X:\...\Quake III Arena\baseq3" (or wherever quake3.exe\mod_name is located)
	Log File: "\qconsole.log"

• Wolfenstein: Enemy Territory
	Game Path: "X:\...\Enemy Territory" (or wherever et.exe is located)
	Log File: "etconsole.log"





Smile! Configuration:
------------------------
Right Click on the smile icon, click open. See settings.

[General]
Options that are globally used by default.
	[Global Snap Settings]
	• Enabled: Enable or disable the snap settings (if you just want to do stats) for all games by default
	• Save Bug: Allow the program to retry saving multiple times if a file does not seem to save the first time (AntiVirus conflict)
	• Snap Delay: How many x milliseconds to wait before you take a kill screenshot
	I have yet to find a good value myself. A delay of 0, is too fast, so the screenshot is 
	taken to early and the player will just look like it's twitching. Good values are 
	between 0-100ms.
	• Next Snap Delay: How long to wait before taking another snap/series of snaps
	• SaveQueue Size: Maximum number of pictures/frames to store in memory if not all saved yet
	• Snap Directory: Where to save your screenshots
	• Image Output Type: Let's you select between image formats (such as bitmap, jpeg, etc..)
	• Quality: If the selected image format supports compression, use this quality setting.
	• Use Optimized Palette: Use an Optimized Octree algorithm for saving gif animations (slower + bigger file size), 
	otherwise used a fixed palette.
	• Use Original Dimentions: Whether to keep or resize the image when saving a sequence to an animation
		• Width: Width of animation
		• Height: Height of animation
	• Use MultiSnap Delay: Whether to use a fixed delay for all animations, or use the capture delay between multiple frames
		• Frame Delay: Custom delay inbetween frames
	
	[Global Stats Settings]
	• Enabled: Enable or disable the stats settings (if you just want to do screenshots) for all games by default
	• View: View stats.
	• Reset: Reset statistics for all games

	[Hot Keys]
	• Enabled: Enable or disable the hot keys
	• Capture Window: Hotkey to capture the current window.
	• Capture Desktop: Hotkey to capture the entire desktop.
	• Capture Active Profile: Hotkey to invoke a typical profile capture.


[Profiles]
options that are specific to certain games.
	[Game Settings]
	• Path:	Path to the game's root directory.

	[Snap Settings]
	• Use Global Settings: Use global options intead of profile specific ones.
	• Enabled: Enable or disable the snap settings (if you just want to do stats) for just this game
	• Save: "Only Snaps" save only single framed images, "Only Animations", Save only animatation sequences (Snap Count must be > 1)
	or "Snaps & Animations" to save a copy of both
	• Snap Delay: How many x milliseconds to wait before you take a kill screenshot
	I have yet to find a good value myself. A delay of 0, is to fast, the screenshot is 
	taken before the hit is even registered on your display. Good values are 
	between 0-100ms.
	• Next Snap Delay: How long to wait before taking another snap/series of snaps
	• Snap Count: How many frames to capture in a sequence
	• MultiSnap Delay: How long to wait inbetween each frame captured in a sequence
	• Snap Directory: Where to save your screenshots
	• Gamma: Adjust gamma. 1.0 = No change.
	• Contrast: Adjust contrast. 0 = No change.
	• Brightness: Adjust brightness. 0 = No change.

	[Stats Settings]
	• Use Global Settings: Use global options intead of profile specific ones.
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
Fuzzy from Team Dystopia
tiki from planetquake (for lots of testing, bug fixing, and JediAcademy Profile help)
Leo for suggestions
`star from #counter-strike
KodeK for .manifest file and bug reports/suggestions
Everyone who has sent me a "thanks"



Known Bugs:
------------------------
-Captures "stray" screenshots. Looks as if the program didn't wait for D3D to finish rendering, and sometimes catches just the environment. 
 Investigating alternative capture methods. Anyone have suggestions? I'm working on a hook dll to try and capture 
 from DirectDraw/Direct3D/OpenGL directly.

-Not all screenshots save. Problem is probably due to the "realtime" antivirus scanners. Specificaly Norton Antivirus/Norton Internet Security. 
 A similar problem used to exist with Visual Studio itself. Make sure "Save Bug" is checked under the global Snap settings, all this does is 
 force the program to try and save multiple times if it fails the first time)


Why Smile? Why not "DEATHCAM! etc":
------------------------
like "say cheese" and you get a bullet in the face.. plus i dont want to change it once i picked it



Version Compatiblity:
------------------------
1.6: Compatible
1.5: Compatible
1.4: Compatible
	-- Need to update Quake III Arena Paths
1.3: Compatible
1.2: Compatible
1.1: Compatible
1.0: NOT Compatible



Changelog:
------------------------
**v1.7**
-Fixed Counter-Strike Profile taking screenshots when you die by killing self
-Fixed the fix (lol) that forced closing the main form to minimizing instead, because it caused windows not to shutdown. 
-Adjusted a few things in each profile


**v1.6**
-Added smiletray.manifest file into the distrubution to enable XP themes (thanks KodeK)
-Fixed Index out of bounds error on ViewStats: Edit->Save To File->Cancel
-Now only one instance of Smile! is allowed. 
-Should no longer crash if cannot open session log file will warn you if anything (one instance only should also prevent this further)
-Pressing "X" in the main form no longer closes the program, but minimizes it instead.
-Removed Minimize and Maximize buttons in the main form.
-General Form timers now will not start till all other things are initiated. Should get rid of all init bugs involving them.
-Fixed bug with blank hotkeys
-Added version checking. Will contact the site for new version details (Enabled by default to "Every Day")
-Code cleanup. Started building up some unused things already. Heh
-Fixed bugs involving using the wrong time


**v1.5**
-Rewrote a lot of stuff to introduce threading (SaveQueue w/ 3 workers and one Scanner/Capture thread).
-MultiSnap support for sequenced captures
-Save Queue features a save delay to help decrease lag due to image saving/trashing activity
-Next Snap Delay added help tweak capture frequencies
-Animated GIF capture support
-Added Dystopia Profile (an HL2/Source Mod)
-Added Jedi Academy Profile
-Removed Quake III Arena: OSP  (See Quake III Arena Installation Notes)
-Removed Quake III Team Arena (See Quake III Arena Installation Notes)
-Redid Counter-Strike 1.6 Profile (Should take all screenshots + more statistics now)
-More misc bug fixes
-Removed "single display" check box, and am temporarly leaving window/fullscreen capture only
-Added QueueSize control
-New filename format for snaps: smile00000001.jpg smile0000002.jpg  etc.
-Added Profile Gamma Slider
-Added Profile Contrast Slider
-Added Profile Brightness Slider
-Added Capture Window, Capture Desktop, and Capture Active Profile hotkeys
-Redid log reading
-Better handling of corrupt configs
-Old settings now backed up, if an error reading them occured. 
-Fixed AutoDetect for Profile Paths
-LICENSE CHANGED. Smile! IS NO LONGER UNDER GPL. As I am the sole contributer and owner of Smile! I am granted this right. Hey Mozilla did it! 
 My reason for this is to allow the future use of code and libraries that do not fall under specific open source requirements thus
 keeping the author's own agreements and needs intact. Smile! will remain freeware, (It's not like there is a glimpse for anything 
 else) but closed source.


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



