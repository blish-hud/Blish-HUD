﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.BitmapFonts;

namespace Blish_HUD.Controls {
    public class Label : LabelBase {

        /// <summary>
        /// The text this <see cref="Label"/> should show.
        /// </summary>
        public string Text {
            get => _text;
            set => SetProperty(ref _text, value, true);
        }

        /// <summary>
        /// The font the <see cref="Text"/> will be rendered in.
        /// </summary>
        public BitmapFont Font {
            get => _font;
            set => SetProperty(ref _font, value, true);
        }

        /// <summary>
        /// The color of the <see cref="Text"/>.
        /// </summary>
        public Color TextColor {
            get => _textColor;
            set => SetProperty(ref _textColor, value);
        }

        public Utils.DrawUtil.HorizontalAlignment HorizontalAlignment {
            get => _horizontalAlignment;
            set => SetProperty(ref _horizontalAlignment, value);
        }

        public Utils.DrawUtil.VerticalAlignment VerticalAlignment {
            get => _verticalAlignment;
            set => SetProperty(ref _verticalAlignment, value);
        }

        /// <summary>
        /// If enabled, a 1px offset shadow will be applied behind the rendered text.
        /// </summary>
        public bool ShowShadow {
            get => _showShadow;
            set => SetProperty(ref _showShadow, value, true);
        }

        /// <summary>
        /// If enabled, a stroke effect will be applied to the text to make it more visible.
        /// <see cref="ShadowColor"/> will set the color of the stroke.
        /// </summary>
        public bool StrokeText {
            get => _strokeText;
            set => SetProperty(ref _strokeText, value, true);
        }

        /// <summary>
        /// If either <see cref="ShowShadow"/> or <see cref="StrokeText"/> is enabled, they will
        /// be drawn in this color.
        /// </summary>
        public Color ShadowColor {
            get => _shadowColor;
            set => SetProperty(ref _shadowColor, value);
        }

        /// <summary>
        /// If enabled, the <see cref="Control.Width"/> of this control will change to match the width of the text.
        /// </summary>
        public bool AutoSizeWidth {
            get => _autoSizeWidth;
            set => SetProperty(ref _autoSizeWidth, value);
        }

        /// <summary>
        /// If enabled, the <see cref="Control.Height"/> of this control will change to match the height of the text.
        /// </summary>
        public bool AutoSizeHeight {
            get => _autoSizeHeight;
            set => SetProperty(ref _autoSizeHeight, value);
        }

        public Label() : base() {
            _cacheLabel = false;
        }

    }
}
