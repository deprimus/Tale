<p align="center">
  <img src="public/logo.png" alt="Tale">
</p>

# Setup

This document describes the setup process for [Tale](https://github.com/deprimus/Tale), a storytelling and prop manipulation utility for Unity.

After following the setup process, you may start using Tale. Reading the [documentation]() is highly recommended for beginner users.

This guide includes images meant to aid you. If they are too small, you can click on them to view them at their original resolution.

This setup assumes you have at least minimal knowledge about Unity, the entity component system (ECS), and the C# programming language.
You need to know what components are and how to add them to objects. The optional setup may require additional knowledge.

The setup is split into 2 parts: required and optional. The former describes how to set up the Tale core, while the latter describes
how to set up optional modules.

## Prerequisites

You will need a new/existing Unity 2D project. For the purpose of this guide, a new project will be used.

Please note that Tale was developed for Unity 2020.3.2f1. It is not guaranteed to work with any other versions.

You will also need the Tale source code.

### Downloading Tale

If you want to use a Tale release, download the release archive (none as of now).

If you want to use the latest Tale version, clone the repository (or download as zip).

## Required Setup

This section contains all necessary steps required to get Tale up and running. This section is mandatory and cannot be skipped.

Open your Unity project and make sure you have the Tale source code handy. The setup steps are as follows:

### 1. Copy the Tale scripts

In your project, if the `Assets/Scripts` directory doesn't exist, create it. Inside it, create another directory named `Tale`.

Go to the Tale source code, navigate inside the `src` folder, and copy everything to the `Assets/Scripts/Tale` folder.
This path is not imposed, but recommended.

You know you did everything right if the `Tale` directory looks something like this:

<p align="center">
  <img src="public/setup/tale_dir.png" alt="The Tale directory">
</p>

After copying the files, you no longer need to have the Tale source code handy.

At this point, you may see an error: `The type or namespace name 'PostProcessing' does not exist...`. It will disappear
once you follow the next step.

### 2. Install the required dependencies

Tale relies on TextMesh Pro and PostProcessing. The former is usually already installed by Unity, and is only required for other
optional modules. The latter has to be manually installed and is required in order for Tale to work.

Go to `Window -> Package Manager`. At the top-right of the package manager window, to the left of the `+` icon, click on the `Packages`
dropdown box and select `Unity Registry`.

Search for `Post Processing` at the top left of the window. Click on the package that appears (it should be named `Post Processing`),
then click on the `Install` button at the bottom left of the window.

<p align="center">
  <img src="public/setup/postprocessing_install.png" alt="Installing Post Processing">
</p>

> Note: in the future, the PostProcessing package will only be required if camera effects are enabled. For now, you have to install it even if you don't
> intend to use such effects.

### 3. Create the Tale Master object

The master object is the heart of Tale, and is responsible for single-handedly orchestrating all of the actions. If this object is disabled,
Tale will cease to function.

In your default scene, create a new empty game object. Call it `Tale Master` and add the `TaleMaster.cs` script to it. Make sure the object
is at the top of the hierarchy.

<p align="center">
  <img src="public/setup/tale_master_obj.png" alt="Tale Master object">
</p>

<p align="center">
  <img src="public/setup/tale_master_component.png" alt="Tale Master object">
</p>

You know you did everything right if the master looks something like this:

<p align="center">
  <img src="public/setup/tale_master_overview.png" alt="Tale Master overview">
</p>

Everything listed there is used by the optional modules. Details about them can be found in the [optional setup]() section.

> Note: the Tale master object can only be instantiated once. If any other objects with the TaleMaster component appear in the future,
> they will be automatically deleted. If you delete the master object at runtime, you will not be able to instantiate a new one.
> This prevents duplicate masters from being active at the same time.

### 4. Create the master prefab

In order for Tale to work properly, the master object must be present in every scene. Duplicate master objects will be automatically
deleted at runtime. This way, Tale can function regardless of the scene from which you start the game.

You may skip this step if you wish to add optional modules. However, don't forget to come back here after you finish setting everything up.

In your project, navigate to the `Assets/Prefabs` directory (create it if needed). Drag the master object into the prefab folder.

It should look like this:

<p align="center">
  <img src="public/setup/tale_master_prefab.png" alt="Tale Master prefab">
</p>

After doing so, drag the prefab into any existing/future scenes, and make sure that the master object always sits at the top of the hierarchy.

If you want to modify the master object (e.g. to add optional modules), you should modify the prefab. This is the reason why it is recommended to
finish setting up Tale before creating the prefab.

### 5. Test

If you want to see if Tale works properly, create a game object and attach the following script to it:

```cs
using UnityEngine;

public class TaleTest : MonoBehaviour
{
    void Start()
    {
        Tale.Exec(() => Debug.Log("Tale works."));
    }
}

```

Make sure to name the script `TaleTest.cs`. Press `Play` and check if `Tale works.` is logged. If so, Tale was correctly set up. You may now add optional
modules by following the optional setup.