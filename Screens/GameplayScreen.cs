#region File Description
//-----------------------------------------------------------------------------
// GameplayScreen.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;
#endregion

namespace ICGGSAssignment
{
    /// <summary>
    /// ICGGS XNA Assignment 2010
    /// Assignment programmed by Nick Edwards
    /// Student No: 09005259
    /// Staffordshire University
    /// </summary>

    public class GameplayScreen : GameScreen
    {
        #region Fields

        ContentManager      Content;
        SpriteBatch         spriteBatch;

        SpriteFont          spriteFont;                                 // Font used to display alerts at the top
        SpriteFont          powerUpFont;                                // Font used on the power-ups

        SoundEffect         backgroundSound;                            // The background sound that will loop
        SoundEffectInstance backgroundSoundInstance;                    // The instance of the bg sound so it can be looped
        SoundEffect         whaleSound;                                 // A whale sound that will play periodically
        SoundEffect         biteSound;                                  // Biting sound that is played when player eats a fish
        SoundEffect         hurtSound;                                  // A sound that is played when player gets hurt
        SoundEffect         powerUpSound;                               // A sound that is played when player gets a power-up

        Effect              refractionEffect;                           // A ripple effect to create an underwater effect

        int                 currentLevel    = 1;                        // This just holds the currentLevel in integer format
        int                 maxLevel        = 5;                        // This is the maximum level the game goes to
        int[]               numFish         = { 30, 40, 50, 65, 100 };  // Number of fish in each level
        int[]               numMines        = { 10, 15, 18, 23, 30 };   // Number of mines in each level
        int                 powerUpTicker   = 1;                        // Just used to know when to generate a new power-up
        int                 whaleSndTicker  = 0;                        // Controls when the whale sound is played

        Piranha             piranha;                                    // The piranha object. Only animated when moving
        PowerUp             powerUp;                                    // Only one instance of PowerUp created, and different types are generated

        List<Blood>         blood           = new List<Blood>();        // Contains a list of Blood. Cleared at every ResetGame()
        List<Fish>          fish            = new List<Fish>();         // Contains a list of all Fish objects.
        List<Fish>          activeFish      = new List<Fish>();         // Contains a list of all Fish that are currently active
        List<GameObject>    fishAvoids      = new List<GameObject>();   // Contains a list of objects that the fish should avoid (mines/piranha)
        List<Mine>          mines           = new List<Mine>();         // Contains a list of all Mine objects

        Texture2D           gameBackground;                             // The underwater scene for the game
        Texture2D           fog;                                        // A foggy image displayed with transparency over the game
        Texture2D           blank;                                      // A blank image used for drawing UI and PowerUps
        Texture2D           bloodTex;                                   // Animated blood texture
        Texture2D           waterfallTexture;                           // Waterfall texture used for refraction effect

        Random              rand            = new Random();

        KeyboardState       prevKeyboard;                               // Used to prevent holding down certain keys
        MouseState          prevMouse;                                  // Used for mouse control

        bool                canUseMouse     = false;                    // Determines whether or not mouse control is active
        bool                isGameOver      = false;                    // Helps to prevent any exceptions
        bool                godMode         = false;

        #endregion

        #region Initialization

