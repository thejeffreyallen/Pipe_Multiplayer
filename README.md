# Pipe_Multiplayer
if you have exisiting 2.0 Multiplayer install:
- overwrite your Mods/FrostyPManager/   folder
- overwrite your PIPE_Data/FrostyPmanager/  folder
- overwrite your PIPE_Data/Custom Players  folder
- Remove PatchaMapImporter and/or Pipeworks map importer folders from the Mods folder

# What's new?
- Integrated the Patcha Map Importer by Herve3527 - https://github.com/herve3527/PatchaMapImporter
- The actual map filename is now sent to other players to make it easier to find who is riding where
- Various bug fixes
- Better handling of audio
- Performance increase
- Spectate Mode
- More Bike Customisation
- Parkbuilder will recognise objects with rigidbodies

# Manual Setup of FrostyP Game Manager


To install you need 4 things to be done

1) move a folder to your mods folder
2) move a folder to your PIPE_Data folder
3) Overwrite your AssemblyC.dll
4) Install Microsoft .exe included or yourself - Microsoft VC_Redistributable 2015-2019 will get you there on google, this must be 32bit for PIPE






# For Game:

step 1) Copy FrostyPGameManager folder from for_games_Mods/  to   YourPIPEfolder/Mods/


step 2) copy the FrostyPGameManager folder from forgamesPIPE_Data/ to your PIPE_Data/ 


step 3) OverWrite your AssemblyC.dll in PIPE_Data/Managed/ with AssemblyC in ForManaged/   (Back yours up!)


step 4) Ensure VC_Redist_X86 has been installed, provided in folder but also available from Microsoft, you may need to restart you pc (Strongly linked to "connect to server" doing nothing in game)

 Dont Grab them elsewhere and use they're included AssemblyC's, there AssemblyC mods are included in this AssemblyC, but not vice versa







# For Dedicated Game Server App:





Step 1) Provide Port forward for your PC, instructions vary on router model but you need to access your router as stated when you got it, usually by using your browser and typing 192.168.1.1
Once there the router must have the option to port forward, if you find that option, it will ask for an IP,port numbers and type,
You should only need UDP but TCP/UDP is fine,
then give your IP, which will be listed in the connected devices somewhere and be 192.000.000.000 format,
then choose any port within router specified range and apply,

this will mean anyone who knows to connect to your External IP on that Port wont be blocked by the router and will find your device for as long as your IP and your routers IP stays the same (ethernet is constant)
this usually means just being met with windows defenders, firewalls etc unless an app is there waiting like Microsoft Remote Desktop etc

If the incoming connection meets ValveSockets standards and passes initial connection phases you become connected,

safe to do but for normal users of alot of different apps, nothing like just flipping the port forward off when you dont want it though.

if your on WIFI you should setup a static IP so the IP your machine uses locally wont change on the fly and mess up your port forward.

you can also setup a dynamic Dns for your Router in its browser settings, doing this will mean people can save a url that will always find your router no matter what its IP gets changed to, useful for friends and with
the port forward off it means nothing really anyway


Step 2) Boot Server App

Step 3) Input Max players count and press enter

Step 4) Enter port to listen (match port setup in Port forwarding) and press Enter

Step 5) Enter TickRate and press enter

Step 6) Enter again to go live

Step 7) now in Host mode, provide clients with port you specified and External IP of your router - port you specify must be that of your port forward

# Auto Server
In the Host App folder, there is a batch script that can be configured and run that will input all the info about the server, i.e. players, port, tickrate, and will start the server in 1-click. It will also restart itself automatically in case it crashes.

There is also an aditional group of scripts in Host App/ServerBatchScripts that will start a group of servers, currently 4 are set up to auto run, but just duplicating the script will add more.

# Using the Mod

# in game:

G key to toggle in Game

toggle debugger is very useful, if you have problems within the manager, toggle on debugger at top of manager, hit space and retry what you did to make logs of the issues appear, i have many error messages in place.
Saves to Desktop/Your_logs





# Known issues:

GameNetworkingSockets not found - error loading dll when clicking Connect to server = issue with VC-redist most likely, 
potentially other builds of GameNetworkingSockets.dll required, message frostyP if this carries on


Return from parkbuilder gives insane mode until you get off or go to marker

ParkBuilder load park only works if you have all required bundles loaded

inital load up of textures not always correct, dependant on image formats, compression, bit depth

Auto sending and receiving of missing textures to/from the server can fail/bug dependant on same as above aswell as image name, disabled for now as server app can crash in some cases.

Server can sometimes catch error when a player disconnects, timing issue

Audio has various bugs, within Grinding mostly, if a Grind sound persists after exit, doing another grind on the same surface type will cut previous sound out.
if you join a game and hear constant bailing sound, reconnecting usually solves it, not sure about that so far.



----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------




# Requirements of ValveSockets:

Game Side:
1) GameNetworkingsockets.dll, Libprotofbuf.dll and libcrypto.dll must be found in PIPE folder next to .exe (all 32bit to match PIPE, modmanagers Load function checks this 
is true and copies files from Mod folder if not found). FrostyPGamemanager uses Valve.dll found in its mod folder, which can access GamenetworkingSockets and translate from C# to C++

2) GameNetworkingSockets is written in C++ not C#, vc redistributable 2015-2019 32bit is required (if you dont have visual studio? i didnt need it but laptop player 2 tester did)


Server side:
1)GameNetworkingsocksets.dll, Libprotofbuf.dll and libcrypto.dll and Valve.dll must be found in folder with servers .exe (64bit versions for server app, not to be messed up)

------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

# Players:

If people can see you, but you cant see them, try reconnecting, but could be a bug with custom players in your scene, restart game if nothing else.


1) Custom characters have their gameobject name and assetbundle name uploaded to the server as neither one is reliable for both finding the file if loading of bundle needed and locating 
an already loaded assetbundle on someone's machine among x players coming and going, Game should return a copy of your Daryien if all else fails.

Known name changes:

ron
Sessionguy
Marty Mcfly
Big Smoke cutscene
deadpool
Rick Sanchez
Shaggy Rogers
shrek






-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

# Other Requirements:

AssemblyC had to be edited, included AssemblyC has Multiplayer edits, TrickmodV2 and Pipeworks PI edits inside.


AssemblyC.dll alteration for Audio; most elegent solution ive found is to flip the EventInstance variable in FMODRiserByVel from private to public, this way i can use the FMOD(third party Audio asset) 
intended method of polling these stored EventInstance's to learn simple info about each sound. This also means i dont have send any audio or complex data, just the name of the sound, playstate(int),volume float, pitch float, velocity float.

One Shots required adding a momentary reference object to each oneshot, which is tracked in the same way
