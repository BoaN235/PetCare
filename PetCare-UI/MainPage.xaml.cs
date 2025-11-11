using Microsoft.Maui.Controls.Shapes;
using PetCare.BE;
using PetCare.Core;
using PetCare.UI.Behaviors;
using System.Data;
using System.Timers;
using CommunityToolkit.Maui.Views;
using CommunityToolkit.Maui.Extensions;
using CommunityToolkit.Maui;

namespace PetCare.UI;

public partial class MainPage : ContentPage
{
    private IBackend _backend { get; }
    private readonly DebounceClickHandler _debouncer = new DebounceClickHandler(1000);
    private System.Timers.Timer _timer;
    private PlayerAction _selectedAction;
    public MainPage(IBackend backend)
    {
        _backend = backend;

        InitializeComponent();


        _timer = new System.Timers.Timer(500); // 0.5 second
        _timer.Elapsed += OnTimerElapsed;
        _timer.AutoReset = true;
        _timer.Start();

        UpdateScreen();
    }

    private void OnTimerElapsed(object sender, ElapsedEventArgs e)
    {
        // Update UI on the main thread
        MainThread.BeginInvokeOnMainThread(() =>
        {
            RefreshLog();
        });
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
            var btn = new Button
            {
                Text = action.Text
            };
            btn.Clicked += async (s, e) =>
            {
                await _debouncer.Handle(async () =>
                {
                    var act = _backend.Actions.SingleOrDefault(a => a.Text == action.Text);
                    DisplayPopupButtonClicked(act);
                    UpdateScreen();
                });
            };
            ActionBox.Children.Add(btn);
        }
    }

    public void RefreshLog()
    {
        log.Text = _backend.GameLog.GetLogText();
    }

    public void Update_stats()
    {
        HealthBar.Progress = _backend.GameState.Pet.Health/100.0;
        HappyBar.Progress = _backend.GameState.Pet.Happiness / 100.0;
        HungerBar.Progress = _backend.GameState.Pet.Hunger / 100.0;
        MoneyLabel.Text = $"${_backend.GameState.Pet.Money}";
    }

    public void UpdateScreen()
    {             
        weekLabel.Text = $"Day: \n {_backend.GameState.Week}";
        LoadActions(_backend.Actions);
        Update_stats();
        RefreshLog();
    }

    private void OnNextWeekClicked(object? sender, EventArgs e)
    {
        if (_selectedAction != null) 
        {
            _backend.PerformAction(_selectedAction);
        }
        _backend.GameState.Week += 1;
        UpdateScreen();
    }

    private async void OnNewGameClicked(object? sender, EventArgs e)
    {
        await _debouncer.Handle(async () =>
        {
            _backend.GameState.Pet = new Pet();
            DisplayNewGamePopupButton();
            await _backend.NewGame();
            UpdateScreen();
        });
    }

    private async void OnAiChatClicked(object? sender, EventArgs e)
    {
        await _debouncer.Handle(async () =>
        {
            if (ChatInput.Text == null || ChatInput.Text.Trim() == string.Empty)
                return;
            await _backend.AiModel.RunModel(ChatIdEnum.UserChat, ChatInput.Text);
            ChatInput.Text = string.Empty;
        });
        UpdateScreen();
    }

    async void DisplayPopupButtonClicked(PlayerAction action)
    {
        _selectedAction = action;

        var content = new Border
        {
            Background = Colors.White,
            Stroke = Colors.Gray,
            StrokeThickness = 2,
            StrokeShape = new RoundRectangle
            {
                CornerRadius = new CornerRadius(20)
            },
            Padding = 30,
            Content = new Label
            {
                Text = $"Health: {_selectedAction.HealthChange}\nMood: {_selectedAction.HappinessChange}\nHunger: {_selectedAction.HungerChange}\nMoney: {_selectedAction.MoneyChange}",
                TextColor = Colors.Black
            }
        };

        await this.ShowPopupAsync(content, new PopupOptions
        {
            CanBeDismissedByTappingOutsideOfPopup = true,
            PageOverlayColor = Colors.Black.MultiplyAlpha(0.4f)
        });
    }


    async void DisplayNewGamePopupButton()
    {

        var submitButton = new Button
        {
            Text = "Submit",
            TextColor = Colors.White,
            BackgroundColor = Colors.MidnightBlue
        };
        // Create Entry fields to capture user input
        var nameEntry = new Entry
        {
            Placeholder = "Pet Name",
            TextColor = Colors.White
        };

        var speciesEntry = new Entry
        {
            Placeholder = "Pet Species",
            TextColor = Colors.White
        };

        // Handle submit button click
        submitButton.Clicked += async (s, e) =>
        {
            // Gather input data from Entry fields
            string petName = nameEntry.Text?.Trim();
            string petSpecies = speciesEntry.Text?.Trim();

            // Validate and assign values
            if (!string.IsNullOrEmpty(petName) && !string.IsNullOrEmpty(petSpecies))
            {
                _backend.GameState.Pet.Name = petName;
                _backend.GameState.Pet.Species = petSpecies;
            }

            await this.ClosePopupAsync();
        };

        // Create the content layout
        var content = new Border
        {
            Background = Colors.DarkBlue,
            Stroke = Colors.Gray,
            StrokeThickness = 2,
            StrokeShape = new RoundRectangle
            {
                CornerRadius = new CornerRadius(20)
            },
            Padding = 30,
            Content = new StackLayout
            {
                Children =
        {
            new Label
            {
                Text = "Enter Pet Name",
                TextColor = Colors.White
            },
            nameEntry,
            new Label
            {
                Text = "Enter Pet Species",
                TextColor = Colors.White
            },
            speciesEntry,
            submitButton
        }
            }
        };


        await this.ShowPopupAsync(content, new PopupOptions
        {
            CanBeDismissedByTappingOutsideOfPopup = false,
            PageOverlayColor = Colors.Black.MultiplyAlpha(0.4f)
        });
    }


    private async void OnSaveClicked(object? sender, EventArgs e)
    {
        await _debouncer.Handle(async () =>
        {
            await _backend.SaveGame();
            UpdateScreen();
        });

    }

    private async void OnLoadClicked(object? sender, EventArgs e)
    {
        await _debouncer.Handle(async () =>
        {
            await _backend.LoadGame();
            UpdateScreen();
        });
    }
}
