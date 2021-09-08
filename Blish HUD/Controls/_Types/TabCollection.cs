using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Blish_HUD.Controls {
    /// <summary>
    /// A collection of tabs, sorted from largest <see cref="Tab.OrderPriority"/> to the smallest.
    /// </summary>
    public class TabCollection : ICollection<Tab> {

        private List<Tab> _tabs = new List<Tab>();

        private readonly ITabOwner _owner;

        public TabCollection(ITabOwner owner) {
            _owner = owner;
        }

        public IEnumerator<Tab> GetEnumerator() {
            return _tabs.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        public void Add(Tab item) {
            _tabs = new List<Tab>(_tabs.Concat(new []{ item }).OrderBy(tab => tab.OrderPriority));

            if (_tabs.Count == 1) {
                _owner.SelectedTab = item;
            }
        }

        public void Clear() {
            _tabs.Clear();

            _owner.SelectedTab = null;
        }

        public bool Contains(Tab item) {
            return _tabs.Contains(item);
        }

        public void CopyTo(Tab[] array, int arrayIndex) {
            _tabs.CopyTo(array, arrayIndex);
        }

        public bool Remove(Tab item) {
            return _tabs.Remove(item);
        }

        /// <summary>
        /// Returns the index of the provided <see cref="Tab"/>.  If the <see cref="Tab"/> is not within the collection, -1 is returned.
        /// </summary>
        public int IndexOf(Tab tab) {
            return _tabs.IndexOf(tab);
        }

        /// <summary>
        /// Returns the <see cref="Tab"/> at the provided index based on <see cref="Tab.OrderPriority"/>.
        /// </summary>
        public Tab FromIndex(int tabIndex) {
            if (tabIndex >= 0 && tabIndex < _tabs.Count) {
                return _tabs[tabIndex];
            }

            return null;
        }

        public int  Count      => _tabs.Count;
        public bool IsReadOnly => false;

    }
}
