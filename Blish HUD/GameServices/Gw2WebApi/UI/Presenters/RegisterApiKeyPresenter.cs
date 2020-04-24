using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Blish_HUD.Graphics.UI;
using Blish_HUD.Gw2WebApi.UI.Views;

namespace Blish_HUD.Gw2WebApi.UI.Presenters {
    public class RegisterApiKeyPresenter : Presenter<RegisterApiKeyView, Gw2WebApiService> {

        private Dictionary<string, string> _keyRepository;

        public RegisterApiKeyPresenter(RegisterApiKeyView view, Gw2WebApiService model) : base(view, model) { /* NOOP */ }

        protected override void UpdateView() {
            base.UpdateView();
        }

    }
}
