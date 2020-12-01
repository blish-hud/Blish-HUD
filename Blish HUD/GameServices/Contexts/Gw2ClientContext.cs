namespace Blish_HUD.Contexts {

    public class Gw2ClientContext : Context {

        private static readonly Logger Logger = Logger.GetLogger<Gw2ClientContext>();

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

        private (bool IsMatch, ContextAvailability CdnAvailability) IsStandardClientType(int currentBuildId, out ContextAvailability contextAvailability) {
            contextAvailability = GameService.Contexts.GetContext<CdnInfoContext>().TryGetStandardCdnInfo(out var standardCdnContextResult);

            Logger.Debug("{contextName} ({contextAvailability}) reported the Standard client build ID to be {standardBuildId}.", nameof(CdnInfoContext), contextAvailability, standardCdnContextResult.Value.BuildId);

            return contextAvailability != ContextAvailability.Available
                       ? (false, contextAvailability)
                       : (currentBuildId == standardCdnContextResult.Value.BuildId, contextAvailability);
        }

        private (bool IsMatch, ContextAvailability CdnAvailability) IsChineseClientType(int currentBuildId, out ContextAvailability contextAvailability) {
            contextAvailability = GameService.Contexts.GetContext<CdnInfoContext>().TryGetChineseCdnInfo(out var chineseCdnContextResult);

            Logger.Debug("{contextName} ({contextAvailability}) reported the Chinese client build ID to be {chineseBuildId}.", nameof(CdnInfoContext), contextAvailability, chineseCdnContextResult.Value.BuildId);

            return contextAvailability != ContextAvailability.Available
                       ? (false, contextAvailability)
                       : (currentBuildId == chineseCdnContextResult.Value.BuildId, contextAvailability);
        }

        #endregion

        /// <summary>
        /// [DEPENDS ON: CdnInfoContext, Mumble Link API]
        /// If <see cref="ContextAvailability.Available"/>, returns if the client is the
        /// <see cref="ClientType.Standard"/> client or the <see cref="ClientType.Chinese"/> client.
        /// </summary>
        public ContextAvailability TryGetClientType(out ContextResult<ClientType> contextResult) {
            int currentBuildId;

            if (GameService.Gw2Mumble.IsAvailable) {
                currentBuildId = GameService.Gw2Mumble.Info.BuildId;
            } else {
                contextResult = new ContextResult<ClientType>(ClientType.Unknown, "The Guild Wars 2 Mumble Link API was not available.");
                return ContextAvailability.NotReady;
            }

            var standardClient = IsStandardClientType(currentBuildId, out var _);
            if (standardClient.IsMatch) {
                contextResult = new ContextResult<ClientType>(ClientType.Standard);
                return ContextAvailability.Available;
            }

            var chineseClient = IsChineseClientType(currentBuildId, out var _);
            if (chineseClient.IsMatch) {
                contextResult = new ContextResult<ClientType>(ClientType.Chinese);
                return ContextAvailability.Available;
            }

            if (standardClient.CdnAvailability == ContextAvailability.Available && chineseClient.CdnAvailability == ContextAvailability.Available) {
                contextResult = new ContextResult<ClientType>(ClientType.Unknown, $"The build ID reported by the Mumble Link API ({currentBuildId}) could not be matched against a CDN provided build ID.");
                return ContextAvailability.Failed;
            }

            contextResult = new ContextResult<ClientType>(ClientType.Unknown, $"The CDN context is either not ready or failed to load.");
            return ContextAvailability.Unavailable;
        }

    }
}
