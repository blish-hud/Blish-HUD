using System;

namespace Blish_HUD.ArcDps {
    /// <summary>
    /// ArcDps Enums and helpful extension methods.
    /// </summary>
    /// <remarks>
    /// Source: <see cref="https://github.com/baaron4/GW2-Elite-Insights-Parser/blob/master/GW2EIEvtcParser/ParserHelpers/ArcDPSEnums.cs"/>
    /// (MIT License)
    /// </remarks>
    public static class ArcDpsIdCollection {

        public const int ArcDPSPollingRate = 300;

        internal static class GW2Builds {
            internal const ulong StartOfLife = ulong.MinValue;
            //
            internal const ulong HoTRelease = 54485;
            internal const ulong November2016NightmareRelease = 69591;
            internal const ulong February2017Balance = 72781;
            internal const ulong May2017Balance = 76706;
            internal const ulong July2017ShatteredObservatoryRelease = 79873;
            internal const ulong December2017Balance = 84832;
            internal const ulong February2018Balance = 86181;
            internal const ulong May2018Balance = 88541;
            internal const ulong July2018Balance = 90455;
            internal const ulong August2018Balance = 92069;
            internal const ulong October2018Balance = 92715;
            internal const ulong November2018Rune = 93543;
            internal const ulong December2018Balance = 94051;
            internal const ulong March2019Balance = 95535;
            internal const ulong April2019Balance = 96406;
            internal const ulong June2019RaidRewards = 97235;
            internal const ulong July2019Balance = 97950;
            internal const ulong July2019Balance2 = 98248;
            internal const ulong October2019Balance = 99526;
            internal const ulong December2019Balance = 100690;
            internal const ulong February2020Balance = 102321;
            internal const ulong February2020Balance2 = 102389;
            internal const ulong July2020Balance = 104844;
            internal const ulong September2020SunquaPeakRelease = 106277;
            internal const ulong May2021Balance = 115190;
            internal const ulong May2021BalanceHotFix = 115728;
            internal const ulong June2021Balance = 116210;
            internal const ulong EODBeta1 = 118697;
            internal const ulong EODBeta2 = 119939;
            internal const ulong EODBeta3 = 121168;
            internal const ulong EODBeta4 = 122479;
            internal const ulong March2022Balance = 126520;
            internal const ulong March2022Balance2 = 127285;
            internal const ulong May2022Balance = 128773;
            internal const ulong June2022Balance = 130910;
            internal const ulong June2022BalanceHotFix = 131084;
            internal const ulong July2022FractalInstabilitiesRework = 131720;
            internal const ulong August2022BalanceHotFix = 132359;
            internal const ulong August2022Balance = 133322;
            internal const ulong October2022Balance = 135242;
            internal const ulong October2022BalanceHotFix = 135930;
            internal const ulong November2022Balance = 137943;
            internal const ulong February2023Balance = 141374;
            internal const ulong May2023Balance = 145038;
            internal const ulong May2023BalanceHotFix = 146069;
            internal const ulong June2023Balance = 147734;
            internal const ulong SOTOBetaAndSilentSurfNM = 147830;
            internal const ulong July2023BalanceAndSilentSurfCM = 148697;
            internal const ulong SOTOReleaseAndBalance = 150431;
            internal const ulong September2023Balance = 151966;
            internal const ulong DagdaNMHPChangedAndCMRelease = 153978;
            //
            internal const ulong EndOfLife = ulong.MaxValue;
        }

        internal static class ArcDPSBuilds {
            internal const int StartOfLife = int.MinValue;
            //
            internal const int ProperConfusionDamageSimulation = 20210529;
            internal const int ScoringSystemChange = 20210800; // was somewhere around there
            internal const int DirectX11Update = 20210923;
            internal const int InternalSkillIDsChange = 20220304;
            internal const int BuffAttrFlatIncRemoved = 20220308;
            internal const int FunctionalIDToGUIDEvents = 20220709;
            internal const int NewLogStart = 20221111;
            internal const int FunctionalEffect2Events = 20230719;
            internal const int BuffExtensionBroken = 20230905;
            internal const int BuffExtensionOverstackValueChanged = 20231107;
            //
            internal const int EndOfLife = int.MaxValue;
        }

        public static class WeaponSetIDs {
            public const int NoSet = -1;
            public const int FirstLandSet = 4;
            public const int SecondLandSet = 5;
            public const int FirstWaterSet = 0;
            public const int SecondWaterSet = 1;
            public const int TransformSet = 3;
            public const int KitSet = 2;
        }

        /// <summary>
        /// Class containing <see cref="int"/> reward types.
        /// </summary>
        internal static class RewardTypes {
            internal const int OldRaidReward1 = 55821; // On each kill
            internal const int OldRaidReward2 = 60685; // On each kill
            internal const int CurrentRaidReward = 22797; // Once per week
            internal const int PostEoDStrikeReward = 29453;
        }

        /// <summary>
        /// Class containing <see cref="ulong"/> reward IDs.
        /// </summary>
        internal static class RewardIDs {
            internal const ulong ShiverpeaksPassChests = 993; // Three chests, once a day
            internal const ulong KodansOldAndCurrentChest = 1035; // Old repeatable chest, now only once a day
            internal const ulong KodansCurrentChest1 = 1028; // Current, once a day
            internal const ulong KodansCurrentChest2 = 1032; // Current, once a day
            internal const ulong KodansCurrentRepeatableChest = 1091; // Current, repeatable
            internal const ulong FraenirRepeatableChest = 1007;
            internal const ulong BoneskinnerRepeatableChest = 1031;
            internal const ulong WhisperRepeatableChest = 1052;
        }

        // Buff cycle
        public enum BuffCycle : byte {
            Cycle, // damage happened on tick timer
            NotCycle, // damage happened outside tick timer (resistable)
            NotCycle_NoResit, // BEFORE MAY 2021: the others were lumped here, now retired
            NotCycle_DamageToTargetOnHit, // damage happened to target on hiting target
            NotCycle_DamageToSourceOnHit, // damage happened to source on hiting target
            NotCycle_DamageToTargetOnStackRemove, // damage happened to target on source losing a stack
            Unknown
        };

