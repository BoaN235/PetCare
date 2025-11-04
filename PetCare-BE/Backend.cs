namespace PetCare_BE
{
    using Microsoft.Maui.Controls;
    using PetCare_AI;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Xml.Linq;
    public class Backend
    {
        private Pet pet = new Pet("Gizmo", "Cat");
        int count = 0;
        private AiModel ai = new AiModel();
        public PetGameState gamestate;
        public PlayerAction[] actions = new PlayerAction[]

        {
            new PlayerAction("Feed the pet", 0 ,1,1,1,1),
            new PlayerAction("Play with the pet", 1 ,1,1,1,1),
            new PlayerAction("Take the pet for a walk", 2 ,1,1,1,1),
            new PlayerAction("Put the pet to sleep", 3 ,1,1,1,1)
        };

        public Backend()
        {
            gamestate = new PetGameState();
            ai.InitModel();
            // RefreshChat();
        }

        //private async void RunAi(object sender, EventArgs e)
        //{

        //    string input = "hi";

        //    // Run AI processing in background
        //    await Task.Run(async () =>
        //    {
        //        //await ai.RunModel(input);
        //    });

        //    // Update UI on main thread after model finishes
        //    MainThread.BeginInvokeOnMainThread(() =>
        //    {
        //        RefreshChat();
        //    });
        //}

        //private void RefreshChat()
        //{
        //    List<string> Text = new List<string> { "Apple", "Banana" };
        //    foreach (var m in ai.chatHistory.Messages)
        //    {
        //        Text.Add($"{m.AuthorRole}: {m.Content}&#x0a;");
        //    }
        //}
    }
}
