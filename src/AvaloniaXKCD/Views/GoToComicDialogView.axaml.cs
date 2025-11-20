using Avalonia.VisualTree;

namespace AvaloniaXKCD.Views;

public partial class GoToComicDialogView : UserControl
{
    public GoToComicDialogView()
    {
        InitializeComponent();

        this.gotoComicNumber.AttachedToVisualTree += async (s, e) =>
        {
            await Task.Delay(100);
            await Dispatcher.UIThread.InvokeAsync(
                () =>
                {
                    var descendants = gotoComicNumber.GetVisualDescendants();
                    var textbox = descendants.SingleOrDefault(d => d.Name == "PART_TextBox");
                    if (textbox is TextBox ctrl)
                    {
                        ctrl.Focus();
                        ctrl.SelectAll();
                    }
                },
                DispatcherPriority.ContextIdle
            );
        };
    }

    private async void NumericUpDown_KeyUp(object? sender, Avalonia.Input.KeyEventArgs e)
    {
        if (DataContext is XKCDViewModel viewModel)
        {
            switch (e.Key)
            {
                case Avalonia.Input.Key.Enter:
                    int value = (int?)((NumericUpDown?)sender)?.Value ?? 0;
                    await viewModel.GoToCommand.ExecuteAsync(value);
                    break;
                case Avalonia.Input.Key.Escape:
                    viewModel.DialogOpen = false;
                    break;
            }
        }
    }
}
