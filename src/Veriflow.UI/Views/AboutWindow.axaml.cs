using System;
using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;

namespace Veriflow.UI.Views;

public partial class AboutWindow : Window
{
    public AboutWindow()
    {
        InitializeComponent();
        
        Width = 600;
        Height = 500;
        CanResize = false;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        
        Title = "About Veriflow Pro";
        
        BuildContent();
    }
    
    private void BuildContent()
    {
        var mainPanel = new StackPanel
        {
            Margin = new Avalonia.Thickness(30),
            Spacing = 20
        };
        
        // Logo and Title
        var titlePanel = new StackPanel
        {
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
            Spacing = 10
        };
        
        var appName = new TextBlock
        {
            Text = "VERIFLOW PRO",
            FontSize = 32,
            FontWeight = FontWeight.Bold,
            Foreground = Brushes.White,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center
        };
        
        var version = new TextBlock
        {
            Text = "Version 3.0.0 Beta",
            FontSize = 16,
            Foreground = Brushes.LightGray,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center
        };
        
        titlePanel.Children.Add(appName);
        titlePanel.Children.Add(version);
        
        // Separator
        var separator1 = new Border
        {
            Height = 1,
            Background = Brushes.Gray,
            Margin = new Avalonia.Thickness(0, 10)
        };
        
        // Credits
        var creditsPanel = new StackPanel
        {
            Spacing = 8
        };
        
        var developedBy = new TextBlock
        {
            Text = "Developed by:",
            FontSize = 14,
            FontWeight = FontWeight.Bold,
            Foreground = Brushes.White
        };
        
        var developer = new TextBlock
        {
            Text = "Hervé OBEJERO - Lead Developer & Architect",
            FontSize = 13,
            Foreground = Brushes.LightGray,
            Margin = new Avalonia.Thickness(20, 0, 0, 0)
        };
        
        var aiAssistant = new TextBlock
        {
            Text = "Antigravity AI - AI Development Assistant",
            FontSize = 13,
            Foreground = Brushes.LightGray,
            Margin = new Avalonia.Thickness(20, 0, 0, 0)
        };
        
        var copyright = new TextBlock
        {
            Text = "Copyright © 2025 Hervé OBEJERO. All rights reserved.",
            FontSize = 12,
            Foreground = Brushes.Gray,
            Margin = new Avalonia.Thickness(0, 10, 0, 0)
        };
        
        creditsPanel.Children.Add(developedBy);
        creditsPanel.Children.Add(developer);
        creditsPanel.Children.Add(aiAssistant);
        creditsPanel.Children.Add(copyright);
        
        // Separator
        var separator2 = new Border
        {
            Height = 1,
            Background = Brushes.Gray,
            Margin = new Avalonia.Thickness(0, 10)
        };
        
        // License Info
        var licensePanel = new StackPanel
        {
            Spacing = 8
        };
        
        var licenseTitle = new TextBlock
        {
            Text = "License:",
            FontSize = 14,
            FontWeight = FontWeight.Bold,
            Foreground = Brushes.White
        };
        
        var licenseText = new TextBlock
        {
            Text = "MIT License - Free for commercial use",
            FontSize = 13,
            Foreground = Brushes.LightGray,
            Margin = new Avalonia.Thickness(20, 0, 0, 0)
        };
        
        var thirdPartyTitle = new TextBlock
        {
            Text = "Third-Party Components:",
            FontSize = 13,
            FontWeight = FontWeight.Bold,
            Foreground = Brushes.White,
            Margin = new Avalonia.Thickness(0, 10, 0, 0)
        };
        
        var thirdPartyList = new TextBlock
        {
            Text = "• Avalonia UI (MIT)\n" +
                   "• NAudio (MIT)\n" +
                   "• PdfSharpCore + MigraDoc (MIT)\n" +
                   "• LibVLCSharp (LGPL v2.1)\n" +
                   "• FFmpeg (LGPL v2.1)\n" +
                   "• MathNet.Numerics (MIT)",
            FontSize = 12,
            Foreground = Brushes.LightGray,
            Margin = new Avalonia.Thickness(20, 5, 0, 0),
            LineHeight = 20
        };
        
        licensePanel.Children.Add(licenseTitle);
        licensePanel.Children.Add(licenseText);
        licensePanel.Children.Add(thirdPartyTitle);
        licensePanel.Children.Add(thirdPartyList);
        
        // Buttons
        var buttonPanel = new StackPanel
        {
            Orientation = Avalonia.Layout.Orientation.Horizontal,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
            Spacing = 15,
            Margin = new Avalonia.Thickness(0, 20, 0, 0)
        };
        
        var viewLicenseButton = new Button
        {
            Content = "View Full License",
            Padding = new Avalonia.Thickness(20, 8),
            Background = new SolidColorBrush(Color.Parse("#0078D4"))
        };
        viewLicenseButton.Click += ViewLicenseButton_Click;
        
        var closeButton = new Button
        {
            Content = "Close",
            Padding = new Avalonia.Thickness(20, 8),
            Background = new SolidColorBrush(Color.Parse("#444444"))
        };
        closeButton.Click += (s, e) => Close();
        
        buttonPanel.Children.Add(viewLicenseButton);
        buttonPanel.Children.Add(closeButton);
        
        // Add all to main panel
        mainPanel.Children.Add(titlePanel);
        mainPanel.Children.Add(separator1);
        mainPanel.Children.Add(creditsPanel);
        mainPanel.Children.Add(separator2);
        mainPanel.Children.Add(licensePanel);
        mainPanel.Children.Add(buttonPanel);
        
        Content = mainPanel;
    }
    
    private void ViewLicenseButton_Click(object? sender, RoutedEventArgs e)
    {
        try
        {
            var licensePath = System.IO.Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "THIRD_PARTY_LICENSES.md"
            );
            
            if (System.IO.File.Exists(licensePath))
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = licensePath,
                    UseShellExecute = true
                });
            }
        }
        catch
        {
            // Silently fail if file not found
        }
    }
}
