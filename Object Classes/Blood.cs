using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ICGGSAssignment
{
    // This class is also used to display the "poison cloud" when the player gets poisoned
    public class Blood : AnimatedObject
    {
        private int timeSinceLastFrame = 0;

        public Blood(Texture2D tex, Vector2 pos)
            : base(tex, new Point(239, 178), new Point(1, 7))
        {
            this.MilliSecondsPerFrame = 90;
            this.Position = pos;
            this.Color = new Color(Color.Red, 175);
        }

        public Blood(Texture2D tex, Vector2 pos, Color col)
            : base(tex, new Point(239, 178), new Point(1, 7))
        {
            this.MilliSecondsPerFrame = 90;
            this.Position = pos;
            this.Color = col;
        }

        public override void Animate(GameTime gameTime)
        {
            if (this.IsActive)
            {
                timeSinceLastFrame += gameTime.ElapsedGameTime.Milliseconds;
                if (timeSinceLastFrame > this.MilliSecondsPerFrame)
                {
                    timeSinceLastFrame -= this.MilliSecondsPerFrame;
                    if (CurrentFrame.Y >= SheetSize.Y - 1) this.IsActive = false; else CurrentFrame = new Point(CurrentFrame.X, CurrentFrame.Y + 1);
                }
            }
        }
    }
}