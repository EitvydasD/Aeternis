using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Text.Json;

namespace Aeternis.Logic.World;
public class MapLoader()
{
    private int[,] _map = new int[0, 0];
    private int _tileWidth;
    private int _tileHeight;

    public List<CustomTiledObject> CollisionObjects { get; private set; } = [];

    public void Load(string mapPath)
    {
        // Load map from JSON format
        string jsonContent = File.ReadAllText(mapPath);
        var mapData = JsonSerializer.Deserialize<CustomTiledMap>(jsonContent);

        if (mapData is null)
        {
            throw new Exception("Invalid map format.");
        }

        _tileWidth = mapData.TileWidth;
        _tileHeight = mapData.TileHeight;
        int mapWidth = mapData.Width;
        int mapHeight = mapData.Height;

        _map = new int[mapWidth, mapHeight];

        var layers = mapData.Layers;
        foreach (var layer in layers)
        {
            if (layer.Type == TiledLayerType.Tile)
            {
                var data = layer.Data;
                for (int y = 0; y < mapHeight; y++)
                {
                    for (int x = 0; x < mapWidth; x++)
                    {
                        _map[x, y] = data[(y * mapWidth) + x];
                    }
                }
            }
            else if (layer.Type == TiledLayerType.Object)
            {
                for (int i = 0; i < layer.Objects.Count; i++)
                {
                    var collisionObject = new CustomTiledObject
                    {
                        X = layer.Objects[i].X,
                        Y = layer.Objects[i].Y,
                        Width = layer.Objects[i].Width,
                        Height = layer.Objects[i].Height,
                        PolygonPoints = layer.Objects[i].PolygonPoints,
                    };
                    CollisionObjects.Add(collisionObject);
                }
            }
        }
    }

    public void Draw(SpriteBatch spriteBatch, Texture2D tileSet, Vector2 cameraPosition, Texture2D collisionTexture)
    {
        if (_map == null || tileSet == null)
            return;

        int tilesetColumns = (tileSet.Width + 1) / _tileWidth;

        for (int y = 0; y < _map.GetLength(1); y++)
        {
            for (int x = 0; x < _map.GetLength(0); x++)
            {
                int tileId = _map[x, y] - 6;
                if (tileId < 0)
                    continue;

                int row = tileId / tilesetColumns;
                int column = tileId % tilesetColumns;

                int sourceX = column * (_tileWidth + 1);
                int sourceY = row * (_tileHeight + 1);

                Rectangle sourceRect = new Rectangle(sourceX, sourceY, _tileWidth, _tileHeight);
                Vector2 position = new Vector2(x * _tileWidth, y * _tileHeight);

                spriteBatch.Draw(tileSet, position, sourceRect, Color.White);
            }
        }

        DrawCollisions(spriteBatch, collisionTexture);
    }

    private void DrawCollisions(SpriteBatch spriteBatch, Texture2D collisionTexture)
    {
        if (!Configuration.DebugMode)
        {
            return;
        }

        // Draw collision lines for collision objects
        foreach (var collisionObject in CollisionObjects)
        {
            if (collisionObject.PolygonPoints != null && collisionObject.PolygonPoints.Count > 0)
            {
                // Convert polygon points to screen space
                var polygonPoints = collisionObject.PolygonPoints.Select(p =>
                    new Vector2(collisionObject.X + p.X, collisionObject.Y + p.Y)).ToArray();

                // Draw polygon edges
                for (int i = 0; i < polygonPoints.Length; i++)
                {
                    Vector2 start = polygonPoints[i];
                    Vector2 end = polygonPoints[(i + 1) % polygonPoints.Length]; // Wrap around to the first point

                    DrawLine(spriteBatch, start, end, Color.Red, collisionTexture);
                }
            }
            else
            {
                // Draw rectangle bounds if no polygon points exist
                Rectangle collisionBounds = new Rectangle(
                    collisionObject.X,
                    collisionObject.Y,
                    collisionObject.Width,
                    collisionObject.Height
                );

                // Draw top edge
                spriteBatch.Draw(collisionTexture, new Rectangle(collisionBounds.Left, collisionBounds.Top, collisionBounds.Width, 1), Color.Red);
                // Draw bottom edge
                spriteBatch.Draw(collisionTexture, new Rectangle(collisionBounds.Left, collisionBounds.Bottom - 1, collisionBounds.Width, 1), Color.Red);
                // Draw left edge
                spriteBatch.Draw(collisionTexture, new Rectangle(collisionBounds.Left, collisionBounds.Top, 1, collisionBounds.Height), Color.Red);
                // Draw right edge
                spriteBatch.Draw(collisionTexture, new Rectangle(collisionBounds.Right - 1, collisionBounds.Top, 1, collisionBounds.Height), Color.Red);
            }
        }
    }

    // Helper method to draw a line between two points
    private void DrawLine(SpriteBatch spriteBatch, Vector2 start, Vector2 end, Color color, Texture2D texture)
    {
        float length = Vector2.Distance(start, end);
        float angle = (float)Math.Atan2(end.Y - start.Y, end.X - start.X);

        spriteBatch.Draw(
            texture,
            position: start,
            sourceRectangle: null,
            color: color,
            rotation: angle,
            origin: Vector2.Zero,
            scale: new Vector2(length, 1), // Line length and thickness
            effects: SpriteEffects.None,
            layerDepth: 0
        );
    }
}