        internal static BuffCycle GetBuffCycle(byte bt) {
            return bt < (byte)BuffCycle.Unknown ? (BuffCycle)bt : BuffCycle.Unknown;
        }

        // Breakbar State

        public enum BreakbarState : byte {
            Active = 0,
            Recover = 1,
            Immune = 2,
            None = 3,
            Unknown
        };
        internal static BreakbarState GetBreakbarState(int value) {
            return value < (int)BreakbarState.Unknown ? (BreakbarState)value : BreakbarState.Unknown;
        }

        // Buff Formula

        // this enum is updated regularly to match the in game enum. The matching between the two is simply cosmetic, for clarity while comparing against an updated skill defs
        public enum BuffStackType : byte {
            StackingConditionalLoss = 0, // the same thing as Stacking
            Queue = 1,
            StackingTargetUniqueSrc = 2, // This one clearly behaves like an intensity buff (multiple stack actives, any instance can be extended), always come with a stack limit of 999. It is unclear at this time what differentiate this one from the traditional Stacking type.
            Regeneration = 3,
            Stacking = 4,
            Force = 5,
            Unknown,
        };
        internal static BuffStackType GetBuffStackType(byte bt) {
            return bt < (byte)BuffStackType.Unknown ? (BuffStackType)bt : BuffStackType.Unknown;
        }

        public enum BuffAttribute : short {
            None = 0,
            Power = 1,
            Precision = 2,
            Toughness = 3,
            Vitality = 4,
            Ferocity = 5,
            Healing = 6,
            Condition = 7,
            Concentration = 8,
            Expertise = 9,
            Armor = 10,
            Agony = 11,
            StatInc = 12,
            FlatInc = 13,
            PhysInc = 14,
            CondInc = 15,
            PhysRec = 16,
            CondRec = 17,
            AttackSpeed = 18,
            UnusedSiphonInc_Arc = 19, // Unused due to being auto detected by the solver
            SiphonRec = 20,
            //
            Unknown = short.MaxValue,
            //
            /*ConditionDurationIncrease = 24,
            RetaliationDamageOutput = 25,
            CriticalChance = 26,
            PowerDamageToHP = 34,
            ConditionDamageToHP = 35,
            GlancingBlow = 47,
            ConditionSkillActivationFormula = 52,
            ConditionDamageFormula = 54,
            ConditionMovementActivationFormula = 55,
            EnduranceRegeneration = 61,
            IncomingHealingEffectiveness = 65,
            OutgoingHealingEffectivenessFlatInc = 68,
            OutgoingHealingEffectivenessConvInc = 70,
            RegenerationHealingOutput = 71,
            ExperienceFromKills = 76,
            GoldFind = 77,
            MovementSpeed = 78,
            KarmaBonus = 87,
            SkillCooldown = 96,
            MagicFind = 92,
            ExperienceFromAll = 100,
            WXP = 112,*/
            // Custom Ids, matched using a very simple pattern detection, see BuffInfoSolver.cs
            ConditionDurationInc = -1,
            DamageFormulaSquaredLevel = -2,
            CriticalChance = -3,
            StrikeDamageToHP = -4,
            ConditionDamageToHP = -5,
            GlancingBlow = -6,
            SkillActivationDamageFormula = -7,
            DamageFormula = -8,
            MovementActivationDamageFormula = -9,
            EnduranceRegeneration = -10,
            HealingEffectivenessRec = -11,
            HealingEffectivenessFlatInc = -12,
            HealingEffectivenessConvInc = -13,
            HealingOutputFormula = -14,
            ExperienceFromKills = -15,
            GoldFind = -16,
            MovementSpeed = -17,
            KarmaBonus = -18,
            SkillRechargeSpeedIncrease = -19,
            MagicFind = -20,
            ExperienceFromAll = -21,
            WXP = -22,
            SiphonInc = -23,
            PhysRec2 = -24,
            CondRec2 = -25,
            BoonDurationInc = -26,
            HealingEffectivenessRec2 = -27,
            MovementSpeedStacking = -28,
            MovementSpeedStacking2 = -29,
            FishingPower = -30,
            MaximumHP = -31,
            VitalityPercent = -32,
            DefensePercent = -33,
        }
        internal static BuffAttribute GetBuffAttribute(short bt, int evtcVersion) {
            BuffAttribute res;
            if (evtcVersion >= ArcDPSBuilds.BuffAttrFlatIncRemoved) {
                // Enum has shifted by -1
                if (bt <= (byte)BuffAttribute.SiphonRec - 1) {
                    // only apply +1 shift to enum higher or equal to the one removed
                    res = bt < (byte)BuffAttribute.FlatInc ? (BuffAttribute)(bt) : (BuffAttribute)(bt + 1);
                } else {
                    res = BuffAttribute.Unknown;
                }
            } else {
                res = bt <= (byte)BuffAttribute.SiphonRec ? (BuffAttribute)bt : BuffAttribute.Unknown;
            }
            if (res == BuffAttribute.UnusedSiphonInc_Arc) {
                res = BuffAttribute.Unknown;
            }
            return res;
        }

        // Broken
        /*
        public enum BuffCategory : byte
        {
            Boon = 0,
            Any = 1,
            Condition = 2,
            Food = 4,
            Upgrade = 6,
            Boost = 8,
            Trait = 11,
            Enhancement = 13,
            Stance = 16,
            Unknown = byte.MaxValue
        }
        internal static BuffCategory GetBuffCategory(byte bt)
        {
            return Enum.IsDefined(typeof(BuffCategory), bt) ? (BuffCategory)bt : BuffCategory.Unknown;
        }*/

        // Content local

        public enum ContentLocal : byte {
            Effect = 0,
            Marker = 1,
            Unknown
        }
        internal static ContentLocal GetContentLocal(byte bt) {
            return bt < (byte)ContentLocal.Unknown ? (ContentLocal)bt : ContentLocal.Unknown;
        }

