namespace AvaloniaXKCD.ViewModels;

public abstract class ViewModelDialogBase : ViewModelBase, IViewModelDialogBase
{
    public virtual bool CanClose => true;
}

public abstract class ViewModelDialogBase<T, T2, T3> : ViewModelDialogBase,
    IViewModelDialogBase<T, T2, T3>
    where T : IViewModelDialogBase<T, T2, T3>
    where T2 : Enum
    where T3 : ViewModelDialogBase<T, T2, T3>.ViewModelDialogBaseEventArgs
{
    public abstract class ViewModelDialogBaseEventArgs : ViewModelDialogBaseEventArgs<T, T2, T3>
    {
        protected ViewModelDialogBaseEventArgs(T2 type, T self) : base(type, self) { }
    }

    public virtual event EventHandler<T3>? OnSubmit;

    public virtual void Submit(T3 e)
    {
        OnSubmit?.Invoke(this, e);
    }
}