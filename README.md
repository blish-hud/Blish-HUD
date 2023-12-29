# Blish HUD
[![Build status](https://ci.appveyor.com/api/projects/status/43fg2d3hy4jt5ip1?svg=true)](https://ci.appveyor.com/project/dlamkins/blish-hud/branch/dev/artifacts)
[![Discord](https://img.shields.io/discord/531175899588984842.svg?logo=discord&logoColor=%237289DA)](https://discord.gg/FYKN3qh)
[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?branch=dev&project=blish-hud_Blish-HUD&metric=alert_status)](https://sonarcloud.io/dashboard?id=blish-hud_Blish-HUD&branch=dev)

**Visit our site:** https://blishhud.com

## Check our Setup Guide

[![Blish HUD Setup Video](https://img.youtube.com/vi/iLYYumF2SCY/0.jpg)](https://www.youtube.com/watch?v=iLYYumF2SCY)

## Download Blish HUD

You can download Blish HUD:
- Using the [direct download (v1.1.1)](https://github.com/blish-hud/Blish-HUD/releases/download/v1.1.1/Blish.HUD.1.1.1.zip) link.
- From our [Releases](https://github.com/blish-hud/Blish-HUD/releases) page here on GitHub.

### Need Help?

Visit our [#💢help](https://discord.gg/qJdUhdG) channel in Discord.

## Links of Interest

### Blish HUD Resources

- [BlishHUD.com](https://blishhud.com/) - Our website.
- [Blish HUD FAQ](https://blishhud.com/docs/user/faq) - Frequently asked questions.
- [Troubleshooting Guide](https://blishhud.com/docs/user/troubleshooting/) - Our troubleshooting guide.
- [Arcdps Blish HUD Integration](https://github.com/blish-hud/arcdps-bhud) - a plugin that uses the Arcdps Combat API and exposes some of the data to Blish HUD for compatible modules.

### Marker Pack Support
- [Pathing Module Setup Guide](https://blishhud.com/docs/markers/) - Video and written guide for using the pathing module for TacO marker packs.
- [Marker Pack Development](https://blishhud.com/docs/markers/development/attributes) - Details on the marker pack format along with attribute support.

## For Developers

Pull requests are welcome. You are encouraged to join the discussion in the [Blish HUD #🔨core_discussion Discord channel](https://discord.gg/nGbd3kU).

### Build Requirements

#### Prerequisites

- [Visual Studio 2022](https://visualstudio.microsoft.com/vs/)
- Individual Components that need to be installed in Visual Studio via the Visual Studio Installer:
  - .NET Core 3.1 Runtime 
  - .NET Framework 4.7.2 SDK
  - .NET Framework 4.7.2 targeting pack
  - Visual C++ Redistributable 2012

#### Instructions

1.  Clone the repo: `git clone -v --recurse-submodules --progress  https://github.com/blish-hud/Blish-HUD.git`
2.  Launch the project solution (.sln file) in Visual Studio 2022.
3.  In the Solution Explorer right click on the solution icon. In the context menu click "Restore NuGet Packages".
4.  Right click the solution icon again and click "Build Solution". 

### Module Development

- [Visual Studio 2019 Module Template](https://github.com/blish-hud/Module-Template)
- [Module Documentation](https://blishhud.com/docs/dev/)
- [Example Module](https://github.com/blish-hud/Example-Blish-HUD-Module/blob/master/README.md)

### Thanks

Thank you [JetBrains](https://www.jetbrains.com/?from=Blish%20HUD) for providing free open source licenses to our primary contributers.

### License

Licensed under the [MIT License](https://choosealicense.com/licenses/mit/)
