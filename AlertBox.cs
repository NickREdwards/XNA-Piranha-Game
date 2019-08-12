using System;
using Microsoft.Xna.Framework.Graphics;

namespace ICGGSAssignment
{
    // This will be set up as a Singleton class as
    // only one instance of it is required.
    public sealed class AlertBox
    {
        private string _message = "";
        public string Message { get { return _message; } }

        // _timeout is when the alert will start to fade off the screen
        private DateTime _timeout = DateTime.Now;

        private Color _color = Color.White;
        public Color Color { get { return new Color(_color, _alpha); } }

        // Reduced gradually after timeout to make it fade out
        private byte _alpha = 255;

        public void Show(string msg, Color col)
        {
            _message = msg;
            _color = col;
            // Reset the transparency
            _alpha = 255;
            // Set timeout to 1 second
            _timeout = DateTime.Now + TimeSpan.FromSeconds(1);
        }

        public void Update()
        {
            // No point carrying on if there's no message to display
            if (_message == "") return;

            if (DateTime.Now >= _timeout)
            {
                // Once _alpha is 0 or less, _message is emptied
                if (_alpha > 0)
                    _alpha -= 5;
                else
                    _message = "";
            }
        }

        #region Singleton Stuff
        private static AlertBox instance = null;

        AlertBox()
        {
        }

        public static AlertBox Instance
        {
            get
            {
                if (instance == null)
                    instance = new AlertBox();
                return instance;
            }
        }
        #endregion
    }
}
