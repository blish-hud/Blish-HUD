using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Blish_HUD.Controls._Types {
    public class TabCollection : ICollection<ITab> {

        private List<ITab> _tabs = new List<ITab>();

        public IEnumerator<ITab> GetEnumerator() {
            return _tabs.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        public void Add(ITab item) {
            _tabs.Add(item);
            _tabs = _tabs.OrderBy(tab => tab.Priority).ToList();
        }

        public void Clear() {
            _tabs.Clear();
        }

        public bool Contains(ITab item) {
            return _tabs.Contains(item);
        }

        public void CopyTo(ITab[] array, int arrayIndex) {
            _tabs.CopyTo(array, arrayIndex);
        }

        public bool Remove(ITab item) {
            return _tabs.Remove(item);
        }

        public int  Count      => _tabs.Count;
        public bool IsReadOnly => false;

    }
}