        // Custom ids
        private const int DummyTarget = -1;
        private const int HandOfErosion = -2;
        private const int HandOfEruption = -3;
        private const int PyreGuardianProtect = -4;
        private const int PyreGuardianStab = -5;
        private const int PyreGuardianRetal = -6;
        private const int QadimLamp = -7;
        private const int AiKeeperOfThePeak2 = -8;
        private const int MatthiasSacrifice = -9;
        private const int BloodstoneFragment = -10;
        private const int BloodstoneShardMainFight = -11;
        private const int ChargedBloodstone = -12;
        private const int PyreGuardianResolution = -13;
        private const int CASword = -14;
        private const int SubArtsariiv = -15;
        private const int DummyMaiTrinStrike = -16;
        private const int TheDragonVoidZhaitan = -17;
        private const int TheDragonVoidSooWon = -18;
        private const int TheDragonVoidKralkatorrik = -19;
        private const int TheDragonVoidMordremoth = -20;
        private const int TheDragonVoidJormag = -21;
        private const int TheDragonVoidPrimordus = -22;
        private const int PushableVoidAmalgamate = -23;
        private const int DragonBodyVoidAmalgamate = -24;
        private const int VentariTablet = -25;
        private const int PoisonMushroom = -26;
        private const int SpearAggressionRevulsion = -27;
        private const int DragonOrb = -28;
        private const int ChestOfSouls = -29;
        private const int ShackledPrisoner = -30;
        private const int DemonicBond = -31;
        private const int BloodstoneShardRift = -32;
        private const int BloodstoneShardButton = -33;
        private const int SiegeChest = -34;
        private const int Mine = -35;
        private const int Environment = -36;
        private const int FerrousBomb = -37;
        private const int SanctuaryPrism = -38;
        private const int Torch = -39;
        private const int BoundIcebroodElemental = -40;
        private const int CAChest = -41;
        private const int ChestOfDesmina = -42;
        public const int NonIdentifiedSpecies = 0;

        //

