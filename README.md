<p align="center">
  <img src="https://litstage-images.s3.us-west-1.amazonaws.com/litstage_icon_green.png" alt="LitStage Logo" width="120" />
</p>

# LitStage Desktop_02

LitStage VR Classroom is an educational game designed to teach students different subjects using immersive VR games.  
Our goal is to create interactive, engaging learning experiences that make education fun and accessible through virtual reality.

---

# Requirements

- **Disk space:** Approximately 22 GB free
- **Unity 6:** <a href="https://unity.com/download" target="_blank">Download here</a>
- **Meta Quest headset:** Quest 3S or Quest 3
- **Meta Quest Link cable:** <a href="https://www.meta.com/us/es/quest/accessories/link-cable/?srsltid=AfmBOop8C9rl6ylWoMuKIhxQGdXsvsKxB_Ugqp3GsWJr-nE6z92YgiXl" target="_blank">Official Link Cable</a>
- **GitHub Desktop:** <a href="https://github.com/apps/desktop" target="_blank">Download here</a>
- **Meta Horizon app:** <a href="https://horizon.meta.com" target="_blank">Download here</a>



# Project Information

- **Unity Version:** 6.0.0 (Build: 6000.0.53f1)
- **Project Template:** VR using OpenXR

---

# 📁 Getting Started

### Unity Installation

1. Download Unity from the official website <a href="https://unity.com/download" target="_blank">Download here</a>
2. After installing, open **Unity Hub**.
3. Click on **Install Editor**.
4. Select the version: `6000.0.53f1`.
5. Choose the following modules:
    - Visual Studio Code
    - Android Build Support
    - OpenJDK
    - Android SDK & NDK Tools
    - Web Build Support

Note: you need to have enough space

---

### ⚡️ Cloning the Project

> **Tip:** Create a specific folder on your machine to keep all Litstage projects organized.

You can clone this repository using either **Git commands** or the **GitHub Desktop app**.

#### Option 1: Using Git Commands

<a href="https://www.youtube.com/shorts/_bzS2dwz-IM" target="_blank">Video tutorial</a>

1. Open a terminal:  
   - On Windows: **Command Prompt (cmd)**
   - On Mac: **Terminal**
2. Navigate to your chosen folder:
    ```sh
    cd myUser/MyDocuments/thisIsAnExample/litstageFolder
    ```
3. Execute the following command:
    ```sh
    git clone https://github.com/XRealityAcademy/litstage_vr.git
    ```
4. Enter your GitHub username and password if prompted.
5. If everything goes well, you’ll see the folder with the project in your directory.

#### **Option 2: Using the GitHub Web UI**

<a href="https://www.youtube.com/shorts/lZf1nrT6iZw" target="_blank">Video tutorial</a>

1.	Go to <a href="https://github.com/XRealityAcademy/litstage_vr.git" target="_blank">https://github.com/XRealityAcademy/litstage_vr.git</a>
2.	Click the green Code button.
3.	Select the HTTPS option and click the copy icon.
4.	Open the GitHub Desktop application.
5.	Go to File → Clone repository and paste the URL.
6.	Select a folder where you want to save the project.

---

# ⚡️ Unity Configuration

1. **Open Unity Hub and import the project:**
    - Click the **Add** button.
    - Select **Add project from disk** and choose your project folder.
2. **Open the project.**
3. **Switch build platform:**
    - Go to **File** → **Build Profiles**.
    - In the left menu, select **Android** and click **Switch Platform**.
4. **Scene List:**
    - By default, SCN_Tutorial example scenes are selected.  
      If this is your first time, keep this configuration. You can change scenes later as needed.
5. **Enable XR Plug-in Management:**
    - Go to **File** → **Project Settings** → **XR Plug-in Management**.
    - Make sure **OpenXR** is checked.
6. **Configure OpenXR Profiles:**
    - Go to **File** → **Project Settings** → **XR Plug-in Management** .
    - Make sure the OpenXR option is checked

---

# Meta Quest Configuration

### Meta Horizon Configuration

- Enable Developer Mode on Meta Quest 3 
  <a href="https://www.youtube.com/watch?v=8WxK8QeaEIc" target="_blank">Video tutorial (starts at 0:37)</a>
- Connect your Meta Quest 3 to your laptop with the Meta Quest Link cable

---

# ▶️ Run the Game