        public GameplayScreen()
        {
            // Setup on and off transition times
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

            #region Load random things
            spriteFont = Content.Load<SpriteFont>("Fonts/menufont");
            powerUpFont = Content.Load<SpriteFont>("Fonts/PowerUpFont");
            refractionEffect = Content.Load<Effect>("Refraction");
            #endregion

            #region Load Sounds
            backgroundSound = Content.Load<SoundEffect>("Sounds/backgroundSound");
            whaleSound = Content.Load<SoundEffect>("Sounds/whaleSound");
            biteSound = Content.Load<SoundEffect>("Sounds/biteSound");
            hurtSound = Content.Load<SoundEffect>("Sounds/hurtSound");
            powerUpSound = Content.Load<SoundEffect>("Sounds/powerUpSound");
            #endregion

            #region Load Texture2D's
            gameBackground = Content.Load<Texture2D>("gamebg");
            fog = Content.Load<Texture2D>("fog");
            blank = Content.Load<Texture2D>("blank");
            bloodTex = Content.Load<Texture2D>("Sprites/Blood");
            waterfallTexture = Content.Load<Texture2D>("waterfall");
            piranha = new Piranha(Content.Load<Texture2D>("Sprites/Piranha"));
            Texture2D fishTex = Content.Load<Texture2D>("Sprites/Fish");
            Texture2D mineTex = Content.Load<Texture2D>("Sprites/Mine");
            #endregion

            #region Assign values to global variables
            Functions.GameSize = new Point(ScreenManager.GraphicsDevice.Viewport.Width, ScreenManager.GraphicsDevice.Viewport.Height);
            Functions.Player = piranha;
            #endregion

            #region Set-up other objects
            for (int x = 0; x < numFish[maxLevel - 1]; x++)
                fish.Add(new Fish(fishTex, Functions.RandScreenPos()));

            for (int x = 0; x < numMines[maxLevel - 1]; x++)
                mines.Add(new Mine(mineTex));

            foreach (Mine mine in mines)
                fishAvoids.Add(mine);
            fishAvoids.Add(piranha);

            powerUp = new PowerUp(PowerUpType.Heal5);
            #endregion

            #region Set up background sound
            backgroundSoundInstance = backgroundSound.CreateInstance();
            backgroundSoundInstance.IsLooped = true;
            backgroundSoundInstance.Volume = 0.1f;
            #endregion

            // Reset everything and prepare the game/level
            ResetGame();
            ScreenManager.Game.ResetElapsedTime();
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
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        public override void Update(GameTime gameTime, bool otherScreenHasFocus,
                                                       bool coveredByOtherScreen)
        {
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);

            // IsScreenActive is a custom method to return a boolean
            // value for whether the screen is active or not
            if (IsActive && !ScreenManager.IsScreenActive(typeof(MessageBoxScreen)) && !isGameOver)
            {
                #region Sounds
                backgroundSoundInstance.Play();
                if (whaleSndTicker % 1800 == 0) // Every 60 seconds
                {
                    // Play the sound at 10% volume
                    whaleSound.Play(0.1f, 0, 0);
                    whaleSndTicker = 0;
                }
                // Increment the ticker
                whaleSndTicker++;
                #endregion

                // If the piranha dies, the player drops down a level
                // but if they were on level 1 it's Game Over.
                if (piranha.Health <= 0)
                {
                    if (currentLevel < 2)
                        GameOver();
                    else
                    {
                        currentLevel--;
                        ResetGame();
                        DisplayLevelMessage();
                    }
                }

                // The alert box is updated every frame
                // but it isn't always active
                Functions.Alert.Update();

                // Update/animate the piranha
                piranha.UpdateObj(gameTime);

                // Don't bother updating anything until invul time is up
                if (!piranha.IsInvul)
                {
                    #region Update power-up
                    // Power-ups are generated every 900 ticks
                    // which is 1 every 30 seconds at 30fps
                    if (powerUpTicker % 900 == 0 && !powerUp.Running)
                    {
                        powerUp.ResetAndActivate();
                        powerUpTicker = 0;
                    }
                    // Increment the ticket and update the power-up
                    powerUpTicker++;
                    powerUp.Update();
                    #endregion

                    #region Update all other objects
                    foreach (Fish f in fish)
                        if (f.IsActive) f.Update(gameTime, fish, fishAvoids);

                    foreach (Mine mine in mines)
                        if (mine.IsActive) mine.UpdateObj(gameTime);

                    foreach (Blood b in blood)
                    {
                        if (b.IsActive)
                            b.Animate(gameTime);
                    }
                    #endregion

                    CheckCollisions();
                }
            }
        }

