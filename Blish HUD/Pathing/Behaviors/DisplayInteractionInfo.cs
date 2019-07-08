﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Blish_HUD.Controls;
using Blish_HUD.Entities;
using Microsoft.Xna.Framework;

namespace Blish_HUD.Pathing.Behaviors {
    public class DisplayInteractionInfo<TPathable, TEntity> : Interactable<TPathable, TEntity>
        where TPathable : ManagedPathable<TEntity>
        where TEntity : Entity {

        //private const string BEHAVIOR_NAME = "ReappearOnMapChange";

        protected readonly InteractionInfo Indicator;

        protected static SettingEntry<Dictionary<string, bool>> _userStateStore;

        public string InfoText {
            get => Indicator.Text;
            set => Indicator.Text = value;
        }

        //private string GetBehaviorStoreEntry(string characterName) {
        //    var toonName = GameService.Player.CharacterName;

        //    if (_userStateStore.Value.ContainsKey(characterName)) {

        //    }
        //}

        public DisplayInteractionInfo(TPathable managedPathable) : base(managedPathable) {
            //_userStateStore = _userStateStore ?? PersistentBehaviorStore.Value.DefineSetting(
            //                                                              BEHAVIOR_NAME,
            //                                                              new Dictionary<string, bool>(),
            //                                                              new Dictionary<string, bool>()
            //                                                             );

            Indicator = new InteractionInfo();
        }

        public override void OnEnterZoneRadius(GameTime gameTime) {
            Indicator.Show();
        }

        public override void OnLeftZoneRadius(GameTime gameTime) {
            Indicator.Hide();
        }

    }
}
