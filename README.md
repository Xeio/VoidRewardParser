# Void Reward Parser
Parses the Void rewards screen for Warframe and displays ducat Values

##To use:

Open in background with Warframe open. As long as Warframe is open it will scan the primary monitor for a void rewards screen.
If detected it will parse out any prime parts and display the rarity and Ducat value in a list.

Requires Warframe to run in *Borderless Windowed* or *Windowed* mode, does not work fullscreen.

##Supported Languages:

English, Russian, Portuguese, German

To change languages open the VoidRewardParser.exe.config file in a text editor and change this line to the selected language.

<add key="Language" value="English"/>

If your Windows default language does not match the language you use in the game, you may also need to change this line to include the language code you run the game as (such as "en", "pt", "ru", "de"). Note that this language must be installed in your Windows 10 region and laguage settings.

<add key="LanguageCode" value="en"/>

If your language isn't currently supported, check out the [new localization readme](https://github.com/Xeio/VoidRewardParser/tree/master/VoidRewardParser/Localization), and send me the file so I can include it.

##Requirements:

* **Windows 10**

* .Net Framework 4.56+