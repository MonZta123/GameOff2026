# GameOff2026

A Unity game made for GitHub Game Off 2026.

## Requirements

- Unity 6000.6.7f1
- [Git LFS](https://git-lfs.com/)
- [Audiokinetic Launcher](https://www.audiokinetic.com/download/) with a free Audiokinetic account
- Wwise 2025.1.12.9287
- Wwise Unity Integration 2025.1.12.4363

## Why the Wwise integration is missing

The Wwise Unity Integration is proprietary Audiokinetic software, so it is not included in this public repository. You need to install it yourself and accept Audiokinetic's license terms.

The repository only contains:

- the `.meta` files of the integration, which keep the asset GUIDs stable
- the project settings in `Assets/Wwise/ScriptableObjects` and `Assets/Wwise/Resources`
- the Wwise project in `GameOff2026_WwiseProject`

## Setup

**Do not open the project in Unity before step 4.** Unity removes `.meta` files without a matching asset, which breaks the Wwise references in the scenes.

1. Clone the repository with Git LFS installed:

   ```
   git lfs install
   git clone https://github.com/MonZta123/GameOff2026.git
   ```

2. In the Audiokinetic Launcher, install Wwise 2025.1.12.9287.
3. In the Launcher, add the cloned folder as a Unity project and integrate Wwise with bundle 2025.1.12.4363.
4. Open the project in Unity 6000.6.7f1.
5. Open `GameOff2026_WwiseProject/GameOff2026_WwiseProject.wproj` in Wwise 2025.1.12 and generate the SoundBanks if none are included yet.

Use exactly this Wwise version. A newer version may upgrade the Wwise project.

## Third-party software

Wwise is © Audiokinetic Inc. and is subject to its own license terms.