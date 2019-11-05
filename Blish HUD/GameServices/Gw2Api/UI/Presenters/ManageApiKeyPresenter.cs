using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Blish_HUD.Graphics.UI;
using Blish_HUD.Gw2Api.UI.Views;

namespace Blish_HUD.Gw2Api.UI.Presenters {
    public class ManageApiKeyPresenter : Presenter<ManageApiKeyView, Gw2ApiService> {

        private Dictionary<string, string> _keyRepository;

        /// <inheritdoc />
        public ManageApiKeyPresenter(ManageApiKeyView view, Gw2ApiService model) : base(view, model) { /* NOOP */ }

        /// <inheritdoc />
        protected override Task<bool> Load(IProgress<string> progress) {
            _keyRepository = this.Model.GetKeyIdRepository();

            return base.Load(progress);
        }

        /// <inheritdoc />
        protected override void UpdateView() {
            if (_keyRepository.Any()) {
                foreach (KeyValuePair<string, string> item in _keyRepository) {
                    this.View.KeySelectionDropdown.Items.Add(item.Key);
                }
            } else {
                //this.View.KeySelectionDropdown.Visible = false;
            }
        }

    }
}
