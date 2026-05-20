# Git Workflow

## Git Workflow for 2-Developer XR Multiplayer POC

This workflow is optimized for:

- Unity projects
- Multiplayer/XR development
- Fast integration
- Minimal merge conflicts
- 7-day sprint execution

## Recommended Branch Structure

| Branch | Purpose |
| --- | --- |
| `main` | Stable demo-ready branch |
| `develop` | Daily integration branch |
| `devA/*` | Developer A feature branches |
| `devB/*` | Developer B feature branches |

## Initial Repository Setup

### Step 1 - Create Repository

Repository structure:

- `main`
- `develop`

### Step 2 - Add Unity `.gitignore`

Use the official Unity `.gitignore`.

Must ignore:

- `/Library`
- `/Temp`
- `/Obj`
- `/Build`
- `/Logs`
- `/UserSettings`

### Step 3 - Setup Git LFS

Unity binary files become massive.

Install Git LFS:

```bash
git lfs install
```

Track:

```bash
git lfs track "*.psd"
git lfs track "*.fbx"
git lfs track "*.wav"
git lfs track "*.mp4"
git lfs track "*.anim"
git lfs track "*.controller"
```

Commit:

```bash
git add .gitattributes
git commit -m "Setup Git LFS"
```

## Branching Strategy

### Main Branch

Purpose:

- Always stable

Rules:

- Never commit directly
- Only tested builds merge here
- Used for final demo APK

### Develop Branch

Purpose:

- Daily integration branch

Rules:

- Both developers merge here daily
- Must always compile
- Must always be testable

### Feature Branches

Developer A examples:

- `devA/xr-setup`
- `devA/ui-system`
- `devA/map-interaction`
- `devA/terrain-system`

Developer B examples:

- `devB/networking`
- `devB/avatar-sync`
- `devB/voice-chat`
- `devB/shared-selection`

## Daily Workflow

### Morning Workflow

#### Step 1 - Pull Latest `develop`

Both developers:

```bash
git checkout develop
git pull origin develop
```

#### Step 2 - Create or Update Feature Branch

Create a new branch:

```bash
git checkout -b devA/map-interaction
```

Or continue an existing branch:

```bash
git checkout devA/map-interaction
git pull origin develop
```

### During Development

Commit frequently.

Good practice:

```bash
git add .
git commit -m "Added XR ray interaction"
```

Avoid giant commits.

## Important Unity Rules

### Rule 1 - Force Text Serialization

In Unity:

1. Go to `Edit -> Project Settings -> Editor`
2. Set `Version Control -> Visible Meta Files`
3. Set `Asset Serialization -> Force Text`

This is critical for merge safety.

### Rule 2 - Avoid Same Scene Editing

Only one developer should edit `MainScene.unity` at a time.

### Rule 3 - Use Prefabs Heavily

Instead of editing the scene directly:

- create isolated prefabs
- test independently
- drag them into the scene later

This dramatically reduces conflicts.

## End of Day Integration Workflow

### Step 1 - Commit Your Feature Work

Example:

```bash
git add .
git commit -m "Completed terrain interaction system"
```

### Step 2 - Push Feature Branch

```bash
git push origin devA/map-interaction
```

### Step 3 - Merge Into `develop`

Option 1, recommended for a small team:

```bash
git checkout develop
git pull origin develop
git merge devA/map-interaction
git push origin develop
```

### Step 4 - Other Developer Pulls Latest `develop`

```bash
git checkout develop
git pull origin develop
```

### Step 5 - Integration Testing

Test every day:

- Multiplayer join
- XR launch
- Voice
- Scene loading
- Shared sync
