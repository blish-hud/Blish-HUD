using Blish_HUD.Controls.Intern;
namespace Blish_HUD
{
    public interface IKeyboard
    {
        void Press(GuildWarsControls key);
        void Release(GuildWarsControls key);
    }
}