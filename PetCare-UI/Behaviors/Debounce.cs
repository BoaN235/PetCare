namespace PetCare.UI.Behaviors;


public partial class DebounceBehavior : Behavior<Entry>
{
    private Entry _entry;
    private CancellationTokenSource _cts;

    public static readonly BindableProperty DelayProperty =
        BindableProperty.Create(nameof(Delay), typeof(int), typeof(DebounceBehavior), 500);

    public int Delay
    {
        get => (int)GetValue(DelayProperty);
        set => SetValue(DelayProperty, value);
    }

    public static readonly BindableProperty CommandProperty =
        BindableProperty.Create(nameof(Command), typeof(Command<string>), typeof(DebounceBehavior));

    public Command<string> Command
    {
        get => (Command<string>)GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    protected override void OnAttachedTo(Entry bindable)
    {
        _entry = bindable;
        _entry.TextChanged += OnTextChanged;
        base.OnAttachedTo(bindable);
    }

    protected override void OnDetachingFrom(Entry bindable)
    {
        _entry.TextChanged -= OnTextChanged;
        _entry = null;
        base.OnDetachingFrom(bindable);
    }

    private async void OnTextChanged(object sender, TextChangedEventArgs e)
    {
        _cts?.Cancel();
        _cts = new CancellationTokenSource();
        var token = _cts.Token;

        try
        {
            await Task.Delay(Delay, token);
            if (!token.IsCancellationRequested)
            {
                Command?.Execute(e.NewTextValue);
            }
        }
        catch (TaskCanceledException) { }
    }
}
