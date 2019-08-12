#region File Description
//-----------------------------------------------------------------------------
// MessageBoxScreen.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace ICGGSAssignment
{
    /// <summary>
    /// A popup message box screen, used to display "are you sure?"
    /// confirmation messages.
    /// </summary>
    class WonGameScreen : GameScreen
    {
        #region Fields

        Texture2D gradientTexture;
        DateTime offAt;

        #endregion

        #region Initialization

        /// <summary>
        /// Shows the Game Over screen
        /// </summary>
        public WonGameScreen()
        {
            IsPopup = true;

            TransitionOnTime = TimeSpan.FromSeconds(0.2);
            TransitionOffTime = TimeSpan.FromSeconds(0.2);

            offAt = DateTime.Now + TimeSpan.FromSeconds(3.0);
        }

        /// <summary>
        /// Loads graphics content for this screen. This uses the shared ContentManager
        /// provided by the Game class, so the content will remain loaded forever.
        /// Whenever a subsequent MessageBoxScreen tries to load this same content,
        /// it will just get back another reference to the already loaded data.
        /// </summary>
        public override void LoadContent()
        {
            ContentManager content = ScreenManager.Game.Content;

            gradientTexture = content.Load<Texture2D>("gradient");
        }


        #endregion

        #region Draw


        /// <summary>
        /// Draws the message box.
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            if (DateTime.Now >= offAt)
                ExitScreen();
            else
            {
                SpriteBatch spriteBatch = ScreenManager.SpriteBatch;
                SpriteFont font = ScreenManager.Font;

                // Darken down any other screens that were drawn beneath the popup.
                ScreenManager.FadeBackBufferToBlack(TransitionAlpha * 2 / 3);

                // Center the message text in the viewport.
                Viewport viewport = ScreenManager.GraphicsDevice.Viewport;
                Vector2 viewportSize = new Vector2(viewport.Width, viewport.Height);
                Vector2 textSize = font.MeasureString("Congratulations!\r\nYou won the game!");
                Vector2 textPosition = (viewportSize - textSize) / 2;

                // The background includes a border somewhat larger than the text itself.
                const int hPad = 32;
                const int vPad = 16;

                Rectangle backgroundRectangle = new Rectangle((int)textPosition.X - hPad,
                                                              (int)textPosition.Y - vPad,
                                                              (int)textSize.X + hPad * 2,
                                                              (int)textSize.Y + vPad * 2);

                // Fade the popup alpha during transitions.
                Color color = new Color(255, 255, 255, TransitionAlpha);

                spriteBatch.Begin();

                // Draw the background rectangle.
                spriteBatch.Draw(gradientTexture, backgroundRectangle, color);

                // Draw the message box text.
                spriteBatch.DrawString(font, "Congratulations!\r\nYou won the game!", textPosition, color);

                spriteBatch.End();
            }
        }

        #endregion
    }
}
