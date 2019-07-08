using Microsoft.Xna.Framework;

namespace Blish_HUD.Pathing {
    public interface IPathable<out TEntity> : IPathable
        where TEntity : Blish_HUD.Entities.Entity {

        TEntity ManagedEntity { get; }
    }

    public interface IPathable : IUpdatable {

        int    MapId  { get; set; }
        string Guid   { get; set; }
        UserAccess Access { get; set; }
        bool   Active { get; set; }

        float   Opacity  { get; set; }
        float   Scale    { get; set; }
        Vector3 Position { get; set; }

    }
}
