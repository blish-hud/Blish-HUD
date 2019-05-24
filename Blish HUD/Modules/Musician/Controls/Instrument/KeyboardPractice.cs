using System;
using System.Collections.Generic;
using Blish_HUD.Controls.Intern;
namespace Blish_HUD.Modules.Musician.Controls.Instrument
{
    public class KeyboardPractice : IKeyboard
    {

        private Conveyor Conveyor;
        public KeyboardPractice()
        {
            GameService.GameIntegration.FocusGw2();
            Conveyor = new Conveyor(){ Parent = ContentService.Graphics.SpriteScreen };
        }

        public void Press(GuildWarsControls key){}

        public void Release(GuildWarsControls key){}
    }
}