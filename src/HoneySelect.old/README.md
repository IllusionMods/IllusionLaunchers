# HoneySelectLaucher
A launcher for the Japanese adult game HoneySelect

![Screenshot](https://i.imgur.com/KBtYDYc.png "Screenshot")

# Features
- Updates the registry with the correct path if it sees the HoneySelect_32.exe or HoneySelect_64.exe file in the same folder.
- Fixes corrupt setup.xml on launch.
- After registry is updated the launcher can be run from any location without problem.
- Launcher has options to open all key folders, instead of just character like the vanilla one.
- Launcher invokes IPA.exe on launch if IPA.exe is present in the game folder.
- If you plan to bundle it with mods or such, there is a field for declaring the pack version.

# Declaring pack version
Just create a file named 'version' and place it together with the launcher. The string from that file will appear in the upper right of the launcher.

# Credits
Based of SmokeOfC's KoikatsuLaucher