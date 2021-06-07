using System.Collections.Generic;

namespace Blish_HUD.Entities {
    public interface IWorld {

        IEnumerable<IEntity> Entities { get; }

        void AddEntity(IEntity entity);

        void AddEntities(IEnumerable<IEntity> entities);

        void RemoveEntity(IEntity entity);

        void RemoveEntities(IEnumerable<IEntity> entities);

    }
}
