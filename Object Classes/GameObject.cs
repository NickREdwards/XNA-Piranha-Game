using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ICGGSAssignment
{
    /// <summary>
    /// The base object class that all other object inherit from
    /// </summary>
    public class GameObject
    {
        private Texture2D _texture;
        public Texture2D Texture { get { return _texture; } set { _texture = value; } }

        private Vector2 _position = Vector2.Zero;
        public Vector2 Position { get { return _position; } set { _position = value; } }

        private Vector2 _wantedPosition = Vector2.Zero;
        public Vector2 WantedPosition { get { return _wantedPosition; } set { _wantedPosition = value; } }

        private Vector2 _velocity = Vector2.Zero;
        public Vector2 Velocity { get { return _velocity; } set { _velocity = value; } }

        private Vector2 _terminalVelocity = new Vector2(20.0f);
        public virtual Vector2 TerminalVelocity { get { return _terminalVelocity; } set { _terminalVelocity = value; } }

        private float _acceleration = 0.025f;
        public virtual float Acceleration { get { return _acceleration; } set { _acceleration = value; } }

        private float _rotation = 0f;
        public float Rotation { get { return _rotation; } set { _rotation = value; } }

        private float _scale = 1.0f;
        public float Scale { get { return _scale; } set { _scale = value; } }

        private bool _isMoving = false;
        public bool IsMoving { get { return _isMoving; } set { _isMoving = value; } }

        private bool _isActive = true;
        public bool IsActive { get { return _isActive; } set { _isActive = value; } }

        private SpriteEffects _spriteEffects = SpriteEffects.None;
        public SpriteEffects SpriteEffects { get { return _spriteEffects; } set { _spriteEffects = value; } }

        private Color _color = Color.White;
        public virtual Color Color { get { return _color; } set { _color = value; } }

        public virtual Vector2 HalfFrameSize
        {
            get
            {
                return new Vector2(_texture.Width * 0.5f, _texture.Height * 0.5f);
            }
        }

        public virtual Rectangle Rectangle
        {
            get
            {
                return new Rectangle((int)(_position.X - HalfFrameSize.X), (int)(_position.Y - HalfFrameSize.Y),
                                     _texture.Width, _texture.Height);
            }
        }

        public virtual Vector2 Origin { get { return HalfFrameSize; } }

        public GameObject(Texture2D tex)
        {
            _texture = tex;
        }

        public virtual void UpdateObj(GameTime gameTime)
        {
            // Only need to update the object if it is active on screen
            if (IsActive)
            {
                // If the object has already reached its destination, make sure it's velocity is zero
                if (Functions.ObjectReachedDestination(this)) { _velocity = Vector2.Zero; _isMoving = false; }
                else
                {
                    Vector2 diff = Vector2.Subtract(_wantedPosition, _position);
                    _rotation = (float)Math.Atan2(diff.Y, diff.X);
                    // Velocity is greater if the wanted position is further away
                    _velocity += diff * _acceleration;
                    // Half the velocity to prevent rubber banding
                    _velocity *= 0.5f;
                    _isMoving = true;
                }

                // Move the object to its new position by adding the velocity
                _position += _velocity;

                // Clamp the position to prevent the object from leaving the screen
                _position = new Vector2(
                    MathHelper.Clamp(_position.X, HalfFrameSize.X, Functions.GameSize.X - HalfFrameSize.X),
                    MathHelper.Clamp(_position.Y, HalfFrameSize.Y, Functions.GameSize.Y - HalfFrameSize.Y));
            }
        }

        public virtual void Destroy()
        {
            this._isActive = false;
        }
    }
}
