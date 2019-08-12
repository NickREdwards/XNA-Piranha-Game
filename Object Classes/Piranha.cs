using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ICGGSAssignment
{
    public class Piranha : AnimatedObject
    {
        // Make sure the piranha's health doesn't go under 0, or over 100
        private int _health = 100;
        public int Health { get { return (int)MathHelper.Clamp(_health, 0, 100); } set { _health = value; } }

        private int _invulTicker = 0;
        private int _invulEndsAt = 180; // Invul lasts for 6 seconds (at 30fps)

        // Contains a list of mines that have already hurt the player
        // so that if the collision is called more than once it won't 
        // reduce the player's health more than once
        private List<Mine> alreadyHurtBy = new List<Mine>();

        public override float Acceleration { get { return 0.075f; } }

        // Piranha will remain poisoned until a "cure" is found
        private bool _poisoned = false;
        public bool Poisoned { get { return _poisoned; } set { _poisoned = value; } }
        private int _poisonTick = 0;

        private int timeSinceLastFrame = 0;

        // Only animate the piranha when its moving
        public override bool IsAnimating { get { return /*!base.AtWantedPosition();*/(this.Velocity != Vector2.Zero); } }

        public override Rectangle Rectangle
        {
            get { return new Rectangle((int)(Position.X - 20), (int)(Position.Y - 20), 30, 30); }
        }

        public Piranha(Texture2D tex)
            : base(tex, new Point(80, 55), new Point(2, 1))
        {
            this.MilliSecondsPerFrame = 350;
        }

        /// <summary>
        /// Return the piranha back to its default state
        /// </summary>
        public void Reset()
        {
            this._health = 100;
            this._poisoned = false;
            this._poisonTick = 0;
            this.Position = new Vector2(Functions.GameSize.X / 2 - this.HalfFrameSize.X,
                                        Functions.GameSize.Y / 2 - this.HalfFrameSize.Y);
            this.WantedPosition = this.Position;
            this.Velocity = Vector2.Zero;
            this.Rotation = 0;
            this.SpriteEffects = SpriteEffects.None;
            this._invulTicker = 0;
        }

        public bool IsInvul { get { return (_invulTicker < _invulEndsAt); } }

        public override void UpdateObj(GameTime gameTime)
        {
            #region Poison stuff
            if (_poisoned)
            {
                this.Color = Color.GreenYellow;
                _poisonTick++;
                if (_poisonTick % 250 == 0) { this.DoPoison(); this._poisonTick = 0; }
            }
            else
                this.Color = Color.White;
            #endregion

            // If piranha is invul it is drawn with some transparency
            if (IsInvul)
                this.Color = new Color(this.Color, 175);
            else
                this.Color = new Color(this.Color, 255);

            // If the piranha is moving left, flip the sprite so it's not upside-down
            if (this.Velocity.X < 0)
                this.SpriteEffects = SpriteEffects.FlipVertically;
            else
                this.SpriteEffects = SpriteEffects.None;

            // Only update ticker if piranha is invul
            if (IsInvul)
                _invulTicker++;
            else
                base.UpdateObj(gameTime);
        }

        /// <summary>
        /// Cause poison damage, between 2 and 8
        /// </summary>
        private void DoPoison()
        {
            this._health -= Functions.Rand(2, 8);
        }

        /// <summary>
        /// Apply damage to the piranha
        /// </summary>
        /// <param name="amnt">Amount of damage</param>
        /// <param name="from">The source of the damage</param>
        public void DoDamage(int amnt, Mine from)
        {
            if (!alreadyHurtBy.Contains(from))
            {
                this._health -= amnt;
                alreadyHurtBy.Add(from);
            }
        }

        public override void Animate(GameTime gameTime)
        {
            if (this.IsActive && this.IsAnimating)
            {
                timeSinceLastFrame += gameTime.ElapsedGameTime.Milliseconds;
                if (timeSinceLastFrame > this.MilliSecondsPerFrame)
                {
                    timeSinceLastFrame -= this.MilliSecondsPerFrame;
                    CurrentFrame = new Point(1 - CurrentFrame.X, CurrentFrame.Y);
                }
            }
        }
    }
}
