using System;
using System.Threading.Tasks;
using Flurl.Http;

namespace Blish_HUD.Contexts {

    /// <summary>
    /// Provides build information provided by the asset CDNs.
    /// </summary>
    public class CdnInfoContext : Context {

        private static readonly Logger Logger = Logger.GetLogger<CdnInfoContext>();

        /// <summary>
        /// Structured information provided by one of the asset CDNs.
        /// </summary>
        public struct CdnInfo {

            private readonly int _buildId;
            private readonly int _exeFileId;
            private readonly int _exeFileSize;
            private readonly int _manifestFileId;
            private readonly int _manifestFileSize;

            public int BuildId          => _buildId;
            public int ExeFileId        => _exeFileId;
            public int ExeFileSize      => _exeFileSize;
            public int ManifestFileId   => _manifestFileId;
            public int ManifestFileSize => _manifestFileSize;

            public CdnInfo(int buildId, int exeFileId, int exeFileSize, int manifestFileId, int manifestFileSize) {
                _buildId          = buildId;
                _exeFileId        = exeFileId;
                _exeFileSize      = exeFileSize;
                _manifestFileId   = manifestFileId;
                _manifestFileSize = manifestFileSize;
            }

            public static CdnInfo Invalid => new CdnInfo(-1, -1, -1, -1, -1);

        }

        private const int TOTAL_CDN_ENDPOINTS = 2;

        private const string GW2_ASSETCDN_URL    = "http://assetcdn.101.arenanetworks.com/latest/101";
        private const string GW2_CN_ASSETCDN_URL = "http://assetcdn.111.cgw2.com/latest/111";

        private CdnInfo _standardCdnInfo;
        private CdnInfo _chineseCdnInfo;

        private int _loadCount = 0;

        #region Context Management

        public CdnInfoContext() {
            GameService.GameIntegration.Gw2Started += GameIntegrationOnGw2Started;
        }

        /// <inheritdoc />
        protected override void Load() {
            GetCdnInfoFromCdnUrl(GW2_ASSETCDN_URL).ContinueWith((cdnInfo) => SetCdnInfo(ref _standardCdnInfo,   cdnInfo.Result));
            GetCdnInfoFromCdnUrl(GW2_CN_ASSETCDN_URL).ContinueWith((cdnInfo) => SetCdnInfo(ref _chineseCdnInfo, cdnInfo.Result));
        }

        /// <inheritdoc />
        protected override void Unload() {
            _loadCount = 0;
        }

        private void GameIntegrationOnGw2Started(object sender, EventArgs e) {
            // Unload without DoUnload to avoid expiring the context
            this.Unload();

            this.DoLoad();
        }

        private CdnInfo ParseCdnInfo(string rawCdnResponse) {
            if (string.IsNullOrEmpty(rawCdnResponse)) {
                Logger.Warn("Failed to parse null or empty CDN response.");
                return CdnInfo.Invalid;
            }

            string[] cdnVars = rawCdnResponse.Split(' ');

            if (cdnVars.Length == 5) {
                bool parsedSuccessfully = true;

                parsedSuccessfully &= int.TryParse(cdnVars[0], out int buildId);
                parsedSuccessfully &= int.TryParse(cdnVars[1], out int exeFileId);
                parsedSuccessfully &= int.TryParse(cdnVars[2], out int exeFileSize);
                parsedSuccessfully &= int.TryParse(cdnVars[3], out int manifestFileId);
                parsedSuccessfully &= int.TryParse(cdnVars[4], out int manifestFileSize);
                
                if (parsedSuccessfully) {
                    return new CdnInfo(buildId, exeFileId, exeFileSize, manifestFileId, manifestFileSize);
                }

                Logger.Warn("Failed to parse CDN response {rawCdnResponse}.", rawCdnResponse);
                return CdnInfo.Invalid;
            }

            Logger.Warn("Unexpected number of values provided by CDN response {rawCdnResponse}.", rawCdnResponse);
            return CdnInfo.Invalid;
        }

        private void SetCdnInfo(ref CdnInfo cdnInfo, string result) {
            cdnInfo = ParseCdnInfo(result);

            if (++_loadCount >= TOTAL_CDN_ENDPOINTS) {
                ConfirmReady();
            }
        }

        private async Task<string> GetCdnInfoFromCdnUrl(string cdnUrl) {
            try {
                return await cdnUrl.GetStringAsync();
            } catch (FlurlHttpException ex) {
                if (ex.Call.Response != null) {
                    Logger.Warn(ex, "Failed to get CDN information from {cdnUrl}.  HTTP response status was ({httpStatusCode}) {statusReason}.", cdnUrl, (int)ex.Call.Response.StatusCode, ex.Call.Response.ReasonPhrase);
                } else {
                    Logger.Warn(ex, "Failed to get CDN information from {cdnUrl}.  No response was received.", cdnUrl);
                }
            } catch (Exception ex) {
                Logger.Warn(ex, "Failed to get CDN information from {cdnUrl}.  An unexpected exception occurred.", cdnUrl);
            }

            return null;
        }

        #endregion

        private ContextAvailability TryGetCdnInfo(ref CdnInfo cdnInfo, out ContextResult<CdnInfo> contextResult) {
            if (this.State != ContextState.Ready) return NotReady(out contextResult);

            if (cdnInfo.BuildId > 0) {
                contextResult = new ContextResult<CdnInfo>(cdnInfo);
                return ContextAvailability.Available;
            }

            if (cdnInfo.BuildId < 0) {
                contextResult = new ContextResult<CdnInfo>(cdnInfo, "Failed to determine build ID from CDN.");
                return ContextAvailability.Failed;
            }

            contextResult = new ContextResult<CdnInfo>(cdnInfo, "Build ID has not been requested from the CDN.");
            return ContextAvailability.Unavailable;
        }

        /// <summary>
        /// If <see cref="ContextAvailability.Available"/>, returns
        /// <see cref="CdnInfo"/> provided by the standard asset CDN.
        /// </summary>
        public ContextAvailability TryGetStandardCdnInfo(out ContextResult<CdnInfo> contextResult) {
            return TryGetCdnInfo(ref _standardCdnInfo, out contextResult);
        }

        /// <summary>
        /// If <see cref="ContextAvailability.Available"/>, returns
        /// <see cref="CdnInfo"/> provided by the Chinese asset CDN.
        /// </summary>
        public ContextAvailability TryGetChineseCdnInfo(out ContextResult<CdnInfo> contextResult) {
            return TryGetCdnInfo(ref _chineseCdnInfo, out contextResult);
        }

    }

}