> **IMPORTANT:** Play the game in a room with enough space. Make sure your play area is clear to avoid any accidents or damage.

1. Go to **File** → **Build Profiles** → **Android**.
2. In the **Open Scene List**, for your first build, keep the default scenes. This lets you review the OpenXR example components. Later, you can select only your own scenes.
3. In **Platform Settings**, set **Run Device** to **Oculus Quest 3** (or the name of your connected Quest device).
4. Click **Build and Run**.
    - The first time, you’ll be prompted to save the build. Save it as `litstage_classroom`.
5. Wait for Unity to build and deploy the app to your headset. Once finished, put on your Quest and play!
6. Each time you select an example in the dashboard, wait for some seconds, don't click to fast
7. After exploring the OpenXR example scenes, try running only your scene:
    - Deselect all example scenes and select only your own scene(s) in the scene list before building.

**Notes:**
- If the first time you don't see any scene loaded in your Hierarchy, please open the Assets-> Production -> Scenes -> SCN_Tutorial
- The first scene includes OpenXR example content. 
- For future builds, use the shortcut:  
    - **Windows:** `Ctrl + B`  
    - **Mac:** `Command + B`

---

# 📁 Folder Structure

This project uses a clear folder structure to keep work organized and make collaboration easy. Here’s how it works:

#### Top-Level Folders
* Production
    
    Only final, approved assets, code, and scenes for the official game build.
    Only leads or integrators should move things here.

* Shared

    Temporary space for work that’s ready for team review or testing.
    When your work is ready for others to check, move it here. Leads will review and move it to Production folder after approval.

* [Your Name]

    Each team member has a personal workspace (e.g., Oliver/, Jacob/, etc.).
    Use your folder for drafts, experiments, or features in progress. Move work to Shared/ when it’s ready for review.


```
Assets/
├── Production/                  # Only approved, final game content (art, code, scenes)
│   ├── Art/
│   │   ├── Textures/
│   │   ├── Images/
│   │   ├── Sprites/
│   │   ├── Models/
│   │   ├── Materials/
│   │   ├── Animations/
│   │   └── Audio/
│   ├── Core/
│   │   ├── Prefabs/
│   │   └── Scripts/
│   └── Scenes/
│       ├── Tutorial.unity
│       └── Lab.unity
│
├── Shared/                      # For work ready for team review/testing
│   ├── Art/
│   │   ├── Textures/
│   │   ├── Images/
│   │   ├── Sprites/
│   │   ├── Models/
│   │   ├── Materials/
│   │   ├── Animations/
│   │   └── Audio/
│   ├── Prefabs/
│   └── Scripts/
│
├── Oliver/                      # Oliver's personal workspace
│   ├── Art/
│   │   ├── Textures/
│   │   ├── Images/
│   │   ├── Sprites/
│   │   ├── Models/
│   │   ├── Materials/
│   │   ├── Animations/
│   │   └── Audio/
│   ├── Prefabs/
│   ├── Scripts/
│   └── Scenes/
│
├── Jacob/                       # Jacob's personal workspace
│   ├── Art/
│   │   ├── Textures/
│   │   ├── Images/
│   │   ├── Sprites/
│   │   ├── Models/
│   │   ├── Materials/
│   │   ├── Animations/
│   │   └── Audio/
│   ├── Prefabs/
│   ├── Scripts/
│   └── Scenes/
│
├── Dominique/                   # Dominique's personal workspace
│   ├── Art/
│   │   ├── Textures/
│   │   ├── Images/
│   │   ├── Sprites/
│   │   ├── Models/
│   │   ├── Materials/
│   │   ├── Animations/
│   │   └── Audio/
│   ├── Prefabs/
│   ├── Scripts/
│   └── Scenes/
│
└── Edwin/                       # Edwin's personal workspace
    ├── Art/
    │   ├── Textures/
│   │   ├── Images/
    │   ├── Sprites/
    │   ├── Models/
    │   ├── Materials/
    │   ├── Animations/
    │   └── Audio/
    ├── Prefabs/
    ├── Scripts/
    └── Scenes/


```


Workflow Summary:
* Work in your personal folder.
* When ready for review, move your work to Shared/.
* After approval, leads/integrators will move approved content to Production folder

---

## 🚦 Git Flow

