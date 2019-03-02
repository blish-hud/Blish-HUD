using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.Xna.Framework;

namespace Blish_HUD.Modules.TacO.Origin {
    public class MarkerTypeData {

        public bool NeedsExportToUserData = false;

        public bool IconFileSaved = false;
        public bool SizeSaved = false;
        public bool AlphaSaved = false;
        public bool FadeNearSaved = false;
        public bool FadeFarSaved = false;
        public bool HeightSaved = false;
        public bool BehaviorSaved = false;
        public bool ResetLengthSaved = false;
        public bool ResetOffsetSaved = false;
        public bool AutoTriggerSaved = false;
        public bool HasCountdownSaved = false;
        public bool TriggerRangeSaved = false;
        public bool MinSizeSaved = false;
        public bool MaxSizedSaved = false;
        public bool ColorSaved = false;
        public bool TrailDataSaved = false;
        public bool AnimSpeedSaved = false;
        public bool TextureSaved = false;
        public bool TrailScaleSaved = false;
        public bool ToggleCategorySaved = false;

        public string IconFile;
        public float Size = 1.0f;
        public float Alpha = 1.0f;
        public float FadeNear = -1.0f;
        public float FadeFar = -1.0f;
        public float Height = 1.5f;
        public POIBehavior Behavior = POIBehavior.AlwaysVisible;
        public int ResetLength = 0;
        public int ResetOffset = 0;
        public int AutoTrigger = 0;
        public int HasCountdown = 0;
        public float TriggerRange = 2.0f;
        public int MinSize = 5;
        public int MaxSize = 2048;
        public Color Color = Color.White;
        public string TrailData;
        public float AnimSpeed = 1;
        public string Texture;
        public float TrailScale = 1;
        public string ToggleCategory;

        public void Read(XmlNode n, Boolean storeSaveState) {
            throw new NotImplementedException();
        }

        public void Write(XmlNode n) {
            throw new NotImplementedException();
        }

    }
}
