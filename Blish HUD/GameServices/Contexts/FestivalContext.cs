using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Blish_HUD.Properties;
using Gw2Sharp.WebApi.Http;

namespace Blish_HUD.Contexts {

    /// <summary>
    /// Provides information about what festivals are currently active in-game.
    /// </summary>
    public class FestivalContext : Context {

        public static readonly Logger Logger = Logger.GetLogger(typeof(FestivalContext));

        /// <summary>
        /// A Guild Wars 2 festival identified by the <see cref="FestivalContext"/>.
        /// </summary>
        public struct Festival {

            private static FestivalContext _context;

            private readonly string _name;
            private readonly string _displayName;

            /// <summary>
            /// The unique identifying name of the festival.
            /// </summary>
            public string Name => _name;

            /// <summary>
            /// The localized display name of the festival.
            /// </summary>
            public string DisplayName => _displayName;

            public Festival(string name, string displayName) {
                _name        = name;
                _displayName = displayName;
            }

            /// <summary>
            /// Indicates if this festival is currently active.
            /// </summary>
            /// <returns><c>true</c> if the festival is active.</returns>
            public bool IsActive() {
                if (_context == null) {
                    _context = GameService.Contexts.GetContext<FestivalContext>();
                }

                // _context can still be null if it has not registered with the ContextService yet
                return _context?.FestivalIsActive(this) ?? false;
            }

            // Known festivals

            /// <summary>
            /// Guild Wars 2 Halloween Festival.
            /// </summary>
            public static readonly Festival Halloween = new Festival("halloween", Strings.Context_FestivalContext_Halloween);

            /// <summary>
            /// Guild Wars 2 Wintersday Festival.
            /// </summary>
            public static readonly Festival Wintersday = new Festival("wintersday", Strings.Context_FestivalContext_Wintersday);

            /// <summary>
            /// Guild Wars 2 Super Adventure Festival.
            /// </summary>
            public static readonly Festival SuperAdventureFestival = new Festival("superadventurefestival", Strings.Context_FestivalContext_SuperAdventureFestival);

            /// <summary>
            /// Guild Wars 2 Lunar New Year Festival.
            /// </summary>
            public static readonly Festival LunarNewYear = new Festival("lunarnewyear", Strings.Context_FestivalContext_LunarNewYear);

            /// <summary>
            /// Guild Wars 2 Festival of the Four Winds.
            /// </summary>
            public static readonly Festival FestivalOfTheFourWinds = new Festival("festivalofthefourwinds", Strings.Context_FestivalContext_FestivalOfTheFourWinds);

            /// <summary>
            /// Guild Wars 2 Dragon Bash Festival.
            /// </summary>
            public static readonly Festival DragonBash = new Festival("dragonbash", Strings.Context_FestivalContext_DragonBash);

        }

        private const string DAILY_GROUP_ID = "18DB115A-8637-4290-A636-821362A3C4A8";

        private readonly Dictionary<int, Festival> _knownFestivalCategories = new Dictionary<int, Festival> {
            {79, Festival.Halloween},
            {98, Festival.Wintersday},
            {162, Festival.SuperAdventureFestival},
            {201, Festival.LunarNewYear},
            {213, Festival.FestivalOfTheFourWinds},
            {233, Festival.DragonBash}
        };

        private CancellationTokenSource _contextLoadCancellationTokenSource;

        private List<Festival> _activeFestivals = new List<Festival>();

        private string _fault;

        public FestivalContext() {
            GameService.GameIntegration.Gw2Started += GameIntegrationOnGw2Started;
        }

        /// <inheritdoc />
        protected override void Load() {
            _contextLoadCancellationTokenSource?.Dispose();
            _contextLoadCancellationTokenSource = new CancellationTokenSource();

            GetFestivalsFromGw2Api(_contextLoadCancellationTokenSource.Token).ContinueWith((festivals) => SetFestivals(festivals.Result));
        }

        /// <inheritdoc />
        protected override void Unload() {
            _contextLoadCancellationTokenSource.Cancel();
        }

        private void GameIntegrationOnGw2Started(object sender, EventArgs e) {
            // Unload without DoUnload to avoid expiring the context
            this.Unload();

            this.DoLoad();
        }

        private void SetFestivals(IEnumerable<Festival> festivals) {
            _activeFestivals.Clear();

            _activeFestivals = festivals.ToList();

            this.ConfirmReady();
        }

        private async Task<IEnumerable<Festival>> GetFestivalsFromGw2Api(CancellationToken cancellationToken) {
            _fault = null;

            try {
                var dailyAchievementCategories = await GameService.Gw2Api.SharedClient.V2.Achievements.Groups.GetAsync(Guid.Parse(DAILY_GROUP_ID), cancellationToken);

                return dailyAchievementCategories.Categories.Where((category) => _knownFestivalCategories.ContainsKey(category)).Select((category) => _knownFestivalCategories[category]);
            } catch (Exception e) {
                _fault = $"Failed to query Guild Wars 2 API: {e.Message}";

                Logger.Warn(e, "Failed to query Guild Wars 2 API.");

                return Enumerable.Empty<Festival>();
            }
        }

        private bool FestivalIsActive(Festival festival) {
            return _activeFestivals.Contains(festival);
        }

        /// <summary>
        /// If <see cref="ContextAvailability.Available"/>, returns
        /// <see cref="ReadOnlyCollection{Festival}"/> containing a
        /// collection of all currently active festivals.
        /// </summary>
        public ContextAvailability TryGetActiveFestivals(out ContextResult<ReadOnlyCollection<Festival>> contextResult) {
            if (!string.IsNullOrEmpty(_fault)) {
                contextResult = new ContextResult<ReadOnlyCollection<Festival>>(default, _fault);
                return ContextAvailability.Failed;
            }

            if (this.State != ContextState.Ready) return NotReady(out contextResult);

            contextResult = new ContextResult<ReadOnlyCollection<Festival>>(_activeFestivals.AsReadOnly());
            return ContextAvailability.Available;
        }

        /// <summary>
        /// If <see cref="ContextAvailability.Available"/>, returns
        /// a <c>bool</c> indicating if the provided<param name="festival">festival</param>
        /// is currently active or not.
        /// </summary>
        public ContextAvailability TryCheckIfFestivalIsActive(Festival festival, out ContextResult<bool> contextResult) {
            if (!string.IsNullOrEmpty(_fault)) {
                contextResult = new ContextResult<bool>(false, _fault);
                return ContextAvailability.Failed;
            }

            if (this.State != ContextState.Ready) return NotReady(out contextResult);

            contextResult = new ContextResult<bool>(FestivalIsActive(festival));
            return ContextAvailability.Available;
        }

    }

}
