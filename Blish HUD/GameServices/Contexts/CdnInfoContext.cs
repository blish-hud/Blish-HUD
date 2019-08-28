using System;
using System.Threading.Tasks;
using Flurl.Http;

namespace Blish_HUD.Contexts {

    public class CdnInfoContext : Context {

        private static readonly Logger Logger = Logger.GetLogger(typeof(CdnInfoContext));
        
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

            public CdnInfo(string rawCdnResponse) : this() {
                string[] cdnVars = rawCdnResponse.Split(' ');

                bool parsedSuccessfully = true;

                if (cdnVars.Length == 5 || (parsedSuccessfully = false)) {
                    parsedSuccessfully &= int.TryParse(cdnVars[0], out _buildId);
                    parsedSuccessfully &= int.TryParse(cdnVars[1], out _exeFileId);
                    parsedSuccessfully &= int.TryParse(cdnVars[2], out _exeFileSize);
                    parsedSuccessfully &= int.TryParse(cdnVars[3], out _manifestFileId);
                    parsedSuccessfully &= int.TryParse(cdnVars[4], out _manifestFileSize);
                }

                if (!parsedSuccessfully) {
                    _buildId = -1;
                    Logger.Warn("Failed to parse CDN response {rawCdnResponse}.", rawCdnResponse);
                }
            }

            public CdnInfo(int buildId, int exeFileId, int exeFileSize, int manifestFileId, int manifestFileSize) {
                _buildId          = buildId;
                _exeFileId        = exeFileId;
                _exeFileSize      = exeFileSize;
                _manifestFileId   = manifestFileId;
                _manifestFileSize = manifestFileSize;
            }

        }

        private const int TOTAL_CDN_ENDPOINTS = 2;

        private const string GW2_ASSETCDN_URL    = "http://assetcdn.101.arenanetworks.com/latest/101";
        private const string GW2_CN_ASSETCDN_URL = "http://assetcdn.111.cgw2.com/latest/111";

        private CdnInfo _standardCdnInfo;
        private CdnInfo _chineseCdnInfo;

        private int _loadCount = 0;

        #region Context Loading

        protected override void Load() {
            GetCdnInfoFromCdnUrl(GW2_ASSETCDN_URL).ContinueWith((cdnInfo) => SetCdnInfo(ref _standardCdnInfo, cdnInfo.Result));
            GetCdnInfoFromCdnUrl(GW2_CN_ASSETCDN_URL).ContinueWith((cdnInfo) => SetCdnInfo(ref _chineseCdnInfo, cdnInfo.Result));
        }

        private void SetCdnInfo(ref CdnInfo cdnInfo, string result) {
            if (string.IsNullOrEmpty(result)) return;

            cdnInfo = new CdnInfo(result);

            if (++_loadCount >= TOTAL_CDN_ENDPOINTS) {
                ConfirmReady();
            }
        }

        private async Task<string> GetCdnInfoFromCdnUrl(string cdnUrl) {
            try {
                return await cdnUrl.GetStringAsync();
            } catch (Exception ex) {
                Logger.Warn(ex, "Failed to get CDN information from {cdnUrl}.", cdnUrl);
            }

            return null;
        }

        #endregion

        private ContextAvailability TryGetCdnInfo(ref CdnInfo cdnInfo, out ContextResult<CdnInfo> contextResult) {
            if (!this.Ready) return NotReady(out contextResult);

            if (cdnInfo.BuildId > 0) {
                contextResult = new ContextResult<CdnInfo>(cdnInfo, true);
                return ContextAvailability.Available;
            }

            if (cdnInfo.BuildId < 0) {
                contextResult = new ContextResult<CdnInfo>(cdnInfo, true);
                return ContextAvailability.Failed;
            }

            contextResult = new ContextResult<CdnInfo>(cdnInfo, false);
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