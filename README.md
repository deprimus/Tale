<p align="center">
  <img src="public/logo.png" alt="Tale">
</p>

# About <a href="https://unity.com"><img align="right" src="https://img.shields.io/badge/Unity-2020.3.2f1-000000?logo=Unity" alt="Unity 20" /></a>

**Tale** is a prop manipulation utility designed for storytelling. In other words, it's a modular framework that can run various actions (either sequentially or in parallel).
These actions manipulate objects and components in an useful way. For example, the dialog action uses a canvas, some animators and some text components
to provide a very customizable dialog system. All you have to do is to create these objects and to give them to Tale.

Tale was created to ease the development of games, especially story-driven ones. However, it can be used in any type of project.
It was developed for Unity 2020, but should also work with 2021 and newer versions. However, some actions are not guaranteed to work (for example, the post-processing actions may not work
because they rely on Post Processing Stack v2).

The framework provides a queue-driven action system, support for parallel actions, and multiple built-in actions for manipulating:

- Scenes
- Dialog (with support for rich text, avatars, animations like click-to-continue, etc)
- Transitions (based on custom animations)
- Audio (multi-channel sound, music, dialog voice with looping support for Undertale-like voices)
- Camera (including post-processing, zoom, position and rotation)
- Cinematics (videos, subtitles, images with multiple animations like crossfade)
- Transforms (position and rotation)

You can easily extend Tale by creating your own actions. 

# Getting started

In order to be able to use Tale, you need to follow the [setup guide](https://github.com/deprimus/Tale/blob/master/SETUP.md).

# Releases

None.

# License <a href="https://github.com/deprimus/Tale/blob/master/LICENSE"><img align="right" src="https://img.shields.io/badge/License-MIT-blue.svg" alt="License: MIT" /></a>

**Tale** was developed by the [Deprimus](https://wiki.deprimus.men) members. It is licensed under the [MIT license](https://github.com/deprimus/Tale/blob/master/LICENSE).

Everything present in the [resources](https://github.com/deprimus/Tale/tree/master/resources) directory is UNLICENSED. The Tale developers do not own any files in that directory (except for the README). They are included
for convenience.