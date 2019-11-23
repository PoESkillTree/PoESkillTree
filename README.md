# PoESkillTree [![Discord](https://b.thumbs.redditmedia.com/YzI6TxCJcacCZw1sx1Z5tyy6YskyNiA84hn4WfPXaRM.png)](https://discord.gg/sC7cUHV)


A Passive Skill Tree and Character Planner for Path of Exile

![](https://github.com/EmmittJ/PoESkillTree/wiki/images/tree.png)

## Features

* DPS and Defensive calculations: Click on "Computation" in the top right corner.
  * This is currently an alpha version with a first basic UI and doesn't support everything (notably no item-inherent skills and nothing minion related).
  * Skills can be set in "Equipment" by double-clicking inventory items.
* Tree comparison: Compare current tree with the tree of a saved build
* Search the tree
  * Search by text through the field in the bottom bar
  * Search by attribute name through the context menus of attributes in the "Attributes" sidebar
  * Find similar nodes by hovering over a node while holding down Shift
* Group attributes: Manage groups through the context menu of attributes. Enter a # as part of the group name and it will sum the attributes in the group.
* Show all changes to attributes when skilling a node: click `View -> Show summary of changes` or press Ctrl+G
* Equipment
  * Accessible by clicking on "Equipment" in the top right corner
  * Change your character's equipped items
  * Change socketed gems of items (double left click or right click the item)
  * Organize imported and crafted items in a stash
  * Import character inventories and stash tabs
  * Craft rare and unique items
  * Inventory is saved per build, stash is shared between all builds
* Import build URLs from:
  * PoEBuilder
  * PoE Planner
  * PoEURL
  * TinyURL
* Export build to PoEURL link
* Build organization
  * Builds are organized in folders
  * Drag&Drop builds to reorder them and move them between folders
  * Edit name, note, account and character name by right click -> Edit
  * Builds are saved as individual files. Open the root folder by clicking `File -> Open build directory`. Edit the location in `Edit -> Settings -> Build save path`.
  * Share build files with others:
    * Share the build file itself: Others can import it by opening the file with PoESkillTree (double click the file).
    * Share the build file as text: Copy a build (press Ctrl+C or right click -> Copy) and paste it somewhere. Others can then copy the pasted text and paste it into the program (select a build or folder in PoESkillTree and press Ctrl+V or right click -> Paste).
  * Check the context menu of builds for more options
* Let PoESkillTree help you in creating trees:
  * The Skill Tree Generator can create trees based on constraints set by you. Go to `Nodes -> Skill Tree Generator` to find out more.
* Hotkeys
  * To see available hotkeys, go to `Help -> Hotkeys` or check the menu/context menu entry in question.
* GUI
  * Hide sidebars
  * Ability to choose color theme (`View -> Theme` and `View -> Accent`)

## Install/Update

### Install

1. Go to the [release page](https://github.com/EmmittJ/PoESkillTree/releases) and select the version you would like, most likely the latest.
2. Download
  1. Portable: choose the zip file
  2. Installer: choose the exe file
3. Unzip/Install it to a location of your choice
4. Start PoESkillTree.exe

### Update

**Automatic**

Go to: ```Help -> Check for Updates```

**Manually**

If you have settings and/or saved builds you want to keep when updating do this:

1. Follow the first three steps under *Install*
2. Now either unzip to same directory and overwrite everything. Or choose a new location and then copy over PersistentData.xml. To copy your saved builds, follow [this wiki page](https://github.com/EmmittJ/PoESkillTree/wiki/How-To:-Copy-Builds).

### Skilltree version

The program always ships with the latest skilltree data available when the release is done. If you know there is a new version of the tree on the official website but no new version of this program is available you can do this:

1. Delete the Data folder and then just start the program, it will download the latest version.
2. If you have the program running just go to: ```Tools -> Redownload Skill Tree Assets```

Note: Both these options require that you can access the official Path of Exile website.

## Help/Issues

* For help please check these links:
  * [Thread in the official forum](https://www.pathofexile.com/forum/view-thread/996805/)
  * [Wiki on Github](https://github.com/EmmittJ/PoESkillTree/wiki)
  * [Open and closed issues on GitHub](https://github.com/EmmittJ/PoESkillTree/issues?utf8=%E2%9C%93&q=is%3Aissue)
  * [PoESkillTree Discord](https://discord.gg/sC7cUHV)
* If you find any bugs/faults please report it here on GitHub
* Miss any features? Create an issue here or post in the forum thread

## Information for contributors

* The code requires Visual Studio 2019 or higher to be compiled and run
* It is compiled to .NET Core 3.0 using C# 8
* https://github.com/PoESkillTree/PoESkillTree.Engine contains the GameModel and Computation engine and is included as a NuGet package

## Credits

* Headhorr - for his original "Unofficial Offline Skilltree Calc" http://www.pathofexile.com/forum/view-thread/19723
* Emmitt
* Kaezin
* SpaceOgre
* l0g0sys
* NadenOfficial
* Ttxman
* yazilliclick
* MauranKilom
* brather1ng

## Screenshots

![](https://github.com/EmmittJ/PoESkillTree/wiki/images/tree.png)
![](https://github.com/EmmittJ/PoESkillTree/wiki/images/equipment.PNG)
![](https://github.com/EmmittJ/PoESkillTree/wiki/images/computationTab.PNG)
![](https://github.com/EmmittJ/PoESkillTree/wiki/images/treeGen.png)
![](https://github.com/EmmittJ/PoESkillTree/wiki/images/gems.png)
