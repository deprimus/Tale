<p align="center">
  <img src="public/logo.png" alt="Tale">
</p>

# About <a href="https://unity.com"><img align="right" src="https://img.shields.io/badge/Unity-6000.0.49f1-000000?logo=Unity" alt="Unity 6.0" /></a><a href="https://unity.com"><img align="right" src="https://img.shields.io/badge/Unity-2020.3.2f1-000000?logo=Unity" alt="Unity 20" /></a>

**Tale** is a powerful prop manipulation utility designed for storytelling. You can use it in your games to show dialog, play sounds and videos, manipulate the camera, create transitions, and much more. Think of it as a more powerful timeline driven by code.

Everything is very customizable; you give Tale a bunch of objects and animations, and it does the heavy lifting.

Here's what Tale currently provides:

- **Scene Manipulation**
- **Dialog**, with support for:
  - rich text
  - avatars
  - additive mode (continue the previous reply)
  - animations like click-to-continue
  - voices:
    - normal and also looping (for Undertale-like voices)
    - reverb effect (for monologue or "inner" voices)
  - custom actions instead of click-to-continue (like `Tale.Choice`)
- **Cinematics**, with support for:
  - Videos
  - Subtitles
  - Images, with multiple animations like crossfade
- **Audio**, with support for:
  - Multi-Channel sound (multiple `AudioSources`)
  - Music, with loop/shuffle support
- **Transitions**, based on custom animations
- **Camera Manipulation**, with support for:
  - Post-Processing
  - zoom, position and rotation
- **Transform Manipulation**, with support for:
  - position and rotation
- **Scene Selector** with support for:
  - auto-generated thumbnails
  - custom thumbnails taken any time via `F11` inside the editor
  - blacklisted scenes, which won't show up in the selector
- **Other Actions**, such as:
  - Exec, for custom code that runs on the Tale queue
  - Repeat and Delay, for managing other actions
  - Multiplex and Parallel, for orchestrating many actions at the same time
- **Await**, for async actions that bypass the Tale queue
- **Trigger System**, for sending events between completely decoupled scripts
- **Input Abstraction**, which emulates the old Input Manager but also seamlessly works with the new Input System behind the scenes
- **Editor Tools**, including:
  - one-click first setup
  - support for adding or removing Tale components after the setup
  - transition and splash scene creation
  - scene selector thumbnail auto-generator
- **Debug Tools**, including:
  - magic stack traces for exceptions, which show where the faulty action was actually created
  - debug info which displays the current action queue
- **Script Compiler**, for easily generating Tale boilerplate code from story scripts

You can easily extend Tale by creating your own actions.

Check out [WaitAction](https://github.com/deprimus/Tale/blob/master/Assets/Scripts/Tale/Actions/WaitAction.cs) to see how to implement a simple action!

# Getting started

1. **Copy** the files from this **repo's** `Assets` folder into your **project's** `Assets` folder
2. In the **toolbar**, click `Tale -> Run Setup`. If you don't see this, reload via `CTRL + R`
3. **Click** `Run Setup`
4. **Click** `Import TMP Essentials` **if** a window pops up (Tale needs the TextMeshPro Essentials package)
5. All done. Tale is now fully operational.

You don't have to worry about the advanced options. You can always tinker with them later by using the same menu.

<p align="center">
  <img src="public/setup/new_menu.png" alt="Setup">
</p>

<p align="center">
  <img src="public/setup/new_setup.png" alt="Setup">
</p>

<p align="center">
  <img src="public/setup/new_dependencies.png" alt="Setup">
</p>

## Extra (not required)

The Tale source code is located in `Assets/Scripts/Tale`. The `Assets/Resources` dir contains additional resources, such as the Tale logo (which is used in the splash scene).

There is currently no documentation; there will be one someday.

Here's the outdated manual setup in case you want to see how the old version worked behind the scenes: [old setup guide](https://github.com/deprimus/Tale/blob/master/SETUP.md). Currently, Tale does this setup, and more, completely automatically via editor scripts.

Tale had support for the old Post Processing V2 stack for effects like Bloom. However, as of now, they haven't been updated and therefore no longer work.

If you need support for those, please create an issue and we'll do our best :)

# Story scripts

Story scripts are basically markdown files in a specific format. When you develop a game, you usually write a script for the game story.

The script describes the scenes and dialog. If you write this script in the format shown below, Tale will be able to compile it to C# code
which is compatible with Tale. This gives you a head start, as it handles the boilerplate for you.

```md
...

# Script

## Scene 1

This is a description. It will be inserted as a C# comment.

Character1: This is dialog. It will be compiled to Tale.Dialog()
Character2: Yes.

There is also support for reverb.

Character1: (This dialog will be compiled with reverb: true).

---

Pauses like the one above will compile to Tale.Wait().

## Scene 2

Scenes don't have to be named 'Scene'. If the are, they will be renamed to 'Story', as
to not confuse story scenes with the game scenes.

This scene will be compiled to 'Story_2.cs'.

## End - The last encounter

Any spaces or special characters in the name will be removed or replaced with an underscore.

This scene will be renamed and compiled into 'End_The_last_encounter.cs'
```

## Compiling
If you have a story script, you can compile it to Tale code. Simply open the menu `Tale -> Compile Story` and select the file.

The Tale scripts will be generated under `Assets/Scripts/Scenes`, and you will also see `Dialog.cs` which contains helper methods.

The compiler will simply ignore what isn't under `# Script`, so you don't have to cut your original script to make it compile.

# Design

At its core, Tale is a modular framework that can run various actions (either sequentially or in parallel).
In this context, parallel means executing multiple actions at the same time every frame. All actions are executed on the main thread, so they can interact with the Unity API.

These actions manipulate objects and components in an useful way. For example, the dialog action uses a canvas, some animators and some text components
to provide a very customizable dialog system. All you have to do is to create these objects and to give them to Tale.

The framework provides a queue-driven action system, support for parallel actions, and multiple built-in actions.

# Implementation

Tale was originally designed for Unity 2020. Over the years, it has been updated to work with Unity 6. However, if you need support for a version older than 6 (but >= 2020),  open an issue and we'll do our best :)

# Releases

Tale isn't properly versioned yet. For now, the commit hash represents the version.

However, there are codenames for large milestones:

- classic - June 26th, 2021 ([e90a6c0](https://github.com/deprimus/Tale/commit/e90a6c0c968b2f2ee7ac500ae8e16e2b1a4a4d2a))

# License <a href="https://github.com/deprimus/Tale/blob/master/LICENSE"><img align="right" src="https://img.shields.io/badge/License-MIT-blue.svg" alt="License: MIT" /></a>

**Tale** was developed by the [Deprimus](https://wiki.deprimus.men) members. It is licensed under the [MIT license](https://github.com/deprimus/Tale/blob/master/LICENSE).

Everything present in the [resources](https://github.com/deprimus/Tale/tree/master/resources) directory is UNLICENSED. The Tale developers do not own any files in that directory (except for the README). They are included
for convenience.

The Tale logo in the `Assets/Resources` directory is licensed under MIT.

The Tale splash sound in the `Assets/Resources` directory is part of a song named Return to Darkness by [YouFulca](https://youfulca.com/). The terms of use from [here](https://youfulca.com/en/kiyaku_jp/) apply to this asset.