using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blish_HUD.Modules.Managers {

    public class DirectoriesManager {

        private readonly HashSet<string>            _directoryNames;
        private readonly Dictionary<string, string> _directoryPaths;

        public IReadOnlyList<string> RegisteredDirectories => _directoryNames.ToList();

        public DirectoriesManager(IEnumerable<string> directoryNames) {
            _directoryNames = new HashSet<string>(directoryNames, StringComparer.OrdinalIgnoreCase);
            _directoryPaths = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            PrepareDirectories();
        }

        private void PrepareDirectories() {
            foreach (string directoryName in _directoryNames) {
                _directoryPaths.Add(directoryName, GameService.Directory.RegisterDirectory(directoryName));
            }
        }

        public string GetFullDirectoryPath(string directoryName) {
            if (!_directoryNames.Contains(directoryName)) return null;

            return _directoryPaths[directoryName];
        }

    }

}
