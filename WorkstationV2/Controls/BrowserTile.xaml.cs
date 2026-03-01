using Microsoft.Web.WebView2.Core;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace WorkstationV2.Controls;

public partial class BrowserTile : UserControl
{
    public int TileId { get; set; }

    private Action<string>? _onUrlChanged;
    private Action? _onFocused;
    private bool _isInitialized;

    public BrowserTile()
    {
        InitializeComponent();
        Loaded += OnLoaded;
        GotFocus += (_, _) => _onFocused?.Invoke();
    }

    public void Initialize(string initialUrl, Action<string> onUrlChanged, Action onFocused)
    {
        _onUrlChanged = onUrlChanged;
        _onFocused = onFocused;
        UrlBox.Text = initialUrl ?? string.Empty;
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (_isInitialized) return;
        _isInitialized = true;

        try
        {
            await Web.EnsureCoreWebView2Async();
            Web.CoreWebView2.NavigationCompleted += CoreWebView2_NavigationCompleted;

            var url = (UrlBox.Text ?? string.Empty).Trim();
            if (!string.IsNullOrWhiteSpace(url))
            {
                Navigate(url);
            }
        }
        catch
        {
        }
    }

    private void CoreWebView2_NavigationCompleted(object? sender, CoreWebView2NavigationCompletedEventArgs e)
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
    }

    public void Navigate(string url)
    {
        try
        {
            url = (url ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(url)) return;

            if (!url.Contains("://", StringComparison.OrdinalIgnoreCase))
            {
                url = "https://" + url;
            }

            UrlBox.Text = url;

            if (Web.CoreWebView2 != null)
            {
                Web.CoreWebView2.Navigate(url);
            }
            else
            {
                Web.Source = new Uri(url);
            }

            _onUrlChanged?.Invoke(url);
        }
        catch { }
    }

    public void FocusWeb()
    {
        try
        {
            Web.Focus();
            Web.CoreWebView2?.Focus();
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