        public enum TrashID : int {
            // Mordremoth
            SmotheringShadow = 15640,
            Canach = 15501,
            Braham = 15778,
            Caithe = 15565,
            BlightedRytlock = 15999,
            //BlightedCanach = 15999,
            BlightedBraham = 15553,
            BlightedMarjory = 15572,
            BlightedCaithe = 15916,
            BlightedForgal = 15597,
            BlightedSieran = 15979,
            //BlightedTybalt = 15597,
            //BlightedPaleTree = 15597,
            //BlightedTrahearne = 15597,
            //BlightedEir = 15597,
            Glenna = 15014,
            // VG
            Seekers = 15426,
            RedGuardian = 15433,
            BlueGuardian = 15431,
            GreenGuardian = 15420,
            // Gorse
            ChargedSoul = 15434,
            EnragedSpirit = 16024,
            AngeredSpirit = 16005,
            // Sab
            Kernan = 15372,
            Knuckles = 15404,
            Karde = 15430,
            BanditSapper = 15423,
            BanditThug = 15397,
            BanditArsonist = 15421,
            // Slothasor
            Slubling1 = 16064,
            Slubling2 = 16071,
            Slubling3 = 16077,
            Slubling4 = 16104,
            PoisonMushroom = ArcDpsIdCollection.PoisonMushroom,
            // Trio
            BanditSaboteur = 16117,
            Warg = 7481,
            VeteranTorturedWarg = 16129,
            BanditAssassin = 16067,
            BanditAssassin2 = 16113,
            BanditSapperTrio = 16074,
            BanditDeathsayer = 16076,
            BanditDeathsayer2 = 16080,
            BanditBrawler = 16066,
            BanditBrawler2 = 16119,
            BanditBattlemage = 16093,
            BanditBattlemage2 = 16100,
            BanditCleric = 16101,
            BanditCleric2 = 16060,
            BanditBombardier = 16138,
            BanditSniper = 16065,
            NarellaTornado = 16092,
            OilSlick = 16096,
            InsectSwarm = 16120,
            Prisoner1 = 16056,
            Prisoner2 = 16103,
            // Matthias
            Spirit = 16105,
            Spirit2 = 16114,
            IcePatch = 16139,
            Storm = 16108,
            Tornado = 16068,
            MatthiasSacrificeCrystal = MatthiasSacrifice,
            // Escort
            MushroomSpikeThrower = 16219,
            MushroomKing = 16255,
            MushroomCharger = 16224,
            WhiteMantleBattleMage1Escort = 16229,
            WhiteMantleBattleMage2Escort = 16240,
            WhiteMantleBattleCultist1 = 16265,
            WhiteMantleBattleCultist2 = 16281,
            WhiteMantleBattleKnight1 = 16242,
            WhiteMantleBattleKnight2 = 16220,
            WhiteMantleBattleCleric1 = 16272,
            WhiteMantleBattleCleric2 = 16266,
            WhiteMantleBattleSeeker1 = 16288,
            WhiteMantleBattleSeeker2 = 16256,
            WargBloodhound = 16222,
            RadiantMcLeod = 16234,
            CrimsonMcLeod = 16241,
            Mine = ArcDpsIdCollection.Mine,
            // KC
            Olson = 16244,
            Engul = 16274,
            Faerla = 16264,
            Caulle = 16282,
            Henley = 16236,
            Jessica = 16278,
            Galletta = 16228,
            Ianim = 16248,
            KeepConstructCore = 16261,
            GreenPhantasm = 16237,
            InsidiousProjection = 16227,
            UnstableLeyRift = 16277,
            RadiantPhantasm = 16259,
            CrimsonPhantasm = 16257,
            RetrieverProjection = 16249,
            // Twisted Castle
            HauntingStatue = 16247,
            //CastleFountain = 32951,
            // Xera
            BloodstoneShardMainFight = ArcDpsIdCollection.BloodstoneShardMainFight,
            BloodstoneShardRift = ArcDpsIdCollection.BloodstoneShardRift,
            BloodstoneShardButton = ArcDpsIdCollection.BloodstoneShardButton,
            ChargedBloodstone = ArcDpsIdCollection.ChargedBloodstone,
            BloodstoneFragment = ArcDpsIdCollection.BloodstoneFragment,
            XerasPhantasm = 16225,
            WhiteMantleSeeker1 = 16238,
            WhiteMantleSeeker2 = 16283,
            WhiteMantleKnight1 = 16251,
            WhiteMantleKnight2 = 16287,
            WhiteMantleBattleMage1 = 16221,
            WhiteMantleBattleMage2 = 16226,
            ExquisiteConjunction = 16232,
            FakeXera = 16289,
            // MO
            Jade = 17181,
            // Samarog
            Guldhem = 17208,
            Rigom = 17124,
            SpearAggressionRevulsion = ArcDpsIdCollection.SpearAggressionRevulsion,
            // Deimos
            Saul = 17126,
            ShackledPrisoner = ArcDpsIdCollection.ShackledPrisoner,
            DemonicBond = ArcDpsIdCollection.DemonicBond,
            Thief = 17206,
            Gambler = 17335,
            GamblerClones = 17161,
            GamblerReal = 17355,
            Drunkard = 17163,
            Oil = 17332,
            Tear = 17303,
            Greed = 17213,
            Pride = 17233,
            Hands = 17221,
            // SH
            TormentedDead = 19422,
            SurgingSoul = 19474,
            Scythe = 19396,
            FleshWurm = 19464,
            // River
            Enervator = 19863,
            HollowedBomber = 19399,
            RiverOfSouls = 19829,
            SpiritHorde1 = 19461,
            SpiritHorde2 = 19400,
            SpiritHorde3 = 19692,
            // Statues of Darkness
            LightThieves = 19658,
            MazeMinotaur = 19402,
            // Statue of Death
            OrbSpider = 19801,
            GreenSpirit1 = 19587,
            GreenSpirit2 = 19571,
            AscalonianPeasant1 = 19810,
            AscalonianPeasant2 = 19758,
            // Skeletons are the same as Spirit hordes
            // Dhuum
            Messenger = 19807,
            Echo = 19628,
            Enforcer = 19681,
            Deathling = 19759,
            UnderworldReaper = 19831,
            DhuumDesmina = 19481,
            // CA
            ConjuredGreatsword = 21255,
            ConjuredShield = 21170,
            ConjuredPlayerSword = CASword,
            // Qadim
            LavaElemental1 = 21236,
            LavaElemental2 = 21078,
            IcebornHydra = 21163,
            GreaterMagmaElemental1 = 21150,
            GreaterMagmaElemental2 = 21223,
            FireElemental = 21221,
            FireImp = 21100,
            PyreGuardian = 21050,
            PyreGuardianRetal = ArcDpsIdCollection.PyreGuardianRetal,
            PyreGuardianResolution = ArcDpsIdCollection.PyreGuardianResolution,
            PyreGuardianProtect = ArcDpsIdCollection.PyreGuardianProtect,
            PyreGuardianStab = ArcDpsIdCollection.PyreGuardianStab,
            ReaperOfFlesh = 21218,
            DestroyerTroll = 20944,
            IceElemental = 21049,
            AncientInvokedHydra = 21285,
            ApocalypseBringer = 21073,
            WyvernMatriarch = 20997,
            WyvernPatriarch = 21183,
            QadimLamp = ArcDpsIdCollection.QadimLamp,
            AngryZommoros = 20961,
            ChillZommoros = 21118,
            AssaultCube = 21092,
            AwakenedSoldier = 21244,
            Basilisk = 21140,
            BlackMoa = 20980,
            BrandedCharr = 21083,
            BrandedDevourer = 21053,
            ChakDrone = 21064,
            CrazedKarkaHatchling = 21040,
            FireImpLamp = 21173,
            GhostlyPirateFighter = 21257,
            GiantBrawler = 21288,
            GiantHunter = 20972,
            GoldOoze = 21264,
            GrawlBascher = 21145,
            GrawlTrapper = 21290,
            GuildInitiateModusSceleris = 21161,
            IcebroodAtrocity = 16504,
            IcebroodKodan = 20975,
            IcebroodQuaggan = 21196,
            Jotun = 21054,
            JungleWurm = 21147,
            Karka = 21192,
            MinotaurBull = 20969,
            ModnirrBerserker = 20951,
            MoltenDisaggregator = 21010,
            MoltenProtector = 21037,
            MoltenReverberant = 20956,
            MordremVinetooth = 20940,
            Murellow = 21032,
            NightmareCourtier = 21261,
            OgreHunter = 21116,
            PirareSkrittSentry = 21189,
            PolarBear = 20968,
            Rabbit = 1085,
            ReefSkelk = 21024,
            RisenKraitDamoss = 21070,
            RottingAncientOakheart = 21252,
            RottingDestroyer = 21182,
            ShadowSkelk = 20966,
            SpiritOfExcess = 21095,
            TamedWarg = 18184,
            TarElemental = 21019,
            WindRider = 21164,
            // Adina
            HandOfErosion = ArcDpsIdCollection.HandOfErosion,
            HandOfEruption = ArcDpsIdCollection.HandOfEruption,
            // Sabir
            ParalyzingWisp = 21955,
            VoltaicWisp = 21975,
            SmallJumpyTornado = 21961,
            SmallKillerTornado = 21957,
            BigKillerTornado = 21987,
            // Peerless Qadim
            PeerlessQadimPylon = 21996,
            PeerlessQadimAuraPylon = 21962,
            EntropicDistortion = 21973,
            EnergyOrb = 21946,
            Brandstorm = 21978,
            GiantQadimThePeerless = 21953,
            DummyPeerlessQadim = 22005,
            // Fraenir
            IcebroodElemental = 22576,
            BoundIcebroodElemental = ArcDpsIdCollection.BoundIcebroodElemental,
            // Boneskinner
            PrioryExplorer = 22561,
            PrioryScholar = 22448,
            VigilRecruit = 22389,
            VigilTactician = 22420,
            AberrantWisp = 22538,
            Torch = ArcDpsIdCollection.Torch,
            // Whisper of Jormag
            WhisperEcho = 22628,
            DoppelgangerElementalist = 22627,
            DoppelgangerElementalist2 = 22691,
            DoppelgangerEngineer = 22625,
            DoppelgangerEngineer2 = 22699,
            DoppelgangerGuardian = 22608,
            DoppelgangerGuardian2 = 22635,
            DoppelgangerMesmer = 22683,
            DoppelgangerMesmer2 = 22721,
            DoppelgangerNecromancer = 22672,
            DoppelgangerNecromancer2 = 22713,
            DoppelgangerRanger = 22667,
            DoppelgangerRanger2 = 22678,
            DoppelgangerRevenant = 22610,
            DoppelgangerRevenant2 = 22615,
            DoppelgangerThief = 22612,
            DoppelgangerThief2 = 22656,
            DoppelgangerWarrior = 22640,
            DoppelgangerWarrior2 = 22717,
            // Cold War
            PropagandaBallon = 23093,
            DominionBladestorm = 23102,
            DominionStalker = 22882,
            DominionSpy1 = 22833,
            DominionSpy2 = 22856,
            DominionAxeFiend = 22938,
            DominionEffigy = 22897,
            FrostLegionCrusher = 23005,
            FrostLegionMusketeer = 22870,
            BloodLegionBlademaster = 22993,
            CharrTank = 22953,
            SonsOfSvanirHighShaman = 22283,
            // Aetherblade Hideout
            MaiTrinStrikeDuringEcho = 23826,
            ScarletPhantomNormalBeam = 24404,
            ScarletPhantomBreakbar = 23656,
            ScarletPhantomHP = 24431,
            ScarletPhantomHPCM = 25262,
            ScarletPhantomConeWaveNM = 24396,
            ScarletPhantomDeathBeamCM = 25284,
            ScarletPhantomDeathBeamCM2 = 25287,
            FerrousBomb = ArcDpsIdCollection.FerrousBomb,
            // Xunlai Jade Junkyard
            Ankka = 24634,
            KraitsHallucination = 24258,
            LichHallucination = 24158,
            QuaggansHallucinationNM = 24969,
            QuaggansHallucinationCM = 25289,
            ReanimatedMalice1 = 24976,
            ReanimatedMalice2 = 24171,
            ReanimatedSpite = 24348,
            ReanimatedHatred = 23673,
            ReanimatedAntipathy = 24827,
            ZhaitansReach = 23839,
            SanctuaryPrism = ArcDpsIdCollection.SanctuaryPrism,
            // Kaineng Overlook
            TheSniper = 23612,
            TheSniperCM = 25259,
            TheMechRider = 24660,
            TheMechRiderCM = 25271,
            TheEnforcer = 24261,
            TheEnforcerCM = 25236,
            TheRitualist = 23618,
            TheRitualistCM = 25242,
            TheMindblade = 24254,
            TheMindbladeCM = 25280,
            SpiritOfPain = 23793,
            SpiritOfDestruction = 23961,
            // Void Amalgamate
            PushableVoidAmalgamate = ArcDpsIdCollection.PushableVoidAmalgamate,
            VoidAmalgamate = 24375,
            KillableVoidAmalgamate = 23956,
            DragonBodyVoidAmalgamate = ArcDpsIdCollection.DragonBodyVoidAmalgamate,
            VoidTangler = 25138,
            VoidColdsteel = 23945,
            VoidAbomination = 23936,
            VoidSaltsprayDragon = 23846,
            VoidObliterator = 23995,
            VoidRotswarmer = 24590,
            VoidGiant = 24450,
            VoidSkullpiercer = 25177,
            VoidTimeCaster = 25025,
            VoidBrandbomber = 24783,
            VoidBurster = 24464,
            VoidWarforged1 = 24129,
            VoidWarforged2 = 24855,
            VoidStormseer = 24677,
            VoidMelter = 24223,
            VoidGoliath = 24761,
            DragonEnergyOrb = DragonOrb,
            // Cosmic Observatory
            TheTormented = 26016,
            VeteranTheTormented = 25829,
            EliteTheTormented = 26000,
            ChampionTheTormented = 25623,
            TormentedPhantom = 25604,
            SoulFeast = 26069,
            Zojja = 26011,
            // Temple of Febe
            EmbodimentOfGluttony = 25677,
            EmbodimentOfRage = 25686,
            EmbodimentOfDespair = 26034,
            EmbodimentOfRegret = 26049,
            EmbodimentOfEnvy = 25967,
            EmbodimentOfMalice = 25700,
            MaliciousShadow = 25747,
            // Freezie
            FreeziesFrozenHeart = 21328,
            IceStormer = 21325,
            IceSpiker = 21337,
            IcyProtector = 21326,
            // Fractals
            FractalVindicator = 19684,
            FractalAvenger = 15960,
            JadeMawTentacle = 16721,
            InspectorEllenKiel = 21566,
            ChampionRabbit = 11329,
            AwakenedAbomination = 21634,
            TheMossman = 11277,
            // MAMA
            Arkk = 16902,
            GreenKnight = 16906,
            RedKnight = 16974,
            BlueKnight = 16899,
            TwistedHorror = 17009,
            // Siax
            VolatileHallucinationSiax = 17002,
            EchoOfTheUnclean = 17068,
            NightmareHallucinationSiax = 16911,
            // Ensolyss
            NightmareHallucination1 = 16912, // (exploding after jump and charging in last phase)
            NightmareHallucination2 = 17033, // (small adds, last phase)
            NightmareAltar = 35791,
            // Skorvald
            FluxAnomaly1 = 17578,
            FluxAnomaly2 = 17929,
            FluxAnomaly3 = 17695,
            FluxAnomaly4 = 17651,
            FluxAnomalyCM1 = 17599,
            FluxAnomalyCM2 = 17770,
            FluxAnomalyCM3 = 17851,
            FluxAnomalyCM4 = 17673,
            SolarBloom = 17732,
            // Artsariiv
            TemporalAnomalyArtsariiv = 17870,
            Spark = 17630,
            SmallArtsariiv = 17811, // tiny adds
            MediumArtsariiv = 17694, // small adds
            BigArtsariiv = 17937, // big adds
            CloneArtsariiv = SubArtsariiv, // clone adds
            // Arkk
            TemporalAnomalyArkk = 17720,
            Archdiviner = 17893,
            FanaticDagger1 = 11281,
            FanaticDagger2 = 11282,
            FanaticBow = 11288,
            EliteBrazenGladiator = 17730,
            BLIGHT = 16437,
            PLINK = 16325,
            DOC = 16657,
            CHOP = 16552,
            ProjectionArkk = 17613,
            // Ai
            EnragedWaterSprite = 23270,
            TransitionSorrowDemon1 = 23265,
            TransitionSorrowDemon2 = 23242,
            TransitionSorrowDemon3 = 23279,
            TransitionSorrowDemon4 = 23245,
            CCSorrowDemon = 23256,
            AiDoubtDemon = 23268,
            PlayerDoubtDemon = 23246,
            FearDemon = 23264,
            GuiltDemon = 23252,
            // Kanaxai
            AspectOfTorment = 25556,
            AspectOfLethargy = 25561,
            AspectOfExposure = 25562,
            AspectOfDeath = 25580,
            AspectOfFear = 25563,
            LuxonMonkSpirit = 25571,
            CaptainThess1 = 25554,
            CaptainThess2 = 25557,
            // Open world Soo-Won
            SooWonTail = 51756,
            VoidGiant2 = 24310,
            VoidTimeCaster2 = 24586,
            VoidBrandstalker = 24951,
            VoidColdsteel2 = 23791,
            VoidObliterator2 = 24947,
            VoidAbomination2 = 23886,
            VoidBomber = 24714,
            VoidBrandbeast = 23917,
            VoidBrandcharger1 = 24936,
            VoidBrandcharger2 = 24039,
            VoidBrandfang1 = 24912,
            VoidBrandfang2 = 24772,
            VoidBrandscale1 = 24053,
            VoidBrandscale2 = 24426,
            VoidColdsteel3 = 24063,
            VoidCorpseknitter1 = 24756,
            VoidCorpseknitter2 = 24607,
            VoidDespoiler1 = 23874,
            VoidDespoiler2 = 25179,
            VoidFiend1 = 23707,
            VoidFiend2 = 24737,
            VoidFoulmaw = 24766,
            VoidFrostwing = 24780,
            VoidGlacier1 = 23753,
            VoidGlacier2 = 24235,
            VoidInfested1 = 24390,
            VoidInfested2 = 24997,
            VoidMelter1 = 24497,
            VoidMelter2 = 24807,
            VoidRimewolf1 = 24698,
            VoidRimewolf2 = 23798,
            VoidRotspinner1 = 25057,
            VoidStorm = 24007,
            VoidStormseer2 = 24419,
            VoidStormseer3 = 23962,
            VoidTangler2 = 23567,
            VoidThornheart1 = 24406,
            VoidThornheart2 = 23688,
            VoidWorm = 23701,
            //
            Environment = ArcDpsIdCollection.Environment,
            //
            Unknown = int.MaxValue,
        };
        public static TrashID GetTrashID(int id) {
            return Enum.IsDefined(typeof(TrashID), id) ? (TrashID)id : TrashID.Unknown;
        }

