using Aeternis.Engine;
using Aeternis.Engine.Rendering;
using Aeternis.Logic.Entities;
using Aeternis.Logic.World;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Aeternis.Client;

public class Aeternis : Game
{
    private readonly GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;

    private Texture2D _blockAtlas;
    private readonly Player _player = new Player();
    private readonly Camera _camera = new Camera();
    private Texture2D _collisionTexture;

    private const int TileSize = 32;

    private MapLoader _mapLoader;

    public Aeternis()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        Dependencies.ContentManager = Content;
    }

    protected override void Initialize()
    {
        IsMouseVisible = true;

        _graphics.PreferredBackBufferWidth = 1024; // Default window size
        _graphics.PreferredBackBufferHeight = 768;
        _graphics.ApplyChanges();

        Window.AllowUserResizing = true; // Enable resizing
        Window.ClientSizeChanged += OnResize;

        _mapLoader = new MapLoader();

        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        _blockAtlas = Content.Load<Texture2D>("tiles/blocks/tiles_grass");

        // Load map using MapLoader
        _mapLoader.Load("Content/map.tmj");

        _collisionTexture = CreateWhiteTexture();

        var startPosition = new Vector2(_blockAtlas.Width / 2, _blockAtlas.Height);

        // Initialize the player
        _player.Initialize(startPosition, _mapLoader.CollisionObjects, TileSize);
    }

    private Texture2D CreateWhiteTexture()
    {
        Texture2D texture = new Texture2D(GraphicsDevice, 1, 1);
        texture.SetData(new[] { Color.White });
        return texture;
    }

    protected override void Update(GameTime gameTime)
    {
        var keyboardState = Keyboard.GetState();

        // Update the player with collision detection
        _player.Update(gameTime, keyboardState);

        _camera.Update(
         _player.Position,
         _graphics.PreferredBackBufferWidth,
         _graphics.PreferredBackBufferHeight
     );

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);

        // Begin rendering with the camera's transform matrix
        _spriteBatch.Begin(transformMatrix: _camera.Transform);

        // Draw the map
        _mapLoader.Draw(_spriteBatch, _blockAtlas, _camera.Position, _collisionTexture);

        // Draw the player
        _player.Draw(_spriteBatch);

        _spriteBatch.End();

        base.Draw(gameTime);
    }

    private void OnResize(object sender, EventArgs e)
    {
        // Update the graphics dimensions
        _graphics.PreferredBackBufferWidth = Window.ClientBounds.Width;
        _graphics.PreferredBackBufferHeight = Window.ClientBounds.Height;
        _graphics.ApplyChanges();

        // Update the camera with the new viewport size
        _camera.Update(_player.Position, _graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight);
    }
}
