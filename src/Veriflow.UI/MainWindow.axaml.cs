using System;
using System.Threading;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using Veriflow.Core.Models;
using Veriflow.UI.Services;
using Veriflow.UI.ViewModels;

namespace Veriflow.UI;

public partial class MainWindow : Window
{
    private readonly IWindowSettingsService _windowSettingsService;
    private Timer? _saveTimer;
    private const int SaveDebounceMs = 500;

    public MainWindow(IWindowSettingsService windowSettingsService)
    {
        _windowSettingsService = windowSettingsService;
        
        InitializeComponent();
        
        // Load and apply saved window settings
        LoadWindowSettings();
        
        // Subscribe to window events to save settings
        PositionChanged += OnWindowStateChanged;
        PropertyChanged += OnWindowPropertyChanged;
    }

    private void LoadWindowSettings()
    {
        var settings = _windowSettingsService.LoadSettings();
        
        // Apply settings
        Width = settings.Width;
        Height = settings.Height;
        Position = new Avalonia.PixelPoint((int)settings.X, (int)settings.Y);
        
        // Apply window state (maximized or normal)
        WindowState = settings.IsMaximized ? WindowState.Maximized : WindowState.Normal;
    }

    private void OnWindowStateChanged(object? sender, EventArgs e)
    {
        DebounceSaveSettings();
    }

    private void OnWindowPropertyChanged(object? sender, Avalonia.AvaloniaPropertyChangedEventArgs e)
    {
        // Save when window state, size, or position changes
        if (e.Property.Name == nameof(WindowState) || 
            e.Property.Name == nameof(Width) || 
            e.Property.Name == nameof(Height) ||
            e.Property.Name == nameof(Position))
        {
            DebounceSaveSettings();
        }
    }

    private void DebounceSaveSettings()
    {
        // Cancel existing timer
        _saveTimer?.Dispose();
        
        // Create new timer to save after debounce period
        _saveTimer = new Timer(_ =>
        {
            Dispatcher.UIThread.Post(() =>
            {
                SaveWindowSettings();
            });
        }, null, SaveDebounceMs, Timeout.Infinite);
    }

    private void SaveWindowSettings()
    {
        var settings = new WindowSettings
        {
            Width = Width,
            Height = Height,
            X = Position.X,
            Y = Position.Y,
            IsMaximized = WindowState == WindowState.Maximized
        };
        
        _windowSettingsService.SaveSettings(settings);
    }
    
    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        
        // Ctrl+Tab: Toggle profile (v1 feature)
        if (e.Key == Key.Tab && e.KeyModifiers == KeyModifiers.Control)
        {
            if (DataContext is MainWindowViewModel vm)
            {
                vm.ToggleProfileCommand.Execute(null);
                e.Handled = true;
            }
        }
    }

    protected override void OnClosing(WindowClosingEventArgs e)
    {
        base.OnClosing(e);
        
        // Save settings one final time when closing
        SaveWindowSettings();
        
        // Dispose timer
        _saveTimer?.Dispose();
    }
}