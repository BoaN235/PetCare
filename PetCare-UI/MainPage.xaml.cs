using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using PetCare_BE;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace PetCare_UI
{
    public partial class MainPage : ContentPage
    {
        int count = 0;
        Backend BackendRef;
        PetGameState gamestate;

        public MainPage()
        {
            InitializeComponent();
            BackendRef = new Backend();
            gamestate = BackendRef.gamestate;

            UpdateScreen();
        }

        public void UpdateLog(string log_text)
        {
            log.Text = log_text;
        }

        private void DeleteActions()
        {
            ActionBox.Children.Clear();
        }

        public void LoadActions(PlayerAction[] actions)
        {
            DeleteActions();
            foreach (PlayerAction action in actions) 
            {
                ActionBox.Children.Add(new Label
                {
                    Text = action.text
                });
                ActionBox.Children.Add(new Button
                {
                    Text = "select"
                });
            }
        }

        public void UpdateScreen()
        {             
            weekLabel.Text = $"Week: \n {gamestate.week}";
            LoadActions(BackendRef.actions);
        }

        private void OnNextWeekClicked(object? sender, EventArgs e)
        {
            gamestate.week += 1;
            UpdateScreen();
        }

        private void OnNewGameClicked(object? sender, EventArgs e)
        {
            gamestate.NewGame();
        }

        private void OnSaveClicked(object? sender, EventArgs e)
        {
            gamestate.SaveGame();
        }

        private void OnLoadClicked(object? sender, EventArgs e)
        {
            gamestate.LoadGame();
        }

        private void OnExitClicked(object? sender, EventArgs e)
        {
            gamestate.ExitGame();

        }
    }
}
