using System;
using System.IO;
using System.Linq;
using Blish_HUD.Overlay.UI.Views;
using Blish_HUD.Graphics.UI;
using System.ServiceModel.Syndication;
using System.Threading.Tasks;
using System.Xml;
using Flurl.Http;

namespace Blish_HUD.Overlay.UI.Presenters.Widgets {
    public class RssWidgetPresenter : Presenter<RssWidgetView, Uri> {

        private readonly string _feedTitle;

        private SyndicationFeed _feed;

        /// <inheritdoc />
        public RssWidgetPresenter(RssWidgetView view, string feedTitle, Uri feedUri) : base(view, feedUri) {
            _feedTitle = feedTitle;
        }

        /// <inheritdoc />
        protected override async Task<bool> Load(IProgress<string> progress) {
            if (!this.Model.IsWellFormedOriginalString()) return false;

            progress.Report("Requesting feed...");

            using (var feedResponse = await this.Model.AbsoluteUri.GetAsync()) {
                if (!feedResponse.IsSuccessStatusCode) {
                    progress.Report($"Request failed: {feedResponse.ReasonPhrase}");
                    return false;
                }

                progress.Report("Loading feed...");

                using (var stringReader = new StringReader(await feedResponse.Content.ReadAsStringAsync())) {
                    using (var xmlReader = XmlReader.Create(stringReader)) {
                        _feed = SyndicationFeed.Load(xmlReader);
                    }
                }
            }

            if (_feed == null) {
                progress.Report("Failed to read response.");
                return false;
            }

            return true;
        }

        /// <inheritdoc />
        protected override void UpdateView() {
            this.View.Title = _feedTitle ?? _feed.Title.Text;

            this.View.SetFeedEntry1(_feed.Items.ElementAt(0));
            this.View.SetFeedEntry2(_feed.Items.ElementAt(1));
        }

    }
}
