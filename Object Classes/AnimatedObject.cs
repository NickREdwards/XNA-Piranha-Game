using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ICGGSAssignment
{
    public class AnimatedObject : GameObject
    {
        private Point _frameSize;
        public Point FrameSize { get { return _frameSize; } set { _frameSize = value; } }

        private Point _currentFrame = new Point(0, 0);
        public Point CurrentFrame { get { return _currentFrame; } set { _currentFrame = value; } }

        private Point _sheetSize = new Point(1, 1);
        public Point SheetSize { get { return _sheetSize; } }

        private bool _isAnimating = true;
        public virtual bool IsAnimating { get { return _isAnimating; } set { _isAnimating = value; } }

        private int timeSinceLastFrame = 0;
        private int milliSecondsPerFrame = 200;
        public int MilliSecondsPerFrame { get { return milliSecondsPerFrame; } set { milliSecondsPerFrame = value; } }

        public override Vector2 HalfFrameSize
        {
            get
            {
                return new Vector2(FrameSize.X * 0.5f, FrameSize.Y * 0.5f);
            }
        }

        public override Rectangle Rectangle
        {
            get
            {
                return new Rectangle((int)(Position.X - HalfFrameSize.X), (int)(Position.Y - HalfFrameSize.Y),
                                     FrameSize.X, FrameSize.Y);
            }
        }

        public Vector2 Origin { get { return HalfFrameSize; } }

        public AnimatedObject(Texture2D tex, Point frSz)
            : base(tex)
        {
            _frameSize = frSz;
        }

        public AnimatedObject(Texture2D tex, Point frSz, Point shSz)
            : base(tex)
        {
            _frameSize = frSz;
            _sheetSize = shSz;
        }

        public override void UpdateObj(GameTime gameTime)
        {
            this.Animate(gameTime);
            base.UpdateObj(gameTime);
        }

        public virtual void Animate(GameTime gameTime)
        {
            if (this.IsActive && this._isAnimating)
            {
                timeSinceLastFrame += gameTime.ElapsedGameTime.Milliseconds;
                if (timeSinceLastFrame > this.milliSecondsPerFrame)
                {
                    timeSinceLastFrame -= this.milliSecondsPerFrame;
                    if (_currentFrame.Y >= _sheetSize.Y - 1) _currentFrame.Y = 0; else _currentFrame.Y++;
                }
            }
        }
    }
}
