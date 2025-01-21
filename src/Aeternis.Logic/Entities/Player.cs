using Aeternis.Engine.Rendering;
using Aeternis.Logic.World;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Aeternis.Logic.Entities;

public class Player
{
    private float _movement; // Horizontal movement input (-1, 0, or 1)
    private bool _isJumping;
    private bool _wasJumping;
    private bool _isOnGround;
    private bool _isFalling;
    private float _jumpTime;
    private Vector2 _velocity;

    private List<CustomTiledObject> _collisionObjects = [];

    private KeyboardState _previousKeyboardState;
    private readonly AnimationManager _animationManager = new();

    // Constants
    private const float MoveAcceleration = 10000f;
    private const float MaxMoveSpeed = 1750f;
    private const float GravityAcceleration = 3400f;
    private const float MaxFallSpeed = 550f;
    private const float JumpLaunchVelocity = -1200f;
    private const float MaxJumpTime = 0.35f;
    private const float JumpControlPower = 0.14f;
    private const float GroundDragFactor = 0.58f;
    private const float AirDragFactor = 0.65f;

    public Vector2 Position { get; private set; }
    public Rectangle BoundingRectangle => new Rectangle((int)Position.X, (int)Position.Y, 32, 40); // TODO: Either this box is wrong or the player collision detection is wrong

    public void Initialize(Vector2 startPosition, List<CustomTiledObject> collisionObjects, int tileSize)
    {
        Position = startPosition;
        _velocity = Vector2.Zero;

        // Store the collision map and tile size for collision checks
        _collisionObjects = collisionObjects;

        _animationManager.LoadAnimation("idle", "player/idle", 120, 80, 10);
        _animationManager.LoadAnimation("run", "player/run", 120, 80, 10);
        _animationManager.LoadAnimation("jump", "player/jump", 120, 80, 3);
        _animationManager.LoadAnimation("fall", "player/fall", 120, 80, 3);
    }

    public void Update(GameTime gameTime, KeyboardState keyboardState)
    {
        HandleInput(keyboardState);
        ApplyPhysics(gameTime);

        _animationManager.Update(gameTime);
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        _animationManager.Draw(spriteBatch, Position);
    }

    /// <summary>
    /// Handles user input for movement and jumping.
    /// </summary>
    private void HandleInput(KeyboardState keyboardState)
    {
        _movement = 0f;

        HandleMovementInput(keyboardState);
        HandleSinglePressInput(keyboardState);

        _previousKeyboardState = keyboardState;
    }

    private void HandleMovementInput(KeyboardState keyboardState)
    {
        _movement = 0f;
        if (keyboardState.IsKeyDown(Keys.Left) || keyboardState.IsKeyDown(Keys.A))
        {
            _movement = -1f;
            _animationManager.SetFlip(SpriteEffects.FlipHorizontally);
        }
        else if (keyboardState.IsKeyDown(Keys.Right) || keyboardState.IsKeyDown(Keys.D))
        {
            _movement = 1f;
            _animationManager.SetFlip(SpriteEffects.None);
        }

        _isJumping = keyboardState.IsKeyDown(Keys.Space);

        if (_movement == 0 && !_isJumping)
            _animationManager.SetAnimation("idle");
        else if (_isJumping)
            _animationManager.SetAnimation("jump");
        else if (_isFalling) // TODO: Doesnt work
            _animationManager.SetAnimation("fall");
        else if (_movement != 0)
            _animationManager.SetAnimation("run");
    }

    private void HandleSinglePressInput(KeyboardState keyboardState)
    {
        if (keyboardState.IsKeyDown(Keys.F1) && !_previousKeyboardState.IsKeyDown(Keys.F1))
            Configuration.DebugMode = !Configuration.DebugMode;
    }

