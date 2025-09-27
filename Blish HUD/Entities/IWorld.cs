using System.Collections.Generic;

namespace Blish_HUD.Entities {
    public interface IWorld {

        public IEnumerable<IEntity> Entities { get; }

        public void AddEntity(IEntity entity);

        public void AddEntities(IEnumerable<IEntity> entities);

        public void RemoveEntity(IEntity entity);

        public void RemoveEntities(IEnumerable<IEntity> entities);

    }
}
