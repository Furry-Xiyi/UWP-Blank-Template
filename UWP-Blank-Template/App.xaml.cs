using Microsoft.UI.Xaml.Controls;
using System;
using System.Diagnostics;
using Windows.ApplicationModel.Activation;
using Windows.Storage;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace UWP
{
    public partial class App : Application
    {
        public App()
        {
            this.InitializeComponent();
        }

        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            Frame rootFrame = Window.Current.Content as Frame;
            if (rootFrame == null)
            {
                rootFrame = new Frame();
                Window.Current.Content = rootFrame;

                AppThemeManager.LoadSettings();
                AppThemeManager.ApplyTheme();
                AppThemeManager.ApplyMaterial();
            }

            if (rootFrame.Content == null)
            {
                rootFrame.Navigate(typeof(MainPage));
            }

            Window.Current.Activate();
        }
    }

    public static class AppThemeManager
    {
        public static ElementTheme CurrentTheme = ElementTheme.Default;
        public static BackgroundMaterial CurrentMaterial = BackgroundMaterial.Mica;

        public static void LoadSettings()
        {
            var localSettings = ApplicationData.Current.LocalSettings;

            try
            {
                if (localSettings.Values.TryGetValue("AppTheme", out object themeObj) && themeObj != null)
                {
                    string theme = themeObj as string ?? themeObj.ToString();
                    CurrentTheme = theme == "Light" ? ElementTheme.Light :
                                   theme == "Dark" ? ElementTheme.Dark :
                                   ElementTheme.Default;
                }
                else
                {
                    CurrentTheme = ElementTheme.Default;
                    localSettings.Values["AppTheme"] = "System";
                }
            }
            catch { CurrentTheme = ElementTheme.Default; }

            try
            {
                if (localSettings.Values.TryGetValue("AppMaterial", out object materialObj) && materialObj != null)
                {
                    string material = materialObj as string ?? materialObj.ToString();
                    CurrentMaterial = material == "Acrylic" ? BackgroundMaterial.Acrylic : BackgroundMaterial.Mica;
                }
                else
                {
                    CurrentMaterial = BackgroundMaterial.Mica;
                    localSettings.Values["AppMaterial"] = "Mica";
                }
            }
            catch { CurrentMaterial = BackgroundMaterial.Mica; }

            try
            {
                if (localSettings.Values.TryGetValue("EnableSound", out object soundObj) && soundObj != null)
                {
                    bool soundEnabled = soundObj is bool b ? b : Convert.ToBoolean(soundObj);
                    ElementSoundPlayer.State = soundEnabled ? ElementSoundPlayerState.On : ElementSoundPlayerState.Off;
                }
                else
                {
                    localSettings.Values["EnableSound"] = true;
                    ElementSoundPlayer.State = ElementSoundPlayerState.On;
                }
            }
            catch { ElementSoundPlayer.State = ElementSoundPlayerState.On; }
        }

        public static void ApplyTheme()
        {
            try
            {
                if (Window.Current.Content is FrameworkElement rootElement)
                {
                    rootElement.RequestedTheme = CurrentTheme;
                }
                CustomizeTitleBar();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ApplyTheme failed: {ex.Message}");
            }
        }

        public static void ApplyMaterial()
        {
            try
            {
                var rootFrame = Window.Current.Content as Frame;
                if (rootFrame == null) return;

                if (CurrentMaterial == BackgroundMaterial.Mica)
                {
                    if (rootFrame is FrameworkElement element)
                        element.ActualThemeChanged -= OnActualThemeChanged;

                    rootFrame.Background = null;
                    Microsoft.UI.Xaml.Controls.BackdropMaterial.SetApplyToRootOrPageBackground(rootFrame, true);

                    Debug.WriteLine("Mica applied");
                }
                else
                {
                    Microsoft.UI.Xaml.Controls.BackdropMaterial.SetApplyToRootOrPageBackground(rootFrame, false);

                    var isDark = GetIsDarkTheme();
                    var tintColor = isDark
                        ? Color.FromArgb(255, 32, 32, 32)
                        : Color.FromArgb(255, 243, 243, 243);

                    rootFrame.Background = new AcrylicBrush
                    {
                        BackgroundSource = AcrylicBackgroundSource.HostBackdrop,
                        TintColor = tintColor,
                        TintOpacity = 0.8,
                        FallbackColor = tintColor
                    };

                    Debug.WriteLine($"Acrylic applied (Theme: {(isDark ? "Dark" : "Light")})");

                    if (rootFrame is FrameworkElement el)
                    {
                        el.ActualThemeChanged -= OnActualThemeChanged;
                        el.ActualThemeChanged += OnActualThemeChanged;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ApplyMaterial failed: {ex.Message}");

                if (Window.Current.Content is Frame frame)
                {
                    var isDark = GetIsDarkTheme();
                    frame.Background = new SolidColorBrush(isDark
                        ? Color.FromArgb(255, 32, 32, 32)
                        : Color.FromArgb(255, 243, 243, 243));
                }
            }
        }

        public static void OnActualThemeChanged(FrameworkElement sender, object args)
        {
            // 刷新标题栏颜色（跟随系统主题）
            CustomizeTitleBar();

            // 如果当前是 Acrylic，同步刷新背景色
            if (CurrentMaterial == BackgroundMaterial.Acrylic)
            {
                var rootFrame = Window.Current.Content as Frame;
                if (rootFrame == null) return;

                var isDark = GetIsDarkTheme();
                var tintColor = isDark
                    ? Color.FromArgb(255, 32, 32, 32)
                    : Color.FromArgb(255, 243, 243, 243);

                if (rootFrame.Background is AcrylicBrush brush)
                {
                    brush.TintColor = tintColor;
                    brush.FallbackColor = tintColor;
                }

                Debug.WriteLine("Acrylic color refreshed on theme change");
            }
        }

        public static bool GetIsDarkTheme()
        {
            if (Window.Current?.Content is FrameworkElement rootElement)
            {
                var actual = rootElement.ActualTheme;
                if (actual != ElementTheme.Default)
                    return actual == ElementTheme.Dark;
            }
            // ActualTheme 不可用时（启动阶段）回退到 CurrentTheme + 系统主题
            if (CurrentTheme == ElementTheme.Default)
                return Application.Current.RequestedTheme == ApplicationTheme.Dark;
            return CurrentTheme == ElementTheme.Dark;
        }

        public static void CustomizeTitleBar()
        {
            try
            {
                var coreTitleBar = Windows.ApplicationModel.Core.CoreApplication.GetCurrentView().TitleBar;
                coreTitleBar.ExtendViewIntoTitleBar = true;

                var titleBar = ApplicationView.GetForCurrentView().TitleBar;
                titleBar.ButtonBackgroundColor = Colors.Transparent;
                titleBar.ButtonInactiveBackgroundColor = Colors.Transparent;

                var isDark = GetIsDarkTheme();
                var fg = isDark ? Colors.White : Colors.Black;
                var inactiveFg = Color.FromArgb(128, fg.R, fg.G, fg.B);
                var hoverBg = isDark
                    ? Color.FromArgb(20, 255, 255, 255)
                    : Color.FromArgb(20, 0, 0, 0);

                titleBar.ButtonForegroundColor = fg;
                titleBar.ButtonInactiveForegroundColor = inactiveFg;
                titleBar.ButtonHoverBackgroundColor = hoverBg;
                titleBar.ButtonHoverForegroundColor = fg;
                titleBar.ButtonPressedBackgroundColor = Color.FromArgb(30, hoverBg.R, hoverBg.G, hoverBg.B);
                titleBar.ButtonPressedForegroundColor = fg;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"CustomizeTitleBar failed: {ex.Message}");
            }
        }
    }

    public enum BackgroundMaterial
    {
        Mica,
        Acrylic
    }
}