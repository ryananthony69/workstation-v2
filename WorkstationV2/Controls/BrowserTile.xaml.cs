using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace WorkstationV2.Controls;

public partial class BrowserTile : UserControl
{
    public int TileId { get; set; } = 1;

    private Action<string>? _onUrlChanged;
    private Action? _onFocused;

    private Microsoft.Web.WebView2.Wpf.WebView2? _web;

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
            if (_web == null)
            {
                _web = new Microsoft.Web.WebView2.Wpf.WebView2();
                _web.GotFocus += (_, _) => _onFocused?.Invoke();
                _web.PreviewMouseDown += (_, _) => _onFocused?.Invoke();

                WebHost.Children.Clear();
                WebHost.Children.Add(_web);
            }

            try
            {
                await _web.EnsureCoreWebView2Async();
            }
            catch (Exception ex)
            {
                ShowFallback("WebView2 runtime unavailable or failed to initialize.\n\n" + ex.Message);
                return;
            }

            try
            {
                _web.CoreWebView2.NavigationCompleted += (_, _) =>
                {
                    try
                    {
                        var uri = _web.Source?.ToString() ?? string.Empty;
                        if (!string.IsNullOrWhiteSpace(uri))
                        {
                            UrlBox.Text = uri;
                            _onUrlChanged?.Invoke(uri);
                        }
                    }
                    catch { }
                };
            }
            catch { }
        }
        catch (Exception ex)
        {
            ShowFallback("Browser tile failed to initialize.\n\n" + ex.Message);
        }
    }

    private void ShowFallback(string message)
    {
        try
        {
            WebFallbackText.Text = message ?? "Browser unavailable.";
            WebFallback.Visibility = Visibility.Visible;
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
            _onUrlChanged?.Invoke(url);
            _onFocused?.Invoke();

            if (_web != null)
            {
                _web.Source = new Uri(url);
            }
        }
        catch { }
    }

    public void FocusWeb()
    {
        try
        {
            _web?.Focus();
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