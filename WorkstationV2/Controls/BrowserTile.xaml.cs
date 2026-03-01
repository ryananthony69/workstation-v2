using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Web.WebView2.Core;

namespace WorkstationV2.Controls;

public partial class BrowserTile : UserControl
{
    public int TileId { get; set; } = 1;

    private Action<string>? _onUrlChanged;
    private Action? _onFocused;

    public BrowserTile()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    public void Initialize(string initialUrl, Action<string> onUrlChanged, Action onFocused)
    {
        _onUrlChanged = onUrlChanged;
        _onFocused = onFocused;

        UrlBox.Text = initialUrl ?? string.Empty;
        if (!string.IsNullOrWhiteSpace(UrlBox.Text))
        {
            Navigate(UrlBox.Text);
        }
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        try
        {
            if (Web.CoreWebView2 == null)
            {
                await Web.EnsureCoreWebView2Async();
                HookEvents();
            }
        }
        catch
        {
            // Keep UI alive even if WebView2 fails to init.
        }
    }

    private void HookEvents()
    {
        try
        {
            Web.CoreWebView2.NavigationCompleted += (_, _) =>
            {
                try
                {
                    var uri = Web.Source?.ToString() ?? string.Empty;
                    if (!string.IsNullOrWhiteSpace(uri))
                    {
                        UrlBox.Text = uri;
                        _onUrlChanged?.Invoke(uri);
                    }
                }
                catch { }
            };

            Web.GotFocus += (_, _) => _onFocused?.Invoke();
            Web.PreviewMouseDown += (_, _) => _onFocused?.Invoke();
        }
        catch { }
    }

    public void Navigate(string url)
    {
        url = (url ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(url)) return;

        if (!url.Contains("://"))
        {
            url = "https://" + url;
        }

        try
        {
            UrlBox.Text = url;
            Web.Source = new Uri(url);
            _onUrlChanged?.Invoke(url);
            _onFocused?.Invoke();
        }
        catch { }
    }

    public void FocusWeb()
    {
        try
        {
            Web.Focus();
            _onFocused?.Invoke();
        }
        catch { }
    }

    private void Go_Click(object sender, RoutedEventArgs e) => Navigate(UrlBox.Text);

    private void UrlBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            Navigate(UrlBox.Text);
            e.Handled = true;
        }
    }
}