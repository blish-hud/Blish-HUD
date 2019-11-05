using System;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Xml;
using Blish_HUD.Controls;
using Blish_HUD.Input;
using Blish_HUD.Overlay.UI.Presenters.Widgets;
using Blish_HUD.Overlay.UI.Views.Widgets;
using Humanizer;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Blish_HUD.Overlay.UI.Views {
    public class RssWidgetView : HomeWidgetView<RssWidgetPresenter> {

        #region Load Static

        private static readonly Texture2D _textureArrowRight;
        private static readonly Texture2D _textureArrowRightHover;

        static RssWidgetView() {
            _textureArrowRight      = GameService.Content.GetTexture(@"common\255390");
            _textureArrowRightHover = GameService.Content.GetTexture(@"common\255391");
        }

        #endregion

        public RssWidgetView(string feedUri) : this(null, feedUri) { /* NOOP */ }

        public RssWidgetView(string feedTitle, string feedUri) {
            this.Presenter = new RssWidgetPresenter(this, feedTitle, new Uri(feedUri));
        }

        /// <inheritdoc />
        protected override void Build(Panel buildPanel) {
            base.Build(buildPanel);
        }

        public void SetFeedEntry1(SyndicationItem feedItem) {
            var entryPanel1 = new Panel() {
                Location = new Point(30,  40),
                Size     = new Point(400, 64),
                Parent   = this.ViewTarget
            };

            BuildEntryPanel(entryPanel1, feedItem);
        }

        public void SetFeedEntry2(SyndicationItem feedItem) {
            var entryPanel2 = new Panel() {
                Location = new Point(30,  129),
                Size     = new Point(400, 64),
                Parent   = this.ViewTarget
            };

            BuildEntryPanel(entryPanel2, feedItem);
        }

        private void BuildEntryPanel(Panel entryPanel, SyndicationItem feedItem) {
            var authorImage = new Image(GameService.Content.GetTexture(@"temp\forum-avatar")) {
                Size            = new Point(64, 64),
                BackgroundColor = Color.White * 0.7f,
                Parent          = entryPanel
            };

            var arrowButton = new GlowButton() {
                Icon       = _textureArrowRight,
                ActiveIcon = _textureArrowRightHover,
                Size       = new Point(64,                    64),
                Location   = new Point(entryPanel.Width - 64, 0),
                Parent     = entryPanel
            };

            var postTitle = new Label() {
                Font           = GameService.Content.DefaultFont16,
                Width          = arrowButton.Left - authorImage.Right - 24,
                AutoSizeHeight = true,
                Text           = $"{feedItem.Title.Text}",
                StrokeText     = true,
                Location       = new Point(authorImage.Right + 12, authorImage.Top),
                WrapText       = true,
                Parent         = entryPanel
            };

            var postMetadata = new Label() {
                Font           = GameService.Content.DefaultFont14,
                AutoSizeHeight = true,
                AutoSizeWidth  = true,
                Text           = $"Posted {feedItem.PublishDate.Humanize()}",
                StrokeText     = true,
                Parent         = entryPanel
            };

            entryPanel.Click += delegate {
                if (feedItem.Links.Count > 0) {
                    Process.Start(feedItem.Links[0].Uri.AbsoluteUri);
                }
            };

            postMetadata.Location = new Point(authorImage.Right + 12, authorImage.Bottom - postMetadata.Height);
        }

    }
}
