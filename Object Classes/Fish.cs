using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ICGGSAssignment
{
    public class Fish : AnimatedObject
    {
        private bool _cureFish = false;
        public bool CureFish { get { return _cureFish; } set { _cureFish = value; } }

        private Color realColor = Color.White;
        public override Color Color { get { return (_cureFish) ? Color.SpringGreen : realColor; } }

        // If slow fish is active just limit the max velocity
        public override Vector2 TerminalVelocity { get { return (Functions.SlowFishActive) ? new Vector2(3f) : new Vector2(15f); } }

        private List<Fish> flockActive = new List<Fish>();

        // Everything to do with appearance of the fish is set up at instantiation.
        // As fish will follow a flocking algorithm they won't use WantedPosition as their
        // real wanted position is updated constantly to stay with the flock
        public Fish(Texture2D tex, Vector2 pos)
            : base(tex, new Point(32, 10), new Point(1, 3))
        {
            // Set the default frame. In this case the X will always be 0
            this.CurrentFrame = new Point(0, 0);
        }

        /// <summary>
        /// The fish will follow the flock and avoid anything passed into the avoids list
        /// </summary>
        /// <param name="flock">The flock that the fish belongs to</param>
        /// <param name="avoids">Objects to stay out of the way of</param>
        private void FollowFlock(List<Fish> flock, List<GameObject> avoids)
        {
            // Apply all of the flocking rules to the velocity
            this.Velocity += GetAveragePos(flock) + SeparateFlock(flock) + MatchVelocity(flock);

            // Avoid objects in the avoids list
            AvoidObjects(avoids);

            // Clamp the velocity to the fish's terminal velocity
            this.Velocity = new Vector2(MathHelper.Clamp(this.Velocity.X, -TerminalVelocity.X, TerminalVelocity.X),
                                        MathHelper.Clamp(this.Velocity.Y, -TerminalVelocity.Y, TerminalVelocity.Y));

            // Work out the x and y differences between the current position and new position
            Vector2 d = Vector2.Subtract((this.Position + this.Velocity), this.Position);

            // Then work out the arc tan and apply to rotation to make the fish face the direction
            // that it's travelling in
            this.Rotation = (float)Math.Atan2(d.Y, d.X);

            // Apply the new velocity to the position of the fish
            this.Position += this.Velocity * this.Acceleration;
            this.Position = new Vector2(
                (float)(MathHelper.Clamp(this.Position.X + (this.Velocity.X * (this.Acceleration + 0.1f)), 0, Functions.GameSize.X) + Functions.RandDouble(-1, 1)),
                (float)(MathHelper.Clamp(this.Position.Y + (this.Velocity.Y * (this.Acceleration + 0.1f)), 0, Functions.GameSize.Y) + Functions.RandDouble(-1, 1)));
        }

        /// <summary>
        /// This method simply moves the fish out of the way of all objects that
        /// are in the avoid list. They avoid the piranha and the mines.
        /// </summary>
        /// <param name="avoids">A list of objects to avoid</param>
        private void AvoidObjects(List<GameObject> avoids)
        {
            // Typically there won't be many objects in avoids but as this will be called by every
            // fish it will save on processing costs to use a delegate.
            List<GameObject> a = avoids.FindAll(delegate(GameObject o) { return (o.IsActive && (Vector2.Distance(this.Position, o.Position) < 60.0f)); });

            // For each object in the avoid list, subtract its position from the fish's position
            // and add the result back onto the fish's position.
            foreach (GameObject o in a)
            {
                Vector2 di = this.Position - o.Position;
                di.Normalize();
                this.Position += di * MathHelper.Clamp((flockActive.Count / 5), 2f, 8f);
            }
        }

        /// <summary>
        /// Updates the fish. Follows its given flock and avoids the given objects
        /// </summary>
        /// <param name="flock">The flock that the fish belongs to</param>
        /// <param name="avoids">Objects that the fish should avoid</param>
        public void Update(GameTime gameTime, List<Fish> flock, List<GameObject> avoids)
        {
            // If the fish hits the edge of the screen its direction is reversed
            if ((this.Position.X + this.Velocity.X) <= 0 ||
                (this.Position.X + this.Velocity.X) > Functions.GameSize.X)
                this.Velocity = (new Vector2(-this.Velocity.X, this.Velocity.Y) + new Vector2((float)Functions.Rand(-15, 15))) * 0.5f;

            if ((this.Position.Y + this.Velocity.Y) <= 0 ||
                (this.Position.Y + this.Velocity.Y) > Functions.GameSize.Y)
                this.Velocity = (new Vector2(this.Velocity.X, -this.Velocity.Y) + new Vector2((float)Functions.Rand(-15, 15))) * 0.5f;

            // Only check active flock members
            flockActive = Functions.GetActiveObjects(flock);
            FollowFlock(flock, avoids);
            // Clear the list ready for next update
            flockActive.Clear();

            this.UpdateObj(gameTime);
        }

        /// <summary>
        /// Overridden UpdateObj to prevent use of WantedPosition
        /// </summary>
        public override void UpdateObj(GameTime gameTime)
        {
            if (IsActive)
            {
                // Move the object to its new position by adding the velocity
                Position += Velocity;
                base.Animate(gameTime);
            }
        }

        /// <summary>
        /// Rule 1 of Craig Reynold's flocking theory.
        /// Add up the positions of all active flock members and divide by the amount of flock still active.
        /// All flock members will head toward this average position (this constantly changes so negates the 
        /// need for the flock to follow a pre-set path.
        /// </summary>
        /// <param name="flock">The flock that the fish belongs to</param>
        /// <returns>Returns a new Vector2 to adjust the fish's heading.
        /// This value changes depending on the number of active flock members.</returns>
        private Vector2 GetAveragePos(List<Fish> flock)
        {
            Vector2 v = Vector2.Zero;

            // For each fish in the flock, if it's active add its position to the total 'v'.
            foreach (Fish f in flock)
                if (f != this && f.IsActive) v += f.Position;

            int numAlive = flockActive.Count;
            // Divide the total by the number of fish still active
            v /= numAlive - 1;

            // Alter the new heading depending on the number of fish active.
            // This helps to avoid a behaviour that looks like "panic"
            return (v - this.Position) / (1000 - Math.Min((100 * numAlive), 650));
        }

        /// <summary>
        /// Rule 2 of Craig Reynold's flocking theory. 
        /// This rule is used to prevent crowding in the flock.
        /// Essentially all it does is try to make the fish keep a certain distance from its neighbours
        /// </summary>
        /// <param name="flock">The flock that the fish belongs to</param>
        /// <returns>Returns a new Vector2 that alters the heading to avoid crowding</returns>
        private Vector2 SeparateFlock(List<Fish> flock)
        {
            Vector2 v = Vector2.Zero;
            foreach (Fish f in flock)
            {
                if (f != this)
                { 
                    // The larger the flock, the bigger the distance between members
                    if (Vector2.Distance(f.Position, this.Position) < Math.Max(flockActive.Count * 0.45f, 15f))
                        v -= (f.Position - this.Position); // Adjust the new heading to avoid collision/crowding
                }
            }
            return v / 100;
        }

        /// <summary>
        /// Rule 3 of Craig Reynold's flocking theory.
        /// Flock members will try to match the velocity of other members.
        /// This essentially just gets the average velocity and returns it.
        /// </summary>
        /// <param name="flock">The flock that the fish belongs to</param>
        /// <returns>Returns a new Vector2 with the adjusted velocity</returns>
        private Vector2 MatchVelocity(List<Fish> flock)
        {
            Vector2 v = Vector2.Zero;
            foreach (Fish f in flock)
                if (f != this && f.IsActive) v += f.Velocity;
            v /= flockActive.Count - 1;
            return (v - this.Velocity) / 6;
        }
    }
}