        /// <summary>
        /// Lets the game respond to player input. Unlike the Update method,
        /// this will only be called when the gameplay screen is active.
        /// </summary>
        public override void HandleInput(InputState input)
        {
            if (input == null)
                throw new ArgumentNullException("input");

            KeyboardState keyboardState = Keyboard.GetState();
            MouseState mouseState = Mouse.GetState();

            if (input.IsPauseGame(ControllingPlayer))
                ScreenManager.AddScreen(new PauseMenuScreen(), ControllingPlayer);
            else
            {
                if (!piranha.IsInvul)
                {
                    #region Keyboard toggle keys
                    if (keyboardState.IsKeyDown(Keys.R) && prevKeyboard.IsKeyUp(Keys.R))
                        ResetGame();
                    if (keyboardState.IsKeyDown(Keys.M) && prevKeyboard.IsKeyUp(Keys.M))
                        canUseMouse = !canUseMouse;
                    if (keyboardState.IsKeyDown(Keys.G) && prevKeyboard.IsKeyUp(Keys.G))
                        godMode = !godMode;
                    #endregion

                    #region Mouse control
                    if (canUseMouse)
                    {
                        // Mouse control, can travel in all directions
                        Vector2 mousePos = new Vector2(mouseState.X, mouseState.Y);
                        Vector2 oldMousePos = new Vector2(prevMouse.X, prevMouse.Y);
                        if (mousePos != oldMousePos)
                            piranha.WantedPosition = new Vector2(mouseState.X, mouseState.Y);
                    }
                    #endregion

                    #region Keyboard directional control keys
                    if (keyboardState.IsKeyDown(Keys.Up))
                    { piranha.WantedPosition += new Vector2(0, -18); }
                    else if (keyboardState.IsKeyDown(Keys.Down))
                    { piranha.WantedPosition += new Vector2(0, 18); }
                    else if (keyboardState.IsKeyDown(Keys.Left))
                    { piranha.WantedPosition += new Vector2(-18, 0); }
                    else if (keyboardState.IsKeyDown(Keys.Right))
                    { piranha.WantedPosition += new Vector2(18, 0); }
                    #endregion

                    // Clamp the wanted position to the screen bounds (with a little leeway)
                    piranha.WantedPosition = new Vector2(
                        MathHelper.Clamp(piranha.WantedPosition.X, -30, Functions.GameSize.X + 30),
                        MathHelper.Clamp(piranha.WantedPosition.Y, -30, Functions.GameSize.Y + 30));
                }
            }

            prevKeyboard = keyboardState;
            prevMouse = mouseState;
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Immediate, SaveStateMode.None);
            ScreenManager.GraphicsDevice.Textures[1] = waterfallTexture;
            refractionEffect.Parameters["DisplacementScroll"].SetValue(Functions.MoveInCircle(gameTime, 0.2f));
            refractionEffect.Begin();
            refractionEffect.CurrentTechnique.Passes[0].Begin();

            // Draw the background first, covers the entire viewport
            spriteBatch.Draw(gameBackground, Vector2.Zero, Color.White);

            #region Draw power-up
            if (powerUp.IsActive)
            {
                spriteBatch.Draw(blank,
                                 powerUp.Position,
                                 powerUp.Rectangle,
                                 powerUp.Color,
                                 0, new Vector2(30, 10), 1,
                                 SpriteEffects.None,
                                 0);
                spriteBatch.DrawString(powerUpFont, powerUp.DisplayName, powerUp.Position - new Vector2(29, 11), new Color(Color.Black, 175));
            }
            #endregion

            #region Draw blood effects
            foreach (Blood b in blood)
            {
                if (b.IsActive)
                {
                    spriteBatch.Draw(b.Texture, b.Position,
                                     new Rectangle(b.CurrentFrame.X * b.FrameSize.X,
                                                   b.CurrentFrame.Y * b.FrameSize.Y,
                                                   b.FrameSize.X,
                                                   b.FrameSize.Y),
                                     b.Color, b.Rotation, b.Origin, b.Scale, b.SpriteEffects, 0);
                }
            }
            #endregion

