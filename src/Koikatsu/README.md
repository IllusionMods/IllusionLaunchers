# KoikatsuLaucher
A launcher for the Japanese adult game Koikatsu

![Screenshot](https://i.imgur.com/P9A0RoI.jpg "Screenshot")

# Features
- Fixes corrupt setup.xml on launch.
- Launcher has options to open all key folders, instead of just character like the vanilla one.
- Launcher has multiple language options.
- If files are placed appropriately, the launcher is able to assist with swapping ingame translations.
- If you plan to bundle it with mods or such, there is customization options available.

# Customization
This launcher allows for different kinds of customization:

### Versioning
Create a file named 'version' and place it together with the launcher. The string from that file will appear in the upper right of the launcher.

### Warning message
Create a folder in UserData named LauncherEN, and place a file named warning.txt file there with the text you want to appear in the notice part of the launcher.

### Background Image
Create a folder in UserData named LauncherEN, and place a file named LauncherBG.png there. The image should be 865 x 563 large.

### Character Image
Create a folder in UserData named LauncherEN, and place a file named Chara.png there. The image should be 250 x 370 large.

### Patreon
Create a folder in UserData named LauncherEN, and place a file named patreon.txt there. File should include a single URL.

### Supported Languages
Languages can be switched at any time in the launcher. The following languages are supported:

English, Chinese, Korean, Spanish, Brazilian, French, German and Norwegian (Bokmål)

### Change the ingame language via the launcher
[Only for Japanese version, not Party]

If you wish to use the launcher functionality to swap ingame language, please create a file named 'TLSwap' in /UserData/LauncherEN.

### Swapping Ingame Languages

To prepare Chinese Translation:

- Download the translation from https://www.zodgame.us/forum.php?mod=viewthread&tid=201179
- Extract to the game folder

To prepare Korean translation:

- Place the korean translations into BepInEx/translationKO (Create folder if it doesn't exist)

# CREDITS
SmokeOfC & ScrewThisNoise: Coding, translator communication/coordination and translation (English and Norwegian)

Earthship: Translation (Japanese)

Madevil & Earthship: Translation (Chinese)

Keris: Translation (Korean)

Heroine Nisa: Translation (Spanish)

Neptune: Translation (Brazilian)

Hyper: Translation (German)

TotalDecay78: Translation (French)

ManlyMarco: Sanity control and code snippets to help me out
