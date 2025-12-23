using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Veriflow.UI.Views;

public partial class OffloadCompletionDialog : Window
{
    public OffloadCompletionDialog()
    {
        InitializeComponent();
    }
    
    public void SetSuccess(string title, string subtitle, string details)
    {
        TitleText.Text = $"✓ {title}";
        TitleText.Foreground = this.FindResource("Brush.Accent.Success") as Avalonia.Media.IBrush;
        SubtitleText.Text = subtitle;
        DetailsText.Text = details;
    }
    
    public void SetError(string title, string subtitle, string details)
    {
        TitleText.Text = $"✗ {title}";
        TitleText.Foreground = this.FindResource("Brush.Accent.Error") as Avalonia.Media.IBrush;
        SubtitleText.Text = subtitle;
        DetailsText.Text = details;
    }
    
    private void OnOkClick(object? sender, RoutedEventArgs e)
    {
        Close();
    }
}
