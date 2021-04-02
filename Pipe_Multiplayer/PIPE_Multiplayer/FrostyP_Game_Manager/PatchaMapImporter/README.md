# Patcha'MapImporter

DISCLAIMER: This mod is provided as-is. No warranty of any kind. Use it at your own risk and enjoy it (exactly the definition of bmx)

-> Volution Modding discord https://discord.gg/S89swkZ

Find all the mods, maps and more for BMX PIPE

-> @patchamak_trail https://www.instagram.com/patchamak_trail 

The trail I ride in my area. This map importer is dedicated to all the "dig'n'riders" who manage it.


## What is it

A map loading/filtering tool for the game BMX PIPE by MASH: http://bmxstreets.com/pipe/ 
BUY THIS AWESOME GAME (and the only serious bmx game we have now) AND THE _REAL_ UPCOMING BMX STREETS GAME.
You must own the game legally to use this mod so BUY THIS GAME TO SUPPORT MASH.

PIPE is a physics based BMX experience built for the BMX community designed to simulate realistic BMX motion. 
Perform over 50 aerial stunts, grind any corner, send big airs in the PIPE! PIPE also includes some fun Easter Eggs for those willing to explore and think outside the box.


## Why it exists

The modding community is creating so much maps that I can't remember what's the name of the map i want to play. 

I created this mod for adding personnal datas to the maps (name, rating, riding types, authors, descriptions) and an UI to filter the maps using theses datas.
This data are stored inside a JSON file in the CustomMaps folder. 

This mod DOES NOT RENAME NOR CHANGE THE MAP FILES. It only use this json file to store data.
Don't delete it or the maps will be back to the default values (name = filename, rating=50, all types of riding). Feel free to make backup of it.


## How it work

You need BMX PIPE 1.9.9, Unity Mod Manager installed and running as well.
Follow the tutorials in the pinned messages in the #help channel in https://discord.gg/S89swkZ for installing Mods inside PIPE BMX.

Copy the folder "PatchaMapImporter" from the zip file inside the folder "Mods" of the game. 

To use it, launch the game, load the standard map "the Community Center", and then press the key "L" (for "List") to show the Patcha'Map Importer UI. 
(the key "M" is already used by the legacy PipeWorksMapImporter, so you can use both at the same time.)

This UI list all the maps you have in your "CustomMaps" folder like the original MapImporter. 
_Please note that this mod does not change in any way the map files. They will remain as is, as the mod is storing elsewhere the additional infos (_pmi.json in the CustomMaps folder)_

For each map, you will be able to add datas to classify your maps :
Name: change the name of the map (default: filename)
Author: show the name of the authors so they can get what they deserve
Types: choose the types of riding you find in the map
Rating: from 1 to 100, what do you think of the map (mainly an option for ordering your map, default=50)
Description : free text to add to the map

All these datas will permit you to filter, order, and search for the map you want to play, by using the filter at the top of the ui.
example : Easy to check only 'Trail', then search "sil" to find directly the silverlake trail map and go into the forest going big.

To load a map, simply click on it in the list.
To edit a map, 3 possibilities:
- click on the "edit" button, 
- right-click on it in the list.
- after loading the map, when playing, directly press "E" (for "Edit"). Easy to put the description of the map you're currently playing.

## How to build

Copy the Assembly-CSharp.dll and Assembly-CSharp-firstpass.dll from the Game inside the "Libs" folder.
