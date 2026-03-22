# RimWorld Headless Patch Mod

A utility mod designed to facilitate running RimWorld in a fully headless environment. This patch intercepts and modifies game initialization processes that typically require a graphical interface, preventing crashes when running the game on servers without a display or virtual framebuffer.

This tool is specifically built to enable automated CI/CD pipelines, integration testing (such as for API mods), and robust dedicated server management.

## Features
* **Headless Execution:** Safely bypasses UI and rendering initializations that cause crashes in non-GUI environments.
* **CI/CD Ready:** Ideal for running automated integration tests for your RimWorld mods via GitHub Actions, GitLab CI, or self-hosted VPS runners.
* **Multi-Version Support:** Targets RimWorld versions 1.5 and 1.6.

## Getting Started

### Installation
1. Download the latest release `.zip` from the [Releases](../../releases) page.
2. Extract the contents into your RimWorld `Mods` folder.
3. Enable `HeadlessRimPatch` in your mod list (or via your headless startup arguments/configs).

### For Developers (Building from Source)
This project uses modern `.NET` and the `Krafs.Rimworld.Ref` NuGet packages to pull game assemblies automatically. You do not need to commit copyrighted game DLLs to build this project.

```bash
# Clone the repository
git clone [https://github.com/YourUsername/HeadlessRimPatch.git](https://github.com/YourUsername/HeadlessRimPatch.git)

# Navigate to the source folder
cd HeadlessRimPatch/Source

# Build the project
dotnet build HeadlessRim.csproj -c Release-1.6
```

## Legal & Licensing

GNU General Public License v3.0 This project is open-source software licensed under the GNU GPLv3. See the LICENSE file for full details.

## Disclaimer & Copyright
This is an unofficial community mod. All rights to the RimWorld game, its assets, and its underlying code belong to Tynan Sylvester and Ludeon Studios. This project is not affiliated with, endorsed by, or sponsored by Ludeon Studios.
