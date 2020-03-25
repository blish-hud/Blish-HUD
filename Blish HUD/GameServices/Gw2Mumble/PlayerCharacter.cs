using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gw2Sharp.Models;
using Gw2Sharp.Mumble;
using Microsoft.Xna.Framework;

namespace Blish_HUD.Gw2Mumble {
    public class PlayerCharacter {

        private readonly Gw2MumbleService _service;

        private Vector3 _position = Vector3.Zero;
        private Vector3 _forward  = Vector3.Forward;

        /// <inheritdoc cref="IGw2MumbleClient.AvatarPosition"/>
        public Vector3 Position => _position;

        /// <inheritdoc cref="IGw2MumbleClient.AvatarFront"/>
        public Vector3 Forward => _forward;

        /// <inheritdoc cref="IGw2MumbleClient.CharacterName"/>
        public string Name => _service.RawClient.CharacterName;

        /// <inheritdoc cref="IGw2MumbleClient.Profession"/>
        public ProfessionType Profession => _service.RawClient.Profession;

        /// <inheritdoc cref="IGw2MumbleClient.Race"/>
        public RaceType Race => _service.RawClient.Race;

        /// <inheritdoc cref="IGw2MumbleClient.Specialization"/>
        public int Specialization => _service.RawClient.Specialization;

        /// <inheritdoc cref="IGw2MumbleClient.TeamColorId"/>
        public int TeamColorId => _service.RawClient.TeamColorId;

        /// <inheritdoc cref="IGw2MumbleClient.IsCommander"/>
        public bool IsCommander => _service.RawClient.IsCommander;

        internal PlayerCharacter(Gw2MumbleService service) {
            _service      = service;
        }

        internal void Update(GameTime gameTime) {
            _position = _service.RawClient.AvatarPosition.ToXnaVector3();
            _forward  = _service.RawClient.AvatarFront.ToXnaVector3();
        }

    }
}
