# XR Collab POC

Unity XR multiplayer proof of concept for collaborative interaction, rapid iteration, and demo-focused development.

## Overview

This project is a practical XR multiplayer POC built in Unity. The current repo is set up around a short sprint workflow, with focus on:

- XR environment setup and interaction
- multiplayer presence and synchronization
- voice-ready collaboration flow
- shared environment or tabletop interaction
- simple, demo-safe iteration

## Tech Stack

| Area | Technology | Why |
| --- | --- | --- |
| Game Engine | Unity Technologies Unity 6000.0.64f1 | Required for MR Template |
| Rendering Pipeline | URP (Universal Render Pipeline) | Best Quest performance |
| XR Runtime | OpenXR | Industry standard XR runtime |
| XR SDK | Meta XR SDK | Native Quest integration |
| XR Interaction | XR Interaction Toolkit | Fast XR interaction development |
| Input System | Unity Input System | XR controller input handling |
| Networking | Photon Fusion | Best for fast XR multiplayer |
| Voice Chat | Photon Voice | Easy spatial voice integration |
| Session Management | Fusion Shared Mode | Simplifies room synchronization |
| Player Sync | Fusion NetworkTransform | XR head/hand synchronization |
| Base Template | MR Multiplayer Tabletop Template | Fastest MR foundation |
| Hand Tracking | Meta Hand Tracking | Natural MR interaction |
| Passthrough | Meta Passthrough API | Mixed Reality experience |
| Spatial Anchors | Meta Spatial Anchors (Optional) | Shared table alignment |
| Terrain Streaming | Cesium | Large-scale terrain support |
| Mapping | Mapbox | Lightweight mapping integration |
| Visualization Type | Simplified terrain tiles | Faster POC implementation |
| Interaction | XR Ray Interaction | Shared map selection |
| Language | C# | Unity native |
| IDE | Microsoft Visual Studio / Rider | Unity debugging |
| Version Control | Git + Git LFS | Unity asset management |
| Repository Hosting | GitHub | Team collaboration |
| Package Management | Unity Package Manager | Dependency management |

## Main Scenes

- `Assets/_Project/Scenes/TabletopCollabScene.unity`

## Project Structure

- `.gitattributes`
- `.gitignore`
- `AGENTS.md`
- `Assets/`
  - `Editor/`
  - `Plugins/`
  - `Resources/`
  - `StreamingAssets/`
  - `ThirdParty/`
    - `MRTabletopAssets/`
    - `XRMP/`
  - `_Project/`
    - `Animations/`
    - `Audio/`
    - `Materials/`
    - `Models/`
    - `Prefabs/`
    - `Scenes/`
      - `TabletopCollabScene.unity`
    - `Scripts/`
    - `ScriptableObjects/`
    - `Shaders/`
    - `Textures/`
  - `CompositionLayers/`
  - `TextMesh Pro/`
  - `XR/`
  - `XRI/`
- `Packages/`
  - `manifest.json`
  - `packages-lock.json`
- `ProjectSettings/`
  - `ProjectVersion.txt`
- `XR POC Git Workflow.md`

## Working Notes

- `Assets/` contains the main game, XR, multiplayer, and imported Unity content.
- `Packages/manifest.json` defines package dependencies for XR, networking, and services.
- `ProjectSettings/` contains Unity project configuration and should stay under version control.
- `.gitignore` excludes Unity-generated local folders and IDE artifacts.
- `.gitattributes` tracks large binary asset types through Git LFS.

## Git Workflow

This repo follows the documented branching model:

- `main` for stable demo-ready code
- `develop` for daily integration
- `devA/*` for XR, interaction, and UI work
- `devB/*` for networking, sync, and voice work

See [XR POC Git Workflow.md](/home/uquest/Learning/XR POC/XR POC Git Workflow.md) for the detailed workflow.
