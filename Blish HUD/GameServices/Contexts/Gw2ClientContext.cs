namespace Blish_HUD.Contexts {

    public class Gw2ClientContext : Context {

        private static readonly Logger Logger = Logger.GetLogger(typeof(Gw2ClientContext));

        /// <summary>
        /// The type of the client currently running.
        /// </summary>
        public enum ClientType {
            /// <summary>
            /// The client type could not be determined.
            /// </summary>
            Unknown,

            /// <summary>
            /// The client is the standard US/EU varient of the client.
            /// </summary>
            Standard,

            /// <summary>
            /// The client is the Chinese varient of the client.
            /// </summary>
            Chinese
        }

        /// <inheritdoc />
        protected override void Load() {
            this.ConfirmReady();
        }

        #region Specific Checks

        private bool IsStandardClientType(int currentBuildId, out ContextAvailability contextAvailability) {
            contextAvailability = GameService.Contexts.GetContext<CdnInfoContext>().TryGetStandardCdnInfo(out var standardCdnContextResult);

            Logger.Debug("{contextName} ({contextAvailability}) reported the Standard client build ID to be {standardBuildId}.", nameof(CdnInfoContext), contextAvailability, standardCdnContextResult.Value.BuildId);

            if (contextAvailability != ContextAvailability.Available)
                return false;

            return currentBuildId == standardCdnContextResult.Value.BuildId;
        }

        private bool IsChineseClientType(int currentBuildId, out ContextAvailability contextAvailability) {
            contextAvailability = GameService.Contexts.GetContext<CdnInfoContext>().TryGetChineseCdnInfo(out var chineseCdnContextResult);

            Logger.Debug("{contextName} ({contextAvailability}) reported the Chinese client build ID to be {chineseBuildId}.", nameof(CdnInfoContext), contextAvailability, chineseCdnContextResult.Value.BuildId);

            if (contextAvailability != ContextAvailability.Available)
                return false;

            return currentBuildId == chineseCdnContextResult.Value.BuildId;
        }

        #endregion

        /// <summary>
        /// [DEPENDS ON: CdnInfoContext, Mumble Link API]
        /// If <see cref="ContextAvailability.Available"/>, returns if the client is the
        /// <see cref="ClientType.Standard"/> client or the <see cref="ClientType.Chinese"/> client.
        /// </summary>
        public ContextAvailability TryGetClientType(out ContextResult<ClientType> contextResult) {
            int currentBuildId;

            if (GameService.Gw2Mumble.Available) {
                currentBuildId = GameService.Gw2Mumble.BuildId;
            } else {
                contextResult = new ContextResult<ClientType>(ClientType.Unknown, "The Guild Wars 2 Mumble Link API was not available.");
                return ContextAvailability.Unavailable;
            }

            if (IsStandardClientType(currentBuildId, out var standardCdnStatus)) {
                contextResult = new ContextResult<ClientType>(ClientType.Standard);
                return ContextAvailability.Available;
            }

            if (IsChineseClientType(currentBuildId, out var chineseCdnStatus)) {
                contextResult = new ContextResult<ClientType>(ClientType.Chinese);
                return ContextAvailability.Available;
            }

            contextResult = new ContextResult<ClientType>(ClientType.Unknown, $"The build ID reported by the Mumble Link API ({currentBuildId}) could not be matched against a CDN provided build ID.");
            return ContextAvailability.Failed;
        }

    }
}
