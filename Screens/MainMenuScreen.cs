#region File Description
//-----------------------------------------------------------------------------
// MainMenuScreen.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using Microsoft.Xna.Framework;
#endregion

namespace ICGGSAssignment
{
    /// <summary>
    /// The main menu screen is the first thing displayed when the game starts up.
    /// </summary>
    class MainMenuScreen : MenuScreen
    {
        #region Initialization


        /// <summary>
        /// Constructor fills in the menu contents.
        /// </summary>
        public MainMenuScreen()
            : base("Main Menu")
        {
            // Create our menu entries.
            MenuEntry playGameMenuEntry = new MenuEntry("Play Game");
            MenuEntry playMovieMenuEntry = new MenuEntry("Play Movie");
            MenuEntry instructionsMenuEntry = new MenuEntry("Instructions");
            MenuEntry exitMenuEntry = new MenuEntry("Exit");

            // Hook up menu event handlers.
            playGameMenuEntry.Selected += PlayGameMenuEntrySelected;
            playMovieMenuEntry.Selected += PlayMovieMenuEntrySelected;
            instructionsMenuEntry.Selected += InstructionsMenuEntrySelected;
            exitMenuEntry.Selected += OnCancel;

            // Add entries to the menu.
            MenuEntries.Add(playGameMenuEntry);
            MenuEntries.Add(playMovieMenuEntry);
            MenuEntries.Add(instructionsMenuEntry);
            MenuEntries.Add(exitMenuEntry);
        }


        #endregion

        #region Handle Input


        /// <summary>
        /// Event handler for when the Play Game menu entry is selected.
        /// </summary>
        void PlayGameMenuEntrySelected(object sender, PlayerIndexEventArgs e)
        {
            LoadingScreen.Load(ScreenManager, true, e.PlayerIndex,
                               new GameplayScreen());
        }

        /// <summary>
        /// Event handler for when the Play Movie menu entry is selected.
        /// </summary>
        void PlayMovieMenuEntrySelected(object sender, PlayerIndexEventArgs e)
        {
            LoadingScreen.Load(ScreenManager, true, e.PlayerIndex,
                               new MovieScreen());
        }


        /// <summary>
        /// Event handler for when the Instructions menu entry is selected.
        /// </summary>
        void InstructionsMenuEntrySelected(object sender, PlayerIndexEventArgs e)
        {
            LoadingScreen.Load(ScreenManager, false, e.PlayerIndex, new InstructionsScreen());
        }


        /// <summary>
        /// When the user cancels the main menu, ask if they want to exit the sample.
        /// </summary>
        protected override void OnCancel(PlayerIndex playerIndex)
        {
            const string message = "Are you sure you want to exit?";

            MessageBoxScreen confirmExitMessageBox = new MessageBoxScreen(message);

            confirmExitMessageBox.Accepted += ConfirmExitMessageBoxAccepted;

            ScreenManager.AddScreen(confirmExitMessageBox, playerIndex);
        }


        /// <summary>
        /// Event handler for when the user selects ok on the "are you sure
        /// you want to exit" message box.
        /// </summary>
        void ConfirmExitMessageBoxAccepted(object sender, PlayerIndexEventArgs e)
        {
            ScreenManager.Game.Exit();
        }


        #endregion
    }
}
