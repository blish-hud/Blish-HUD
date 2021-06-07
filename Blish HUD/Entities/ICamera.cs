using Microsoft.Xna.Framework;

namespace Blish_HUD.Entities {
    public interface ICamera {

        /// <summary>
        /// The camera's position.
        /// </summary>
        Vector3 Position { get; }

        /// <summary>
        /// The normalized vector pointing forward out of the camera.
        /// </summary>
        Vector3 Forward { get; }

        /// <summary>
        /// The angle describing the camera's field of view.
        /// </summary>
        float FieldOfView { get; }

        /// <summary>
        /// The closest distance that entities are rendered.
        /// </summary>
        float NearPlaneRenderDistance { get; }

        /// <summary>
        /// The farthest distance that entities are rendered.
        /// </summary>
        float FarPlaneRenderDistance { get; }

    }
}
