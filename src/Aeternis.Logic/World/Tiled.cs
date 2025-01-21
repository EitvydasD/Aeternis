using System.Text.Json.Serialization;

namespace Aeternis.Logic.World;

public static class TiledLayerType
{
    public const string Tile = "tilelayer";
    public const string Object = "objectgroup";
}

public class CustomTiledMap
{
    [JsonPropertyName("height")]
    public required int Height { get; set; }

    [JsonPropertyName("width")]
    public required int Width { get; set; }

    [JsonPropertyName("tileheight")]
    public required int TileHeight { get; set; }

    [JsonPropertyName("tilewidth")]
    public required int TileWidth { get; set; }

    [JsonPropertyName("infinite")]
    public required bool Infinite { get; set; }

    [JsonPropertyName("layers")]
    public List<CustomTiledLayer> Layers { get; set; } = [];
}

public class CustomTiledLayer
{
    [JsonPropertyName("id")]
    public required int Id { get; set; }

    [JsonPropertyName("data")]
    public int[] Data { get; set; } = [];

    [JsonPropertyName("objects")]
    public List<CustomTiledObject> Objects { get; set; } = [];

    [JsonPropertyName("height")]
    public int Height { get; set; }

    [JsonPropertyName("width")]
    public int Width { get; set; }

    [JsonPropertyName("name")]
    public required string Name { get; set; }

    [JsonPropertyName("type")]
    public required string Type { get; set; }

    [JsonPropertyName("opacity")]
    public required int Opacity { get; set; }

    [JsonPropertyName("visible")]
    public required bool Visible { get; set; }

    [JsonPropertyName("x")]
    public required int X { get; set; }

    [JsonPropertyName("y")]
    public required int Y { get; set; }
}

public class CustomTiledObject
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("x")]
    public required int X { get; set; }

    [JsonPropertyName("y")]
    public required int Y { get; set; }

    [JsonPropertyName("width")]
    public required int Width { get; set; }

    [JsonPropertyName("height")]
    public required int Height { get; set; }

    [JsonPropertyName("polygon")]

    public List<CustomTiledObjectPolygonPoint> PolygonPoints { get; set; } = [];
}

public class CustomTiledObjectPolygonPoint
{
    [JsonPropertyName("x")]
    public required int X { get; set; }

    [JsonPropertyName("y")]
    public required int Y { get; set; }
}