using static AvaloniaXKCD.ViewModels.ErrorDialogViewModel;

namespace AvaloniaXKCD.ViewModels
{
    public partial class ErrorDialogViewModel
        : ViewModelDialogBase<ErrorDialogViewModel, ErrorDialogViewModelEventType, ErrorDialogViewModelEventArgs>
    {
        readonly MainViewModel Parent;

#if DEBUG
        public ErrorDialogViewModel(MainViewModel parent)
        {
            Parent = parent;

            Exception exception = new("Example Error");
            Exception = exception;
            ErrorText = exception.ToString();
        }
#endif

        public ErrorDialogViewModel(MainViewModel parent, Exception exception, bool canContinue = false)
        {
            Parent = parent;

            Exception = exception;
            ErrorText = exception.GetType().FullName + ": " + exception.Message;

            CanContinue = canContinue;
        }

        internal ErrorDialogViewModel(MainViewModel parent, ErrorDialogViewModel viewModel, bool canContinue = false)
        {
            Parent = parent;

            ErrorText = viewModel.ErrorText;

            CanContinue = canContinue;
        }

        [JsonIgnore]
        public Exception? Exception { get; private init; }
        public string? ErrorText { get; private init; }

        public void Quit() => Submit(new(ErrorDialogViewModelEventType.Quit, this));

        public void Continue() => Submit(new(ErrorDialogViewModelEventType.Continue, this));

        [ObservableProperty]
        public bool _canContinue = true;
        public override bool CanClose
        {
            get => CanContinue;
        }

        #region Dialog
        public enum ErrorDialogViewModelEventType
        {
            Quit,
            Continue,
        }

        public class ErrorDialogViewModelEventArgs : ViewModelDialogBaseEventArgs
        {
            public ErrorDialogViewModelEventArgs(ErrorDialogViewModelEventType type, ErrorDialogViewModel self)
                : base(type, self) { }
        }

        public override event EventHandler<ErrorDialogViewModelEventArgs>? OnSubmit;

        public override void Submit(ErrorDialogViewModelEventArgs e)
        {
            OnSubmit?.Invoke(this, e);
        }
        #endregion
    }
}