Our team works using branches for each developer.  
- Start your work in your own branch (Edwin, Dominique, Oliver, Jacob).
- Always **pull the latest changes from `main`** before starting new work.
- When your feature is ready, **create a Pull Request (PR) to `main`**.
- All code is **reviewed before merging** into `main`.

![Git flow diagram](https://litstage-images.s3.us-west-1.amazonaws.com/git-flow.png)

**Quick Steps:**
1. Pull from `main` to your branch before working.
2. Work on your branch.
3. Create a PR to `main` when ready.
4. Team reviews your PR before merging.

> If you’re new to git, follow this flow to avoid conflicts and keep our project organized!



---
# 🚀 GIT Daily Commands

> ### ⚡️ **Pull often!**
> The longer you go without syncing, the worse merge conflicts get.


### ✅ Commit Changes

- Watch this excellent video guide (**from 2:29 to 3:00 only**):  
  <a href="https://www.youtube.com/watch?v=g2XjJhrGGg4" target="_blank">How to Commit in Git</a>
- Always write a clear description of what you changed.
- **Tip:** Commit your changes often—even small working changes!  
  This way, if something goes wrong, we can easily roll back to a previous version.


### 🔄 Update Your Branch with the Latest Changes from Main

- **ALWAYS commit your local changes** (with/without pushing) before updating your branch.
- Watch this excellent video guide (**until minute 1:55**):  
  
  <a href="https://www.youtube.com/watch?v=Btu0SuwPmz0" target="_blank">How to update your branch with main</a>


### 🚩 Create a Pull Request

1. Open the GitHub website and go to your repository.
2. Click the **Pull requests** tab.
3. Click **New pull request**.
4. On the left side, set the target (usually `main`).  
   On the right side, select your branch.
5. Click **Create pull request**.
6. Add a description of your changes (e.g., new models, textures, animations, bug fixes, etc.).
7. Ask to review the pull before merge it to the main branch


> **Remember:**  
> - Commit often  
> - Keep your branch up to date  
> - PR when ready and always describe your work!

---




## 🎮 Industry Naming Conventions for Game Assets

| Asset Type     | Prefix  | Example Name                     | Notes/Versioning                   |
|----------------|---------|----------------------------------|------------------------------------|
| Prefab         | P_      | P_PlayerCharacter                |                                    |
| Model (Mesh)   | M_      | M_DeskWood, M_DeskWood_v2        | Add `_v2`, `_v3` for new versions  |
| Static Mesh    | SM_     | SM_PropsCrate, SM_Car_v2         | Used in Unreal, optional for Unity |
| Texture        | T_      | T_WallBrick_ALB                  | Use suffix for type (see below)    |
| Material       | MAT_    | MAT_WallBrick                    |                                    |
| Script         | (none)  | PlayerManager.cs                 | PascalCase, no prefix (Unity)      |
| Animation      | A_      | A_EnemyRunCycle                  |                                    |
| Audio          | AU_     | AU_ButtonClick                   |                                    |
| Sprite         | SP_     | SP_HealthIcon                    |                                    |
| Scene          | SCN_    | SCN_Lab, SCN_Tutorial            |                                    |
| **Image/Logo/Icon** | **Logo_** / **Icon_** | Logo_LitStage_Green, Icon_LitStage | For branding, UI, or app icon     |

### **Texture Suffixes:**
| Suffix   | Meaning           | Example                |
|----------|-------------------|------------------------|
| _ALB     | Albedo/Diffuse    | T_WallBrick_ALB        |
| _NRM     | Normal Map        | T_WallBrick_NRM        |
| _RMA     | Rough/Met/AO      | T_WallBrick_RMA        |
| _EMI     | Emission          | T_WallLamp_EMI         |
| _OCC     | Occlusion         | T_WallBrick_OCC        |

---

### **Versioning Assets**
- If you create an updated version of an asset (like a model), add a suffix:  
  - `M_DeskWood_v2` (new version, keep the old for reference or rollback)
  - `P_EnemyRobot_v3`
- Use whole numbers (`_v2`, `_v3`) for **major revisions**.

---

#### **Examples**

- Old model: `M_DoorIron`
- New test version: `M_DoorIron_v2`
- Prefab for new version: `P_DoorIron_v2`
- Texture for new version: `T_DoorIron_ALB_v2`

## 📄 License

MIT License © 2025 Litstage

---
