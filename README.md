# Blish HUD
[![Build status](https://ci.appveyor.com/api/projects/status/43fg2d3hy4jt5ip1?svg=true)](https://ci.appveyor.com/project/dlamkins/blish-hud)
[![Discord](https://img.shields.io/discord/531175899588984842.svg?logo=discord&logoColor=%237289DA)](https://discord.gg/FYKN3qh)
[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?branch=dev&project=blish-hud_Blish-HUD&metric=alert_status)](https://sonarcloud.io/dashboard?id=blish-hud_Blish-HUD&branch=dev)


## Download Blish HUD

You can download Blish HUD from:
- Our [#💾blish_releases](https://discord.gg/2HKg78n) channel in Discord (which includes the latest modules).
- From our [CI build feed](https://ci.appveyor.com/project/dlamkins/blish-hud/branch/dev/artifacts) (⚠modules may not be compatible with these builds).

*We'll get them releasing straight to GitHub in the near future.*

### Need Help?

Visit our [#💢help](https://discord.gg/qJdUhdG) channel in Discord.

## Links of Interest

- [Mini Wiki](https://github.com/blish-hud/Blish-HUD/wiki) - a mix of a few developer and end user resources.
- [Arcdps BHUD Integration](https://github.com/blish-hud/arcdps-bhud) - a plugin that uses the Arcdps Combat API and exposes some of the data to Blish HUD for compatible modules.
- [Community Module Pack](https://github.com/blish-hud/Community-Module-Pack) - the source for the Community Modules 
*(For module downloads, we recommend downloading from the [#💾blish_releases](https://discord.gg/2HKg78n) channel in Discord)*.

## For Developers

Pull requests are welcome. You are encouraged to join the discussion in the [Blish HUD #🔨core_discussion Discord channel](https://discord.gg/nGbd3kU).

### Build Requirements

#### Prerequisites

- [Visual Studio 2019](https://visualstudio.microsoft.com/vs/)
- [MonoGame 3.7.1](http://community.monogame.net/t/monogame-3-7-1-release/11173)
- [.NET 4.7.1 Developer Pack](https://www.microsoft.com/en-us/download/details.aspx?id=56119)
- [Visual C++ Redistributable 2012](https://www.microsoft.com/en-us/download/details.aspx?id=30679)

#### Instructions

1.  Clone the repo: `git clone https://github.com/blish-hud/Blish-HUD.git`
2.  Launch the project solution in Visual Studio 2019.
3.  Restore NuGet dependencies: `nuget restore`

### Module Development

- [Visual Studio 2019 Module Template](https://github.com/blish-hud/Module-Template)
- [Example Module](https://github.com/blish-hud/Example-Blish-HUD-Module/blob/master/README.md)

### Thanks

Thank you [JetBrains](https://www.jetbrains.com/?from=Blish%20HUD) for providing free open source licenses to our primary contributers.

### License

Licensed under the [MIT License](https://choosealicense.com/licenses/mit/)
