using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ICGGSAssignment
{
    // Each mine will move in a random direction until it hits the edges of the screen.
    // It will then choose a new, random, direction to travel in. If the player comes
    // within 200f of the mine it will chase the player.
    public class Mine : AnimatedObject
    {
        private enum Direction
        {
            North,
            NorthEast,
            East,
            SouthEast,
            South,
            SouthWest,
            West,
            NorthWest
        }

        private Direction _direction = Direction.North;

        // Speed is only used when wandering, not when chasing.
        // If chasing, Acceleration and Velocity are used
        private float _speed = 2.5f;

        // If a mine is poison tipped it will poison the piranha on contact
        private bool _poisonTipped = false;
        public bool PoisonTipped { get { return _poisonTipped; } set { _poisonTipped = value; } }

        public override Color Color { get { return (_poisonTipped) ? Color.YellowGreen : Color.Gray; } }

        public Mine(Texture2D tex)
            : base(tex, new Point(60, 60), new Point(1, 11))
        {
            this.Acceleration = 0.015f;
            // Choose a random direction on instantiation
            _direction = RandDirection();
        }

        /// <summary>
        /// Chooses a new direction to travel in
        /// </summary>
        public void ChangeDirection()
        {
            _direction = RandDirection();
        }

        /// <summary>
        /// Picks a random direction to travel in
        /// </summary>
        /// <returns>The new Direction</returns>
        private Direction RandDirection()
        {
            return (Direction)Functions.Rand(0, 8);
        }

        /// <summary>
        /// Overriden UpdateObj to choose whether to use WantedPosition or not
        /// </summary>
        public override void UpdateObj(GameTime gameTime)
        {
            // If the player is within 200f of the mine (and NoChase is inactive) then chase the player
            if (Vector2.Distance(this.Position, Functions.Player.Position) < 200f && !Functions.NoChaseActive)
            {
                // Set the mine's WantedPosition to the player's position
                this.WantedPosition = Functions.Player.Position;
                // Find the difference
                Vector2 d = this.WantedPosition - this.Position;
                // Calculate the velocity. The mine will move slower as it gets closer
                this.Velocity += (d * this.Acceleration) * ((Functions.SlowMinesActive) ? 0.25f : 1f);
                // Prevent rubber banding
                this.Velocity *= 0.5f;

                // If repel is active the mine should move away from WantedPosition, otherwise move toward it
                if (Functions.RepelActive)
                {
                    this.Position -= this.Velocity;
                    // Clamp the position to prevent it moving off screen
                    this.Position = new Vector2(
                        MathHelper.Clamp(this.Position.X, HalfFrameSize.X, Functions.GameSize.X - HalfFrameSize.X),
                        MathHelper.Clamp(this.Position.Y, HalfFrameSize.Y, Functions.GameSize.Y - HalfFrameSize.Y));
                    this.Rotation = -(float)Math.Atan2(d.Y, d.X);
                }
                else
                {
                    this.Position += this.Velocity;
                    this.Rotation = (float)Math.Atan2(d.Y, d.X);
                }
            }
            else
                Wander();
            this.Animate(gameTime);
        }

        /// <summary>
        /// Moves the mine along its current direction at _speed (25% if slow mines is active)
        /// </summary>
        private void Wander()
        {
            float speed = _speed;
            if (Functions.SlowMinesActive)
                speed *= 0.25f;

            switch (_direction)
            {
                case Direction.North:
                    Position += new Vector2(0, -speed);
                    if (Position.Y - HalfFrameSize.Y <= 0)
                        _direction = RandDirection();
                    break;
                case Direction.NorthEast:
                    Position += new Vector2(speed, -speed);
                    if (Position.Y - HalfFrameSize.Y <= 0 || Position.X + HalfFrameSize.X >= Functions.GameSize.X)
                        _direction = RandDirection();
                    break;
                case Direction.East:
                    Position += new Vector2(speed, 0);
                    if (Position.X + HalfFrameSize.X >= Functions.GameSize.X)
                        _direction = RandDirection();
                    break;
                case Direction.SouthEast:
                    Position += new Vector2(speed, speed);
                    if (Position.Y + HalfFrameSize.Y >= Functions.GameSize.Y || Position.X + HalfFrameSize.X >= Functions.GameSize.X)
                        _direction = RandDirection();
                    break;
                case Direction.South:
                    Position += new Vector2(0, speed);
                    if (Position.Y + HalfFrameSize.Y >= Functions.GameSize.Y)
                        _direction = RandDirection();
                    break;
                case Direction.SouthWest:
                    Position += new Vector2(-speed, speed);
                    if (Position.Y + HalfFrameSize.Y >= Functions.GameSize.Y || Position.X - HalfFrameSize.X <= 0)
                        _direction = RandDirection();
                    break;
                case Direction.West:
                    Position += new Vector2(-speed, 0);
                    if (Position.X - HalfFrameSize.X <= 0)
                        _direction = RandDirection();
                    break;
                case Direction.NorthWest:
                    Position += new Vector2(-speed, -speed);
                    if (Position.Y - HalfFrameSize.Y <= 0 || Position.X - HalfFrameSize.X <= 0)
                        _direction = RandDirection();
                    break;
            }
        }
    }
}
