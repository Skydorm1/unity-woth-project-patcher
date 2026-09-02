<div align="center">
  <h1>Whisper of the House Project Patcher</h1>

  <img src="images/banner.jpg" />

  <p>
    A game wrapper that generates a Unity project from Whisper of the Houses's build that can be playable in-editor
  </p>
</div>
<br />

# Table of Contents

- [Recent Updates](#recent-updates)
- [Current State](#current-state)
- [About the Project](#about-the-project)
- [Getting Started](#getting-started)
- [Installation](#installation)
- [Usage](#usage)
- [FAQ](#faq)

## Current State

This project is currently in a very early stage of development. It is able to extract the necessary game assets, move the required DLLs, and apply a number of source code patches to generate a Unity project that can be opened normally in the Unity editor without entering Safe Mode.

However, the generated project is **not yet fully functional**. Many issues that occur while entering Play Mode have not been addressed yet. These can include missing assets or additional asset processing (Annoying Stuff like Text Animator missing so idk i own that package thankfully so i just gonna ignore that problem atm).

There are also known runtime issues, such as a crash caused by Wwise during the initialization of `SetBasePath`. This can currently be avoided by disabling the `AkInitializerCompat` GameObject. Or by not entering the Main Menu scene.

The current goal is therefore to provide a **playable-in-editor project structure rather than a fully working game**. The project can be opened and worked with in Unity, but further work is required before the game can run correctly and all of its systems function as intended to properly produce Mods.

A significant amount of work has gone into getting the project to this point, including the AssetRipper changes, wrapper adjustments, Unity project setup, and resolving the resulting compilation and initialization issues.

At this point, I'm taking a break from the project. I also need to prioritize my bachelor's thesis, as time is becoming increasingly limited ,_,

The project has been tested through multiple fresh Unity project setups and a considerable amount of time has already gone into investigating and fixing the issues introduced by the AssetRipper changes.

If you continue working on the wrapper and run into additional **major issues**, feel free to document them. I would appreciate keeping track of anything that prevents the project from reaching a fully playable state.

## About the Project

This tool is a game wrapper on top of the [Unity Project Patcher](https://github.com/nomnomab/unity-project-patcher) and was build by looking into other Wrappers like https://github.com/Kesomannen/unity-repo-project-patcher. (As you might notice from the very similar Readme)

The Tool takes a build of Whisper of the House, extracts its assets/scripts/etc, and then generates a project for usage in the Unity editor.

> [!IMPORTANT]  
> This tool does not distribute game files. It simply works off of your copy of the game!
>
> Also, this tool is for **personal** use only. Do not re-distrubute game files to others.

## Getting Started

Make sure you have the following before using the tool in any way:

- [Git](https://git-scm.com/download/win)
  - To download packages in Package Manager through git URL
- [.NET 9.0](https://dotnet.microsoft.com/en-us/download/dotnet/9.0)
  - To run Asset Ripper

> [!IMPORTANT]  
> The Project requires up to 30GB of free Space.
> (No i don't know why its so much)


## Installation

### Unity Project

- Requires [Unity 2021.3.45f2](https://unity.com/de/releases/editor/whats-new/2021.3.45f2)
- Unity (3D) URP render pipeline

Create a new Unity project with the above requirements before getting started.

You will need to install three packages in sequence here:

- Unity Project Patcher: `https://github.com/nomnomab/unity-project-patcher.git`
  - [Can be disabled](#disabling-bepinex-usage)
- This project
- Unity Project Patcher BepInEx: `https://github.com/Kesomannen/unity-project-patcher-bepinex.git#update-mono-cecil`

### Installing the Unity Project Patcher core

1. Open the Package Manager from `Window > Package Manager`
2. Click the '+' button in the top-left of the window
3. Click 'Add package from git URL'
4. Provide the URL of the this git repository: `https://github.com/nomnomab/unity-project-patcher.git`
   - If you are using a specific version, you can append it to the end of the git URL, such as `#v1.2.3`
5. Click the 'add' button

```json
"com.nomnom.unity-project-patcher": "https://github.com/nomnomab/unity-project-patcher.git"
```

- If you are using a specific version, you can append it to the end of the git URL, such as `#v1.2.3`

### Installing this Game Wrapper

The same steps as previously, just with `https://github.com/Skydorm1/unity-woth-project-patcher.git`

### Installing the BepInEx Wrapper

Open the tool window `Tools > Unity Project Patcher > Open Window` and press the `Install BepInEx` button.

Otherwise, follow the steps at https://github.com/nomnomab/unity-project-patcher-bepinex

#### Disabling BepInEx Usage

If you don't want to use plugins, then follow the steps at https://github.com/nomnomab/unity-project-patcher-bepinex#disabling-this-package

## Usage

The tool window can be opened via `Tools > Unity Project Patcher > Open Window`

1. Open the **Unity Project Patcher** window and press **Run**.
2. You will be prompted to download three required packages. Confirm by pressing **Yes**.
3. If Unity asks you to restart for the **Input System**, select **No**.
4. If Unity asks whether to enter **Safe Mode**, select **Ignore** and allow the project to open normally.

> [!IMPORTANT]  
> This tool mostly supports patching an already patched project, although this can lead to broken assets.
> So make sure you back up your project beforehand.

Estimated patch durations:

- Fresh patch: ~1:30h

These can vary wildly depending on system speed and project size.

The current wrapper setup is not fully compatible with the default AssetRipper workflow.

For this project, we use a **custom AssetRipper build** that extracts all scripts and places them inside the project's `Scripts` folder.

After extraction:

* Delete all folders and files inside the `Scripts` folder **except `Assembly-CSharp`**.
* This should remove the majority of the compilation errors caused by duplicate or incompatible extracted files.

If compilation errors show up, fix them first before continuing with the next step.

Next, create the Addressables configuration:

1. Open:
   `Window > Asset Management > Addressables > Groups`
2. Select **Create Addressables Settings**.
3. Unity will ask whether you want to automatically convert the legacy AssetBundle setup.
4. Select **Convert**.
5. The conversion may take some time. Let Unity finish the process before continuing.

After this step, most of the major setup issues should be resolved.

The project should now be able to enter Play Mode, **as long as you start from the `InGameLevelEditor` scene**.

The main menu currently has an issue related to **Wwise**, so it should be avoided for now.

At this stage, there are currently two major known issues:

1. **Text Animator**
2. **Wwise**

These are the remaining major areas that still need investigation and fixing.

### Addressables Issues

If you encounter an Addressables-related error where the game is unable to locate a required object:

1. Find the object referenced by the error.
2. Add it to the **Default Group** in:
   `Window > Asset Management > Addressables > Groups`
3. If necessary, update the corresponding path/reference in the code.

The exact object and path may vary depending on the error.

### Known Additional Issue

Basement and City can't be entered currently.


## FAQ

**Q: Why is my game crashing, when going into playmode in main menu**

You have to deactivate the Object "AkInitializerCompat" in the main menu Scene.

**Q: I get errors with Febucci**

There is currently no workaround for that, it's the biggest hurdle atm if you don't own the package.
