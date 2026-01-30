using R3;

public interface IButton : IActable
{
    public ReadOnlyReactiveProperty<bool> IsPressed {get;}
}