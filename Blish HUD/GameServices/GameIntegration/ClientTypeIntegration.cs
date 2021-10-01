using System;
using Blish_HUD.Contexts;
using Blish_HUD.GameServices;

namespace Blish_HUD.GameIntegration {
    public class ClientTypeIntegration : ServiceModule<GameIntegrationService> {

        private static readonly Logger Logger = Logger.GetLogger<ClientTypeIntegration>();

        public Gw2ClientContext.ClientType ClientType { get; private set; } = Gw2ClientContext.ClientType.Unknown;

        public ClientTypeIntegration(GameIntegrationService service) : base(service) { /* NOOP */ }

        public override void Load() {
            GameService.Gw2Mumble.Info.BuildIdChanged += delegate { DetectClientType(); };

            GameService.Contexts.GetContext<CdnInfoContext>().StateChanged += OnCdnInfoContextStateChanged;
        }

        private void OnCdnInfoContextStateChanged(object sender, EventArgs e) {
            if (((Context)sender).State == ContextState.Ready) {
                DetectClientType();
            }
        }

        private void DetectClientType() {
            var checkClientTypeResult = GameService.Contexts.GetContext<Gw2ClientContext>().TryGetClientType(out var contextResult);

            switch (checkClientTypeResult) {
                case ContextAvailability.Available:
                    this.ClientType = contextResult.Value;
                    Logger.Info("Detected Guild Wars 2 client to be the {clientVersionType} version.", this.ClientType);
                    break;
                case ContextAvailability.Unavailable:
                case ContextAvailability.NotReady:
                    Logger.Debug("Unable to detect current Guild Wars 2 client version: {statusForUnknown}.", contextResult.Status);
                    break;
                case ContextAvailability.Failed:
                    Logger.Warn("Failed to detect current Guild Wars 2 client version: {statusForFailed}", contextResult.Status);
                    break;
            }
        }

    }
}
