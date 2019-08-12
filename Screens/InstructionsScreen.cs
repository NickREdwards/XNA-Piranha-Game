#region File Description
//-----------------------------------------------------------------------------
// MovieScreen.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Threading;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
#endregion

namespace ICGGSAssignment
{
    class InstructionsScreen : GameScreen
    {
        #region Fields

        ContentManager Content;

        // Width and Height of the game window, in pixels
        private int gameWidth;
        private int gameHeight;
        string menuTitle = "Instructions";

        SpriteBatch spriteBatch;

        Texture2D background;
        Texture2D blank;

        // Easier to have an instance of each power up than draw them all manually
        List<PowerUp> powerUps = new List<PowerUp>(7);

        SpriteFont font;
        SpriteFont smallfont;

        #endregion

        #region Initialization

        public InstructionsScreen()
        {
            TransitionOnTime = TimeSpan.FromSeconds(0.5);
            TransitionOffTime = TimeSpan.FromSeconds(0.5);
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        public override void LoadContent()
        {
            if (Content == null)
                Content = new ContentManager(ScreenManager.Game.Services, "Content");

            // Save local copy of SpriteBatch, which can be used to draw textures.
            spriteBatch = ScreenManager.SpriteBatch;
            font = ScreenManager.Font;
            smallfont = Content.Load<SpriteFont>("Fonts/PowerUpFont");

            blank = Content.Load<Texture2D>("blank");

            // Get screen width and height
            gameWidth = ScreenManager.GraphicsDevice.Viewport.Width;
            gameHeight = ScreenManager.GraphicsDevice.Viewport.Height;

            background = Content.Load<Texture2D>("background");

            // Populate powerUps list with one of each
            powerUps.Add(new PowerUp(PowerUpType.Heal5));
            powerUps.Add(new PowerUp(PowerUpType.Heal10));
            powerUps.Add(new PowerUp(PowerUpType.Heal25));
            powerUps.Add(new PowerUp(PowerUpType.NoChase));
            powerUps.Add(new PowerUp(PowerUpType.Repel));
            powerUps.Add(new PowerUp(PowerUpType.SlowFish));
            powerUps.Add(new PowerUp(PowerUpType.SlowMines));

        }

        /// <summary>
        /// Unload graphics content used by the game.
        /// </summary>
        public override void UnloadContent()
        {
            Content.Unload();
        }

        #endregion

        #region Update and Draw

        /// <summary>
        /// Lets the game respond to player input. Unlike the Update method,
        /// this will only be called when the gameplay screen is active.
        /// </summary>
        public override void HandleInput(InputState input)
        {
            if (input == null)
                throw new ArgumentNullException("input");

            // Look up inputs for the active player profile.
            int playerIndex = (int)ControllingPlayer.Value;

            KeyboardState keyboardState = input.CurrentKeyboardStates[playerIndex];
            GamePadState gamePadState = input.CurrentGamePadStates[playerIndex];

            // The game pauses either if the user presses the pause button, or if
            // they unplug the active gamepad. This requires us to keep track of
            // whether a gamepad was ever plugged in, because we don't want to pause
            // on PC if they are playing with a keyboard and have no gamepad at all!
            bool gamePadDisconnected = !gamePadState.IsConnected &&
                                       input.GamePadWasConnected[playerIndex];

            if (input.IsPauseGame(ControllingPlayer) || gamePadDisconnected)
            {
                ScreenManager.AddScreen(new PauseMenuScreen(), ControllingPlayer);
            }
        }

        /// <summary>
        /// This is called when the movie should draw itself.
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            float transitionOffset = (float)Math.Pow(TransitionPosition, 2);

            spriteBatch.Begin();

            spriteBatch.Draw(background, Vector2.Zero, Color.White);

            // Draw a black, semi-transparent, background to see the text better
            spriteBatch.Draw(blank, new Rectangle(10, 140, 582, 285), new Color(Color.Black, 100));

            #region Draw Title
            // Draw the menu title.
            Vector2 titlePosition = new Vector2(426, 80);
            Vector2 titleOrigin = font.MeasureString(menuTitle) / 2;
            //Color titleColor = new Color(192, 192, 192, TransitionAlpha);
            Color titleColor = Color.LightCyan;
            float titleScale = 1.25f;

            titlePosition.Y -= transitionOffset * 100;

            spriteBatch.DrawString(font, menuTitle, titlePosition, titleColor, 0,
                                   titleOrigin, titleScale, SpriteEffects.None, 0);

            #endregion

            spriteBatch.DrawString(smallfont, "The aim of the game is simple: eat the fish and avoid the mines", new Vector2(20, 150), Color.White);

            spriteBatch.DrawString(smallfont, "Gray mines will cause instant damage", new Vector2(20, 165), Color.Tomato);
            spriteBatch.DrawString(smallfont, "Yellow/Green mines will poison you. Poison will cause damage over time.", new Vector2(20, 180), Color.YellowGreen);
            spriteBatch.DrawString(smallfont, "When you are poisoned, one fish will turn green. You must eat this fish to be cured.", new Vector2(40, 195), Color.YellowGreen);

            spriteBatch.DrawString(smallfont, "Power-Ups: these can be eaten to provide different beneficial effects", new Vector2(20, 225), Color.White);

            Vector2 puPos = new Vector2(50, 230);
            foreach (PowerUp pu in powerUps)
            {
                puPos += new Vector2(0, 25);
                spriteBatch.Draw(blank,
                                 puPos,
                                 pu.Rectangle,
                                 pu.Color,
                                 0, new Vector2(30, 10), 1,
                                 SpriteEffects.None,
                                 0);
                spriteBatch.DrawString(smallfont, pu.DisplayName, puPos - new Vector2(29, 11), new Color(Color.Black, 175));
            }

            spriteBatch.DrawString(smallfont, "Health +5",  new Vector2(87, 248), Color.LightGreen);
            spriteBatch.DrawString(smallfont, "Health +10", new Vector2(87, 273), Color.LightGreen);
            spriteBatch.DrawString(smallfont, "Health +25", new Vector2(87, 298), Color.LightGreen);
            spriteBatch.DrawString(smallfont, "No Chase - while active, mines won't chase you when you get close.",   new Vector2(87, 323), Color.LightSkyBlue);
            spriteBatch.DrawString(smallfont, "Repel - while active, mines will move away from you.",      new Vector2(87, 348), Color.LightPink);
            spriteBatch.DrawString(smallfont, "Slow Fish - while active, fish will move slower.",  new Vector2(87, 373), Color.Yellow);
            spriteBatch.DrawString(smallfont, "Slow Mines - while active, mines will move slower.", new Vector2(87, 398), Color.Salmon);

            spriteBatch.End();

            // If the screen is transitioning on or off, fade it out to black.
            if (TransitionPosition > 0)
                ScreenManager.FadeBackBufferToBlack(255 - TransitionAlpha);
        }

        #endregion
    }
}
