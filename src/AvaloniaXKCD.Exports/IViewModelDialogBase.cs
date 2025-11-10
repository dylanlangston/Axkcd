namespace AvaloniaXKCD.Exports
{
    public interface IViewModelDialogBaseEventArgs { }

    public abstract class ViewModelDialogBaseEventArgs<T1, T2, T3> : EventArgs, IViewModelDialogBaseEventArgs
        where T1 : IViewModelDialogBase<T1, T2, T3>
        where T2 : Enum
        where T3 : ViewModelDialogBaseEventArgs<T1, T2, T3>
    {
        protected ViewModelDialogBaseEventArgs(T2 type, T1 self)
        {
            Type = type;
            Self = self;
        }

        protected T1 Self { get; }

        public T2 Type { get; }
    }

    public interface IViewModelDialogBase : IViewModelBase
    {
        public bool CanClose { get; }
    }

    public interface IViewModelDialogBase<T1, T2, T3> : IViewModelDialogBase, IDisposable
        where T1 : IViewModelDialogBase<T1, T2, T3>
        where T2 : Enum
        where T3 : ViewModelDialogBaseEventArgs<T1, T2, T3>
    {
        public event EventHandler<T3>? OnSubmit;

        public void Submit(T3 e);
    }
}