            #region Draw fish
            foreach (Fish f in fish)
            {
                if (f.IsActive)
                {
                    spriteBatch.Draw(f.Texture, f.Position,
                                     new Rectangle(f.CurrentFrame.X * f.FrameSize.X,
                                                   f.CurrentFrame.Y * f.FrameSize.Y,
                                                   f.FrameSize.X,
                                                   f.FrameSize.Y),
                                     f.Color, f.Rotation, f.Origin, f.Scale, f.SpriteEffects, 0);
                }
            }
            #endregion

            #region Draw piranha
            if (piranha.IsActive)
            {
                spriteBatch.Draw(piranha.Texture, piranha.Position,
                                 new Rectangle(piranha.CurrentFrame.X * piranha.FrameSize.X,
                                               piranha.CurrentFrame.Y * piranha.FrameSize.Y,
                                               piranha.FrameSize.X,
                                               piranha.FrameSize.Y),
                                               piranha.Color, piranha.Rotation, piranha.Origin, piranha.Scale, piranha.SpriteEffects, 0);
            }
            #endregion

            #region Draw mines
            foreach (Mine mine in mines)
            {
                if (mine.IsActive)
                {
                    spriteBatch.Draw(mine.Texture, mine.Position,
                                     new Rectangle(mine.CurrentFrame.X * mine.FrameSize.X,
                                                   mine.CurrentFrame.Y * mine.FrameSize.Y,
                                                   mine.FrameSize.X,
                                                   mine.FrameSize.Y),
                                     mine.Color, mine.Rotation, mine.Origin, mine.Scale, mine.SpriteEffects, 0);
                }
            }
            #endregion

            spriteBatch.Draw(fog, Vector2.Zero, new Color(Color.SteelBlue, 100));

            spriteBatch.End();
            refractionEffect.CurrentTechnique.Passes[0].End();
            refractionEffect.End();
            spriteBatch.Begin();

            #region Draw UI
            if (powerUp.Running)
            {
                spriteBatch.Draw(blank, new Rectangle(10, 10, 210, 30), new Color(Color.Black, 100));
                int left = (powerUp.EffectEnd - DateTime.Now).Seconds;
                spriteBatch.Draw(blank, new Rectangle(15, 15, left * 10, 20), new Color(Color.LightBlue, 120));
            }

            spriteBatch.Draw(blank, new Rectangle(Functions.GameSize.X - 220, 10, 210, 30), new Color(Color.Black, 100));
            spriteBatch.Draw(blank, new Rectangle(Functions.GameSize.X - 215, 15, piranha.Health * 2, 20), new Color((piranha.Poisoned) ? Color.GreenYellow : Color.Red, 150));

            spriteBatch.DrawString(spriteFont,
                                   Functions.Alert.Message,
                                   new Vector2((Functions.GameSize.X / 2) - (spriteFont.MeasureString(Functions.Alert.Message).X / 2), 15),
                                   Functions.Alert.Color);
            #endregion

            spriteBatch.End();

            // If the game is transitioning on or off, fade it out to black.
            if (TransitionPosition > 0)
                ScreenManager.FadeBackBufferToBlack(255 - TransitionAlpha);
        }

        #endregion

        #region Private methods
        /// <summary>
        /// Reset everything and prepare the new game/level
        /// </summary>
        private void ResetGame()
        {
            // Reset random objects
            piranha.Reset();
            activeFish.Clear();
            blood.Clear();
            powerUp.EndEffect();

            #region Reset fish
            int goodToDraw = numFish[currentLevel - 1];
            foreach (Fish f in fish)
            {
                f.Position = Functions.RandScreenPos(new Rectangle(50, 50, Functions.GameSize.X - 50, Functions.GameSize.Y - 50));
                f.CureFish = false;
                f.Rotation = (float)Functions.RandDouble(-1, 1);
                f.IsActive = !(goodToDraw < 1);
                if (f.IsActive) activeFish.Add(f);
                goodToDraw--;
            }
            #endregion

            #region Reset mines
            int badToDraw = numMines[currentLevel - 1];
            foreach (Mine mine in mines)
            {
                mine.Position = Functions.RandScreenPos();
                mine.IsActive = !(badToDraw < 1);
                mine.PoisonTipped = false;
                if (mine.IsActive)
                    if (rand.Next(0, 5) == 4) mine.PoisonTipped = true;
                badToDraw--;
            }
            #endregion
        }

