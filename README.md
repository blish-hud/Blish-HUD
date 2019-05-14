# Blish HUD

_A more structured readme will come in the future._

- Visual Studio 2019 is preferred at the moment (if not, you'll need to downgrade Fody, which isn't such a big deal)
- Install MonoGame 3.7.1: http://community.monogame.net/t/monogame-3-7-1-release/11173
- Make sure you have .NET 4.7.1 Developer Pack: https://www.microsoft.com/en-us/download/details.aspx?id=56119

I included MonoGame.Extended content pipeline dependency straight in the Content folder with a reference already set, so you shouldn't have to worry about that anymore.

Place graphics into the new "ref" folder found in the project directory.  When Blish HUD builds, it will auto build a ref.dat and include it in the output directory for you.