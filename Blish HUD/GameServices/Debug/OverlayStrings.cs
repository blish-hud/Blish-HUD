using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Blish_HUD.Debug {

    public class OverlayStrings {

        public Func<GameTime, string> this[string key] {
            get => _texts[key];
            set => _texts[key] = value;
        }
        public ICollection<string>                 Keys   => _texts.Keys;
        public ICollection<Func<GameTime, string>> Values => _texts.Values;

        private readonly ConcurrentDictionary<string, Func<GameTime, string>> _texts = new ConcurrentDictionary<string, Func<GameTime, string>>();

        public bool ContainsKey(string key) {
            return _texts.ContainsKey(key);
        }

        public void Add(string key, Func<GameTime, string> value) {
            _texts.TryAdd(key, value);
        }

        public bool TryAdd(string key, Func<GameTime, string> value) {
            return _texts.TryAdd(key, value);
        }

        public bool Remove(string key) {
            return _texts.TryRemove(key, out _);
        }

    }

}