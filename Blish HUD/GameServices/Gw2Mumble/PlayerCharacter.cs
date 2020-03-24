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
        public string Name => _service.SharedGw2MumbleClient.CharacterName;

        /// <inheritdoc cref="IGw2MumbleClient.Profession"/>
        public ProfessionType Profession => _service.SharedGw2MumbleClient.Profession;

        /// <inheritdoc cref="IGw2MumbleClient.Race"/>
        public RaceType Race => _service.SharedGw2MumbleClient.Race;

        /// <inheritdoc cref="IGw2MumbleClient.Specialization"/>
        public int Specialization => _service.SharedGw2MumbleClient.Specialization;

        /// <inheritdoc cref="IGw2MumbleClient.TeamColorId"/>
        public int TeamColorId => _service.SharedGw2MumbleClient.TeamColorId;

        /// <inheritdoc cref="IGw2MumbleClient.IsCommander"/>
        public bool IsCommander => _service.SharedGw2MumbleClient.IsCommander;

        internal PlayerCharacter(Gw2MumbleService service) {
            _service      = service;
        }

        internal void Update(GameTime gameTime) {
            _position = _service.SharedGw2MumbleClient.AvatarPosition.ToXnaVector3();
            _forward  = _service.SharedGw2MumbleClient.AvatarFront.ToXnaVector3();
        }

    }
}
