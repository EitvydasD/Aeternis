using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Aeternis.Engine.Rendering;
public class AnimationManager(double frameTime = 0.1)
{
    private readonly Dictionary<string, (Texture2D Texture, int FrameWidth, int FrameHeight, int FrameCount)> _animations = [];
    private string _currentAnimation = string.Empty;
    private int _currentFrame = 0;
    private readonly double _frameTime = frameTime;
    private double _timeSinceLastFrame = 0;
    private SpriteEffects _spriteEffects = SpriteEffects.None;

    // Load animations
    public void LoadAnimation(string animationName, string textureName, int frameWidth, int frameHeight, int frameCount)
    {
        if (!_animations.ContainsKey(animationName))
        {
            Texture2D texture = Dependencies.ContentManager.Load<Texture2D>(textureName);
            _animations[animationName] = (texture, frameWidth, frameHeight, frameCount);
        }
    }

    // Set the current animation
    public void SetAnimation(string animationName)
    {
        if (_animations.ContainsKey(animationName) && _currentAnimation != animationName)
        {
            _currentAnimation = animationName;
            _currentFrame = 0;
            _timeSinceLastFrame = 0;
        }
    }

    // Set sprite flipping
    public void SetFlip(SpriteEffects effect)
    {
        _spriteEffects = effect;
    }

    // Update animation state (handle timing)
    public void Update(GameTime gameTime)
    {
        if (_animations.ContainsKey(_currentAnimation))
        {
            _timeSinceLastFrame += gameTime.ElapsedGameTime.TotalSeconds;

            if (_timeSinceLastFrame >= _frameTime)
            {
                _timeSinceLastFrame = 0;
                _currentFrame++;
                if (_currentFrame >= _animations[_currentAnimation].FrameCount)
                {
                    _currentFrame = 0;
                }
            }
        }
    }

    // Draw the current animation
    public void Draw(SpriteBatch spriteBatch, Vector2 position, float scale = 1.0f, float rotation = 0f, Color? color = null)
    {
        if (_animations.ContainsKey(_currentAnimation))
        {
            var (texture, frameWidth, frameHeight, _) = _animations[_currentAnimation];
            int column = _currentFrame % (texture.Width / frameWidth);
            int row = _currentFrame / (texture.Width / frameWidth);

            Rectangle sourceRectangle = new Rectangle(column * frameWidth, row * frameHeight, frameWidth, frameHeight);
            spriteBatch.Draw(texture, position, sourceRectangle, color ?? Color.White, rotation, Vector2.Zero, scale, _spriteEffects, 0f);
        }
    }
}
