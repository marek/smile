## Changelog:

### **v1.9**
-Select Game Menu under the ViewStats form should now display sorted.
-Fixed Crash message on exit (log file closing problem)
-Changed the default priority settings everyone has back to Normal. Seemed to cause a lot of lag issues. Woops. :(


### **v1.8**
-Fixed the tray icon's right click 'about' menu item.
-Added Counter-Strike: Condition Zero support (thanks Jake)
-Added Ricochet support (thanks T@T-5-MoNk3y)
-Added the ability to adjust priority thanks to feedback (Thanks eram)
-Improved Cross platform compatability.
-Redid parts of the logging system again. Should provide some better information.


### **v1.7**
-Converted to .NET Framework v2.0 using Visual Studio 2005
-Removed depricated code
-Fixed Counter-Strike death statistics
-Minor adjustments to statistics text For all Profiles
-Added Team Fortress classic Profile (thanks Flank!)
-Fixed version checker
-Removed smiletray.exe.manifest file, instead it's now embeded into the program.
-Fixed path auto-detect for the Quake III Arena profile
-Slowly altering code to follow .NET coding standards and guidelines (gah, no end in sight!)


### **v1.6.2**
-Application will now come to foreground if another instance tries to start
-Fixed a bug with Enabling/Disabling Snaps/Stats


### **v1.6.1**
-Fixed Counter-Strike Profile taking screenshots when you die by killing self
-Fixed the fix (lol) that forced closing the main form to minimizing instead, because it caused windows not to shutdown. 
-Adjusted a few things in each profile yet again


### **v1.6**
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


### **v1.5**
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
-LICENSE CHANGED. Smile! IS NO LONGER UNDER GPL. 
 My reason for this is to allow the future use of code and libraries that do not fall under specific open source requirements thus
 keeping the author's own agreements and needs intact. Smile! will remain freeware, (It's not like there is a glimpse for anything 
 else) but closed source.


### **v1.4**
-Fixed controls not updating when reopening after previously pressing "cancel" on the dialog
-Adding "Save Bug" for antivirus scanners interfering with saving images (Norton AV, Norton Internet Security)
-Added Day of Defeat:Source Profile
-Very minor adjustments to some profiles


### **v1.3**
-Fixed a browse button under Profiles->Snap Settings
-Added Total Kills/Killed stats for weapons in all profiles
-Fixed "stray" screenshot bug


### **v1.2**
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


### **v1.1**
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

### **v1.0**
- Initial Release
- Rewritten in C#

### **v0.0-v1.0 Alpha**
- Initial proof of concept release, written in C