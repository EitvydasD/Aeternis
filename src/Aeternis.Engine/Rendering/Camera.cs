using Microsoft.Xna.Framework;

namespace Aeternis.Engine.Rendering;
public class Camera
{
    public Matrix Transform { get; private set; }
    public Vector2 Position { get; private set; }

    /// <summary>
    /// Updates the camera's position and transform to follow the target.
    /// </summary>
    /// <param name="targetPosition">The position of the player or target.</param>
    /// <param name="screenWidth">The width of the game screen.</param>
    /// <param name="screenHeight">The height of the game screen.</param>
    public void Update(Vector2 targetPosition, int screenWidth, int screenHeight)
    {
        // Center the camera on the target
        Position = targetPosition - new Vector2(screenWidth / 2f, screenHeight / 2f);

        // Update the transform matrix
        Transform = Matrix.CreateTranslation(-Position.X, -Position.Y, 0);
    }
}