        /// <summary>
        /// Checks for any collisions with fish, mines, or power-ups.
        /// The method instantly returns if the player is still invul
        /// </summary>
        private void CheckCollisions()
        {
            // No need to check the collisions if the player is invul
            if (piranha.IsInvul)
                return;

            // If the piranha "eats" a power-up, its ApplyEffect method is used
            if (powerUp.IsActive && powerUp.Rectangle.Intersects(piranha.Rectangle))
            {
                powerUp.ApplyEffect(piranha);
                // Play the power-up sound
                powerUpSound.Play(0.2f, 0, 0);
            }

            foreach (Fish f in fish)
            {
                // Break straight out of the current iteration if the object isn't active
                if (!f.IsActive) continue;

                if (f.Rectangle.Intersects(piranha.Rectangle))
                {
                    f.Destroy();
                    biteSound.Play(0.1f, 0, 0);
                    blood.Add(new Blood(bloodTex, f.Position));
                    activeFish.Remove(f);

                    if (f.CureFish && piranha.Poisoned)
                        piranha.Poisoned = false;
                }
            }

            if (!godMode)
            {
                foreach (Mine mine in mines)
                {
                    // Break straight out of the current iteration if the object isn't active
                    if (!mine.IsActive) continue;

                    // As the mines are more-or-less circular a simple distance calculation
                    // will be able to determine if there is a collision.
                    if (Vector2.Distance(mine.Position, piranha.Position) < 40f)
                    {
                        if (mine.PoisonTipped && !piranha.Poisoned)
                        {
                            piranha.Poisoned = true;
                            Functions.Alert.Show("You have been poisoned!", Color.Orange);
                            blood.Add(new Blood(bloodTex, mine.Position, Color.GreenYellow));
                            activeFish[0].CureFish = true;
                        }

                        piranha.DoDamage(rand.Next(10, 15), mine);
                        hurtSound.Play(0.15f, 0, 0);
                        mine.Destroy();
                    }
                }
            }

            if ((activeFish.Count - 1) < 1)
                LevelComplete();
        }

        /// <summary>
        /// Called whenever a level is complete. If they are on the final level, they win the game.
        /// </summary>
        private void LevelComplete()
        {
            if (currentLevel >= maxLevel)
                WonTheGame();
            else
            {
                currentLevel++;
                ResetGame();
                DisplayLevelMessage();
            }
        }

        /// <summary>
        /// Ends the current game and returns to the main menu.
        /// </summary>
        public void GameOver()
        {
            isGameOver = true;
            LoadingScreen.Load(ScreenManager, false, null, new BackgroundScreen(), new MainMenuScreen());
            ScreenManager.AddScreen(new GameOverScreen(), ControllingPlayer);
        }

        /// <summary>
        /// User has won the game, show a "You won!" message and return to the main menu
        /// </summary>
        private void WonTheGame()
        {
            LoadingScreen.Load(ScreenManager, false, null, new BackgroundScreen(), new MainMenuScreen());
            ScreenManager.AddScreen(new WonGameScreen(), ControllingPlayer);
        }

        /// <summary>
        /// Shows a message box with the current level. Used when level is increased and decreased
        /// </summary>
        private void DisplayLevelMessage()
        {
            ScreenManager.AddScreen(new MessageBoxScreen("Level " + currentLevel + "\r\nPress enter to continue", false), ControllingPlayer);
        }
        #endregion
    }
}
