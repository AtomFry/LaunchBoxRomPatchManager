# LaunchBoxRomPatchManager
LaunchBoxRomPatchManager is a plug-in for LaunchBox that can help to streamline the process of patching a game and importing the patched game into your LaunchBox library

## Installation
1.  Download LaunchBoxRomPatchManager.zip from the LaunchBox forums or from this github repositories Releases
2.  Extract LaunchBoxRomPatchManager.zip to a folder.  Inside the LaunchBoxRomPatchManager folder is a folder called LaunchBox.  Inside the LaunchBox folder will be two folders: LaunchBoxRomPatchManager and Plugins.  Copy these two folders
3.  Go to your LaunchBox installation folder and paste the copied folders

## Included Patchers - Floating IPS
The plug-in includes Floating IPS by Alcaro to be used as IPS patcher.  This patcher should be automatically included when following the installation instructions and can be found in your LaunchBox folder under LaunchBoxRomPatchManager\Patchers.  By default, the plugin will be configured to use Floating IPS to patch games for the following platforms: 
- Nintendo Entertainment System
- Nintendo Game Boy Advance
- Nintendo Game Boy Color
- Nintendo 64
- Super Nintendo Entertainment System
- Sega Genesis
- Sega 32X
- NEC TurboGrafx-16

## Included Patchers - Paradox PPF
The plug-in includes Paradox PPF 3 by Icarus of Paradox to be used as a PPF patcher.  This patcher should be automatically included when following the installation instructions and can be found in your LaunchBox folder under LaunchBoxRomPatchManager\Patchers.  By default, the plugin will be configured to use Paradox PPF to patch games for the following platforms: 
- Sony Playstation
- Sony PSP

## Managing ROM Patchers
The plug-in includes a menu item called "Manage ROM Patchers" under the LaunchBox Tools menu.  The plug-in uses these configurations when applying a patch to a game.  

### Name
Give the patcher a name

### File path
Specify the path to the patcher

### Command line 
Specify the format of the command line that should be used when applying a patch to a game file.  There are two special values that need to be included in the command line field for the plug-in to know how to apply a patch to a game file.  {patch} indicates the patch file and {rom} indicates the game's rom file.

### Platforms
Select the platforms that the patcher can be used with

## Importing a ROM Hack
The plug-in includes a menu item called "Import ROM Hack" when you right click on a game.  To import a rom hack, right click on a game and select "Import ROM Hack".  A file dialog will open prompting to select a patch file.  Select the file that contains the patch file.  The "Import rom hack" screen will be displayed with information about the selected game and selected patch file.  Enter values for the imported ROM hack as desired and click OK.  The plug-in will make a copy of the source game, extract files as needed, apply the selected patch file to the source ROM file, and import the game into your LaunchBox library.   

# Special Thanks
- Thank you to Alcano for the amazing Floating IPS which the plug-in uses to apply IPS patches via command line
- Thank you to Icarus of Paradox for the amazing Paradox PPF which the plug-in uses to apply PPF patches via command line