    /// <summary>
    /// Updates the player's velocity and position based on input, gravity, etc.
    /// </summary>
    private void ApplyPhysics(GameTime gameTime)
    {
        float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;

        Vector2 previousPosition = Position;

        _velocity.X += _movement * MoveAcceleration * elapsed;
        _velocity.Y = MathHelper.Clamp(_velocity.Y + (GravityAcceleration * elapsed), -MaxFallSpeed, MaxFallSpeed);

        _velocity.Y = DoJump(_velocity.Y, gameTime);

        if (_isOnGround)
            _velocity.X *= GroundDragFactor;
        else
            _velocity.X *= AirDragFactor;

        _velocity.X = MathHelper.Clamp(_velocity.X, -MaxMoveSpeed, MaxMoveSpeed);

        Position += _velocity * elapsed;
        Position = new Vector2((float)Math.Round(Position.X), (float)Math.Round(Position.Y));

        HandleCollisions();

        if (Position.X == previousPosition.X)
            _velocity.X = 0;

        if (Position.Y == previousPosition.Y)
            _velocity.Y = 0;

        _isFalling = !_isOnGround && _velocity.Y > 0;
    }

    /// <summary>
    /// Calculates the Y velocity accounting for jumping.
    /// </summary>
    private float DoJump(float velocityY, GameTime gameTime)
    {
        if (_isJumping)
        {
            if ((!_wasJumping && _isOnGround) || _jumpTime > 0.0f)
            {
                _jumpTime += (float)gameTime.ElapsedGameTime.TotalSeconds;
            }

            if (0.0f < _jumpTime && _jumpTime <= MaxJumpTime)
            {
                velocityY = JumpLaunchVelocity * (1.0f - (float)Math.Pow(_jumpTime / MaxJumpTime, JumpControlPower));
            }
            else
            {
                _jumpTime = 0.0f;
            }
        }
        else
        {
            _jumpTime = 0.0f;
        }

        _wasJumping = _isJumping;

        return velocityY;
    }

    private void HandleCollisions()
    {
        Rectangle bounds = BoundingRectangle;
        _isOnGround = false;

        foreach (var collisionObject in _collisionObjects)
        {
            Rectangle collisionBounds = new Rectangle(
                collisionObject.X,
                collisionObject.Y,
                collisionObject.Width,
                collisionObject.Height);

            Vector2 depth = GetIntersectionDepth(bounds, collisionBounds);

            if (depth != Vector2.Zero)
            {
                ResolveCollision(ref bounds, depth);
            }
        }
    }

    private void ResolveCollision(ref Rectangle bounds, Vector2 depth)
    {
        float absDepthX = Math.Abs(depth.X);
        float absDepthY = Math.Abs(depth.Y);

        if (absDepthY < absDepthX)
        {
            if (_velocity.Y > 0 && bounds.Bottom <= bounds.Bottom + depth.Y + 1)
                _isOnGround = true;

            Position = new Vector2(Position.X, Position.Y + depth.Y);
        }
        else
        {
            Position = new Vector2(Position.X + depth.X, Position.Y);
        }

        bounds = BoundingRectangle;
    }

    /// <summary>
    /// Calculates intersection depth between two rectangles.
    /// </summary>
    private Vector2 GetIntersectionDepth(Rectangle rectA, Rectangle rectB)
    {
        float minDistanceX = (rectA.Width / 2.0f) + (rectB.Width / 2.0f);
        float minDistanceY = (rectA.Height / 2.0f) + (rectB.Height / 2.0f);

        float centerA_X = rectA.Left + (rectA.Width / 2.0f);
        float centerA_Y = rectA.Top + (rectA.Height / 2.0f);
        float centerB_X = rectB.Left + (rectB.Width / 2.0f);
        float centerB_Y = rectB.Top + (rectB.Height / 2.0f);

        float distanceX = centerA_X - centerB_X;
        float distanceY = centerA_Y - centerB_Y;

        if (Math.Abs(distanceX) >= minDistanceX || Math.Abs(distanceY) >= minDistanceY)
            return Vector2.Zero;

        float depthX = distanceX > 0 ? minDistanceX - distanceX : -minDistanceX - distanceX;
        float depthY = distanceY > 0 ? minDistanceY - distanceY : -minDistanceY - distanceY;

        return new Vector2(depthX, depthY);
    }
}