        public enum TargetID : int {
            WorldVersusWorld = 1,
            Instance = 2,
            DummyTarget = ArcDpsIdCollection.DummyTarget,
            Mordremoth = 15884,
            // Raid
            ValeGuardian = 15438,
            Gorseval = 15429,
            Sabetha = 15375,
            Slothasor = 16123,
            Berg = 16088,
            Zane = 16137,
            Narella = 16125,
            Matthias = 16115,
            McLeodTheSilent = 16253,
            KeepConstruct = 16235,
            Xera = 16246,
            Xera2 = 16286,
            Cairn = 17194,
            MursaatOverseer = 17172,
            Samarog = 17188,
            Deimos = 17154,
            SoullessHorror = 19767,
            Desmina = 19828,
            BrokenKing = 19691,
            EaterOfSouls = 19536,
            EyeOfJudgement = 19651,
            EyeOfFate = 19844,
            Dhuum = 19450,
            ConjuredAmalgamate = 43974, // Gadget
            CARightArm = 10142, // Gadget
            CALeftArm = 37464, // Gadget
            ConjuredAmalgamate_CHINA = 44885, // Gadget
            CARightArm_CHINA = 11053, // Gadget
            CALeftArm_CHINA = 38375, // Gadget
            Nikare = 21105,
            Kenut = 21089,
            Qadim = 20934,
            Freezie = 21333,
            Adina = 22006,
            Sabir = 21964,
            PeerlessQadim = 22000,
            // Strike Missions
            IcebroodConstruct = 22154,
            VoiceOfTheFallen = 22343,
            ClawOfTheFallen = 22481,
            VoiceAndClaw = 22315,
            FraenirOfJormag = 22492,
            IcebroodConstructFraenir = 22436,
            Boneskinner = 22521,
            WhisperOfJormag = 22711,
            VariniaStormsounder = 22836,
            MaiTrinStrike = 24033,
            DummyMaiTrinStrike = ArcDpsIdCollection.DummyMaiTrinStrike,
            EchoOfScarletBriarNM = 24768,
            EchoOfScarletBriarCM = 25247,
            Ankka = 23957,
            MinisterLi = 24485,
            MinisterLiCM = 24266,
            GadgetTheDragonVoid1 = 43488,
            GadgetTheDragonVoid2 = 1378,
            VoidAmalgamate1 = 24375,
            TheDragonVoidZhaitan = ArcDpsIdCollection.TheDragonVoidZhaitan,
            TheDragonVoidJormag = ArcDpsIdCollection.TheDragonVoidJormag,
            TheDragonVoidKralkatorrik = ArcDpsIdCollection.TheDragonVoidKralkatorrik,
            TheDragonVoidSooWon = ArcDpsIdCollection.TheDragonVoidSooWon,
            TheDragonVoidPrimordus = ArcDpsIdCollection.TheDragonVoidPrimordus,
            TheDragonVoidMordremoth = ArcDpsIdCollection.TheDragonVoidMordremoth,
            PrototypeVermilion = 25413,
            PrototypeArsenite = 25415,
            PrototypeIndigo = 25419,
            PrototypeVermilionCM = 25414,
            PrototypeArseniteCM = 25416,
            PrototypeIndigoCM = 25423,
            Dagda = 25705,
            Cerus = 25989,
            //VoidAmalgamate = 
            // Fract
            MAMA = 17021,
            Siax = 17028,
            Ensolyss = 16948,
            Skorvald = 17632,
            Artsariiv = 17949,
            Arkk = 17759,
            MaiTrinFract = 19697,
            ShadowMinotaur = 20682,
            BroodQueen = 20742,
            TheVoice = 20497,
            AiKeeperOfThePeak = 23254,
            AiKeeperOfThePeak2 = ArcDpsIdCollection.AiKeeperOfThePeak2,
            KanaxaiScytheOfHouseAurkusNM = 25572,
            KanaxaiScytheOfHouseAurkusCM = 25577,
            // Golems
            MassiveGolem10M = 16169,
            MassiveGolem4M = 16202,
            MassiveGolem1M = 16178,
            VitalGolem = 16198,
            AvgGolem = 16177,
            StdGolem = 16199,
            LGolem = 19676,
            MedGolem = 19645,
            ConditionGolem = 16174,
            PowerGolem = 16176,
            // Open world
            SooWonOW = 35552,
            //
            Unknown = int.MaxValue,
        };
        public static TargetID GetTargetID(int id) {
            return Enum.IsDefined(typeof(TargetID), id) ? (TargetID)id : TargetID.Unknown;
        }

