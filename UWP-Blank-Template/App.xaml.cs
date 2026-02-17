using Microsoft.UI.Xaml;
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
            // ❌ 不要在这里加载设置！
            // AppThemeManager.LoadSettings();
        }

        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            Frame rootFrame = Window.Current.Content as Frame;
            if (rootFrame == null)
            {
                rootFrame = new Frame();
                Window.Current.Content = rootFrame;

                // ✅ 在窗口创建后再加载设置
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

    // 将主题与材质管理器放在 App 后端，供全局调用
    public static class AppThemeManager
    {
        public static ElementTheme CurrentTheme = ElementTheme.Default;
        public static BackgroundMaterial CurrentMaterial = BackgroundMaterial.Mica;

        public static void LoadSettings()
        {
            var localSettings = ApplicationData.Current.LocalSettings;

            // 主题 - 改用 TryGetValue 模式
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
            catch
            {
                CurrentTheme = ElementTheme.Default;
            }

            // 材质
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
            catch
            {
                CurrentMaterial = BackgroundMaterial.Mica;
            }

            // 声音 - bool 类型特别容易出问题
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
            catch
            {
                ElementSoundPlayer.State = ElementSoundPlayerState.On;
            }
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

                // UWP 官方 Mica API（WinUI 2.6+）
                if (CurrentMaterial == BackgroundMaterial.Mica)
                {
                    // 禁用 Acrylic 的主题监听
                    if (rootFrame is FrameworkElement element)
                    {
                        element.ActualThemeChanged -= RootElement_ActualThemeChanged;
                    }

                    // 清空背景
                    rootFrame.Background = null;

                    // 正确启用 Mica
                    Microsoft.UI.Xaml.Controls.BackdropMaterial.SetApplyToRootOrPageBackground(rootFrame, true);

                    Debug.WriteLine("Mica applied (UWP WinUI 2.x)");
                }
                else // Acrylic
                {
                    // 禁用 Mica
                    Microsoft.UI.Xaml.Controls.BackdropMaterial.SetApplyToRootOrPageBackground(rootFrame, false);

                    rootFrame.Background = null;

                    var isDark = GetIsDarkTheme();
                    var tintColor = isDark
                        ? Color.FromArgb(255, 32, 32, 32)
                        : Color.FromArgb(255, 243, 243, 243);

                    var acrylicBrush = new AcrylicBrush
                    {
                        BackgroundSource = AcrylicBackgroundSource.HostBackdrop,
                        TintColor = tintColor,
                        TintOpacity = 0.8,
                        FallbackColor = tintColor
                    };

                    rootFrame.Background = acrylicBrush;

                    Debug.WriteLine($"Acrylic applied (Theme: {(isDark ? "Dark" : "Light")})");

                    // Acrylic 监听主题变化
                    if (rootFrame is FrameworkElement element)
                    {
                        element.ActualThemeChanged -= RootElement_ActualThemeChanged;
                        element.ActualThemeChanged += RootElement_ActualThemeChanged;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ApplyMaterial failed: {ex.Message}");

                // 降级为纯色背景
                if (Window.Current.Content is Frame frame)
                {
                    var isDark = GetIsDarkTheme();
                    var fallbackColor = isDark
                        ? Color.FromArgb(255, 32, 32, 32)
                        : Color.FromArgb(255, 243, 243, 243);
                    frame.Background = new SolidColorBrush(fallbackColor);
                }
            }
        }

        private static bool GetIsDarkTheme()
        {
            if (CurrentTheme == ElementTheme.Default)
                return Application.Current.RequestedTheme == ApplicationTheme.Dark;

            return CurrentTheme == ElementTheme.Dark;
        }

        private static void RootElement_ActualThemeChanged(FrameworkElement sender, object args)
        {
            if (CurrentMaterial == BackgroundMaterial.Acrylic)
            {
                Debug.WriteLine("Theme changed, refreshing acrylic color");
                ApplyMaterial();
            }
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
                var hoverBg = isDark ? Color.FromArgb(20, 255, 255, 255) : Color.FromArgb(20, 0, 0, 0);

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