namespace PetCare.UI.Behaviors;

public class DebounceClickHandler
{
    private bool _isWaiting = false;
    private readonly int _delayMilliseconds;

    public DebounceClickHandler(int delayMilliseconds = 1000)
    {
        _delayMilliseconds = delayMilliseconds;
    }

    public async Task Handle(Func<Task> action)
    {
        if (_isWaiting) return;

        _isWaiting = true;
        await action();
        await Task.Delay(_delayMilliseconds);
        _isWaiting = false;
    }
}
