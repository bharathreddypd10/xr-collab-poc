# AGENTS.md

This file provides project-level context and working rules for `/home/uquest/Learning/XR POC`.
Future Codex chats working in this folder should treat this as the primary local context document and stay aligned with it.

## Project Summary

- Project name: `XR POC`
- Project type: Unity XR multiplayer proof of concept
- Primary goal: build a rapid multiplayer XR demo focused on collaborative interaction
- Delivery style: practical POC work, optimized for correctness, simplicity, and fast integration over heavy architecture

## Source Context Provided By User

The following user-provided documents were reviewed and their key points are summarized here:

- `/home/uquest/Documents/XR-POC-DOC.pdf`
- `/home/uquest/Documents/XR POC Git Workflow.pdf`

Those documents describe:

- a 7-day sprint plan for a multiplayer XR demo
- a phased approach: XR setup -> networking -> multiplayer presence -> voice -> shared terrain/map -> collaborative point selection -> polish
- a 2-developer execution model with parallel responsibilities
- a Git workflow intended to reduce Unity merge conflicts during a short sprint

## Current Repo Reality

- Engine: Unity `6000.0.64f1`
- Pipeline: Universal Render Pipeline
- XR stack includes:
  - `com.unity.xr.interaction.toolkit` `3.3.0`
  - `com.unity.xr.openxr` `1.16.0`
  - `com.unity.xr.meta-openxr` `2.3.0`
  - `com.unity.xr.androidxr-openxr` `1.1.0`
  - `com.unity.xr.hands` `1.7.2`
  - `com.unity.xr.arfoundation` `6.3.1`
- Multiplayer and services currently present:
  - `com.unity.netcode.gameobjects` `2.7.0`
  - `com.unity.multiplayer.playmode` `1.6.2`
  - `com.unity.multiplayer.tools` `2.2.6`
  - `com.unity.services.multiplayer` `1.1.8`
  - `com.unity.services.vivox` `16.8.0`
  - `com.unity.services.authentication` `3.5.2`

Important note:

- The repo directory currently does not appear to be initialized as a Git repository from this workspace view, even though a Git workflow document exists. Do not assume Git commands will work unless `.git` is present.

## Working Interpretation Of The POC

Unless the user says otherwise, assume this project is aiming for:

- a demoable XR collaboration prototype, not a production-finished product
- two-user or small-group multiplayer behavior
- immersive interaction first, feature completeness second
- practical iteration with visible progress every day

## Expected Feature Direction

When helping on this project, prioritize work that supports or fits into these areas:

- XR environment and device setup
- hand/controller interaction
- avatar/head/hand presence synchronization
- networking stability and ownership handling
- spatial or real-time voice communication
- shared terrain, map, or tabletop visualization
- collaborative point selection, markers, highlights, or annotations
- demo readiness, bug fixing, and polish

## Team Split Context

The planning document uses this responsibility split:

- Developer A: XR, interaction, UI, visual behavior
- Developer B: networking, voice, sync, session stability

Codex should respect that split when proposing tasks, branches, or localized changes. If a task crosses both areas, prefer the smallest integration-safe implementation.

## Unity Collaboration Rules

Follow these assumptions unless the user overrides them:

- keep changes localized and easy to review
- prefer prefab-based or isolated asset work over broad scene churn
- avoid unnecessary edits to the same scene from multiple directions
- match existing Unity project patterns before introducing new structure
- do not add dependencies unless clearly needed
- preserve meta files and Unity serialization safety

When advising on project settings, prefer these Unity collaboration conventions:

- Version Control: Visible Meta Files
- Asset Serialization: Force Text

## Git Workflow Context

If Git is enabled later, align guidance with the user-provided workflow:

- `main`: stable, demo-ready branch
- `develop`: daily integration branch
- `devA/*`: Developer A feature branches
- `devB/*`: Developer B feature branches

Workflow intent:

- never commit directly to `main`
- integrate into `develop` regularly
- keep daily integration buildable and testable
- commit small, focused changes
- avoid giant scene conflicts

If asked to help with Git workflow, prefer advice that reduces Unity merge conflicts:

- one developer edits a shared scene at a time
- use prefabs heavily
- keep features isolated until ready to integrate

## How Codex Should Behave In This Repo

In future chats for this folder, Codex should:

- read relevant files before editing
- choose the safest and best implementation
- favor beginner-friendly readability
- avoid overengineering
- avoid inventing missing APIs, credentials, or service setup
- state uncertainty clearly
- validate changes with the most relevant available checks
- mention what was and was not verified

## User Preferences To Preserve

- communication should be concise, clear, and implementation-focused
- practical options should be preferred over theoretical ones
- important decisions should be explained simply
- avoid overwhelming the user with too many alternatives
- preserve existing work and avoid unrelated file churn
- keep edits localized and easy to review

## File Modification Preference

The user has asked that Codex always ask before modifying files.

Interpretation for this repo:

- if the user explicitly asks for a file to be created or updated, that counts as permission for the requested change
- otherwise, before editing files, ask for confirmation

## Response Checklist For Future Tasks

After completing work, responses should include:

1. what changed
2. why it changed
3. files affected
4. how to test or validate it
5. any follow-up or limitations