        public enum ChestID : int {
            ChestOfDesmina = ArcDpsIdCollection.ChestOfDesmina,
            ChestOfSouls = ArcDpsIdCollection.ChestOfSouls,
            SiegeChest = ArcDpsIdCollection.SiegeChest,
            CAChest = ArcDpsIdCollection.CAChest,
            //
            None = int.MaxValue,
        };
        public static ChestID GetChestID(int id) {
            return Enum.IsDefined(typeof(ChestID), id) ? (ChestID)id : ChestID.None;
        }

        public enum MinionID : int {
            // Racial Summons
            HoundOfBalthazar = 6394,
            SnowWurm = 6445,
            DruidSpirit = 6475,
            SylvanHound = 6476,
            IronLegionSoldier = 6509,
            IronLegionMarksman = 6510,
            BloodLegionSoldier = 10106,
            BloodLegionMarksman = 10107,
            AshLegionSoldier = 10108,
            AshLegionMarksman = 10109,
            STAD007 = 10145,
            STA7012 = 10146,
            // GW2 Digital Deluxe
            MistfireWolf = 9801,
            // Rune Summons
            RuneJaggedHorror = 21314,
            RuneRockDog = 8836,
            RuneMarkIGolem = 8837,
            RuneTropicalBird = 8838,
            // Consumables with summons
            Ember = 1454,
            HawkeyeGriffon = 5614,
            SousChef = 10076,
            SunspearParagonSupport = 19643,
            RavenSpiritShadow = 22309,
            // Mesmer Phantasmas
            IllusionarySwordsman = 6487,
            IllusionaryBerserker = 6535,
            IllusionaryDisenchanter = 6621,
            IllusionaryRogue = 9444,
            IllusionaryDefender = 9445,
            IllusionaryMage = 5750,
            IllusionaryDuelist = 5758,
            IllusionaryWarlock = 6449,
            IllusionaryWarden = 7981,
            IllusionaryMariner = 9052,
            IllusionaryWhaler = 9057,
            IllusionaryAvenger = 15188,
            // Mesmer Clones
            // - Single Weapon
            CloneSword = 8108,
            CloneScepter = 8109,
            CloneAxe = 18894,
            CloneGreatsword = 8110,
            CloneStaff = 8111,
            CloneTrident = 9058,
            CloneSpear = 6479,
            CloneDownstate = 10542,
            CloneDagger = 25569,
            CloneUnknown = 8107, // Possibly -> https://wiki.guildwars2.com/wiki/Clone_(Snowball_Mayhem)
            // - Sword + Offhand
            CloneSwordTorch = 15090,
            CloneSwordFocus = 15114,
            CloneSwordSword = 15233,
            CloneSwordShield = 15199,
            CloneSwordPistol = 15181,
            // - Sword 3 + Offhand
            CloneIllusionaryLeap = 8106,
            CloneIllusionaryLeapFocus = 15084,
            CloneIllusionaryLeapShield = 15131,
            CloneIllusionaryLeapSword = 15117,
            CloneIllusionaryLeapPistol = 15003,
            CloneIllusionaryLeapTorch = 15032,
            // - Scepter + Offhand
            CloneScepterTorch = 15044,
            CloneScepterShield = 15156,
            CloneScepterPistol = 15196,
            CloneScepterFocus = 15240,
            CloneScepterSword = 15249,
            // - Axe + Offhand
            CloneAxeTorch = 18922,
            CloneAxePistol = 18939,
            CloneAxeSword = 19134,
            CloneAxeFocus = 19257,
            CloneAxeShield = 25576,
            // - Dagger + Offhand
            CloneDaggerShield = 25570,
            CloneDaggerPistol = 25573,
            CloneDaggerFocus = 25575,
            CloneDaggerTorch = 25578,
            CloneDaggerSword = 25582,
            // Necromancer Minions
            BloodFiend = 1104,
            BoneFiend = 1458,
            FleshGolem = 1792,
            ShadowFiend = 5673,
            FleshWurm = 6002,
            BoneMinion = 1192,
            UnstableHorror = 18802,
            ShamblingHorror = 15314,
            // Ranger Spirits
            StoneSpirit = 6370,
            SunSpirit = 6330,
            FrostSpirit = 6369,
            StormSpirit = 6371,
            WaterSpirit = 12778,
            SpiritOfNatureRenewal = 6649,
            // Ranger Pets
            JuvenileJungleStalker = 3827,
            JuvenileKrytanDrakehound = 4425,
            JuvenileBrownBear = 4426,
            JuvenileCarrionDevourer = 5581,
            JuvenileSalamanderDrake = 5582,
            JuvenileAlpineWolf = 6043,
            JuvenileSnowLeopard = 6044,
            JuvenileRaven = 6045,
            JuvenileJaguar = 6849,
            JuvenileMarshDrake = 6850,
            JuvenileBlueMoa = 6883,
            JuvenilePinkMoa = 6884,
            JuvenileRedMoa = 6885,
            JuvenileWhiteMoa = 6886,
            JuvenileBlackMoa = 6887,
            JuvenileRiverDrake = 6888,
            JuvenileIceDrake = 6889,
            JuvenileMurellow = 6898,
            JuvenileShark = 6968,
            JuvenileFernHound = 7336,
            JuvenilePolarBear = 7926,
            JuvenileBlackBear = 7927,
            JuvenileArctodus = 7928,
            JuvenileLynx = 7932,
            JuvenileWhiptailDevourer = 7948,
            JuvenileLashtailDevourer = 7949,
            JuvenileWolf = 7975,
            JuvenileHyena = 7976,
            JuvenileOwl = 8002,
            JuvenileEagle = 8003,
            JuvenileWhiteRaven = 8004,
            JuvenileCaveSpider = 8005,
            JuvenileJungleSpider = 8006,
            JuvenileForestSpider = 8007,
            JuvenileBlackWidowSpider = 8008,
            JuvenileBoar = 8013,
            JuvenileWarthog = 8014,
            JuvenileSiamoth = 8015,
            JuvenilePig = 8016,
            JuvenileArmorFish = 8035,
            JuvenileBlueJellyfish = 8041,
            JuvenileRedJellyfish = 8042,
            JuvenileRainbowJellyfish = 9458,
            JuvenileHawk = 10022,
            JuvenileReefDrake = 11491,
            JuvenileTiger = 15380,
            JuvenileFireWywern = 15399,
            JuvenileSmokescale = 15402,
            JuvenileBristleback = 15418,
            JuvenileEletricWywern = 15436,
            JuvenileJacaranda = 18119,
            JuvenileFangedIboga = 18688,
            JuvenileCheetah = 19005,
            JuvenileRockGazelle = 19104,
            JuvenileSandLion = 19166,
            JuvenileWallow = 24203,
            JuvenileWhiteTiger = 24298,
            JuvenileSiegeTurtle = 24796,
            JuvenilePhoenix = 25131,
            JuvenileAetherHunter = 25652,
            // Guardian Weapon Summons
            BowOfTruth = 6383,
            HammerOfWisdom = 5791,
            ShieldOfTheAvenger = 6382,
            SwordOfJustice = 6381,
            // Thief
            Thief1 = 7580,
            Thief2 = 7581,
            Thief3 = 10090,
            Thief4 = 10091,
            Thief5 = 10092,
            Thief6 = 10093,
            Thief7 = 10094,
            Thief8 = 10095,
            Thief9 = 10098,
            Thief10 = 10099,
            Thief11 = 10102,
            Thief12 = 10103,
            Thief13 = 18049,
            Thief14 = 18419,
            Thief15 = 18492,
            Thief16 = 18853,
            Thief17 = 18871,
            Thief18 = 18947,
            Thief19 = 19069,
            Thief20 = 19087,
            Thief21 = 19244,
            Thief22 = 19258,
            Daredevil1 = 17970,
            Daredevil2 = 18161,
            Daredevil3 = 18369,
            Daredevil4 = 18420,
            Daredevil5 = 18502,
            Daredevil6 = 18600,
            Daredevil7 = 18723,
            Daredevil8 = 18742,
            Daredevil9 = 19197,
            Daredevil10 = 19242,
            Deadeye1 = 18023,
            Deadeye2 = 18053,
            Deadeye3 = 18224,
            Deadeye4 = 18249,
            Deadeye5 = 18264,
            Deadeye6 = 18565,
            Deadeye7 = 18710,
            Deadeye8 = 18812,
            Deadeye9 = 18870,
            Deadeye10 = 18902,
            Specter1 = 25210,
            Specter2 = 25211,
            Specter3 = 25212,
            Specter4 = 25220,
            Specter5 = 25221,
            Specter6 = 25223,
            Specter7 = 25227,
            Specter8 = 25231,
            Specter9 = 25232,
            Specter10 = 25234,
            // Elementalist Summons
            LesserAirElemental = 8711,
            LesserEarthElemental = 8712,
            LesserFireElemental = 8713,
            LesserIceElemental = 8714,
            AirElemental = 6522,
            EarthElemental = 6523,
            FireElemental = 6524,
            IceElemental = 6525,
            // Scrapper Gyros
            SneakGyro = 15012,
            ShredderGyro = 15046,
            BulwarkGyro = 15134,
            PurgeGyro = 15135,
            MedicGyro = 15208,
            BlastGyro = 15330,
            FunctionGyro = 15336,
            // Revenant Summons
            ViskIcerazor = 18524,
            KusDarkrazor = 18594,
            JasRazorclaw = 18791,
            EraBreakrazor = 18806,
            OfelaSoulcleave = 19002,
            VentariTablet = ArcDpsIdCollection.VentariTablet,
            // Mechanist
            JadeMech = 23549,
            //
            Unknown,
        }

        public static MinionID GetMinionID(int id) {
            return Enum.IsDefined(typeof(MinionID), id) ? (MinionID)id : MinionID.Unknown;
        }
    }
}
