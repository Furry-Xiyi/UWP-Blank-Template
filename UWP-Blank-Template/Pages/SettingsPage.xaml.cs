using Microsoft.UI.Xaml.Controls;
using System;
using System.Diagnostics;
using UWP.Dialogs;
using Windows.ApplicationModel;
using Windows.Storage;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media.Imaging;

namespace UWP.Pages
{
    public sealed partial class SettingsPage : Page
    {
        private ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
        private bool _isInitializing = true; // 初始化标志

        public SettingsPage()
        {
            this.InitializeComponent();
            this.Loaded += SettingsPage_Loaded;
        }

        private void SettingsPage_Loaded(object sender, RoutedEventArgs e)
        {
            LoadUI();
            LoadAppInfo();
            _isInitializing = false;

            if (Window.Current.Content is FrameworkElement root)
            {
                root.ActualThemeChanged -= Root_ActualThemeChanged;
                root.ActualThemeChanged += Root_ActualThemeChanged;
            }
        }

        private void Root_ActualThemeChanged(FrameworkElement sender, object args)
        {
            CustomizeTitleBarDirect();
        }

        private void LoadUI()
        {
            // 主题
            string theme = localSettings.Values["AppTheme"] as string ?? "System";
            RbTheme.SelectedIndex = theme switch
            {
                "Light" => 1,
                "Dark" => 2,
                _ => 0 // "System"
            };
            Debug.WriteLine($"加载主题: {theme}, 索引: {RbTheme.SelectedIndex}");

            // 材质
            string material = localSettings.Values["AppMaterial"] as string ?? "Mica";
            RbMaterial.SelectedIndex = material == "Acrylic" ? 1 : 0;
            Debug.WriteLine($"加载材质: {material}, 索引: {RbMaterial.SelectedIndex}");

            // 导航栏位置
            string pos = localSettings.Values["PanePosition"] as string ?? "Left";
            PanePositionCombo.SelectedIndex = pos == "Top" ? 1 : 0;

            // 声音
            bool sound = true; // 默认开启
            if (localSettings.Values.ContainsKey("EnableSound"))
            {
                sound = (bool)localSettings.Values["EnableSound"];
            }
            else
            {
                // 第一次启动，设置默认值
                localSettings.Values["EnableSound"] = true;
            }
            SoundToggle.IsOn = sound;
        }

        public void LoadAppInfo()
        {
            try
            {
                TxtAppName.Text = Package.Current.DisplayName;

                var v = Package.Current.Id.Version;
                TxtVersion.Text = $"{v.Major}.{v.Minor}.{v.Build}.{v.Revision}";

                ImgAppIcon.Source = new BitmapImage(Package.Current.Logo);

                string publisher = Package.Current.PublisherDisplayName;
                int year = DateTime.Now.Year;
                TxtCopyright.Text =
                    $"©{year} {publisher}。保留所有权利。";
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"LoadAppInfo 错误: {ex.Message}");
            }
        }

        public static string GetAppDisplayName() => Package.Current.DisplayName;
        public static BitmapImage GetAppLogo() => new BitmapImage(Package.Current.Logo);


        private async void OpenExternalLink(object sender, RoutedEventArgs e)
        {
            if (sender is HyperlinkButton btn && btn.Tag is string url)
            {
                var dialog = new ExternalOpenDialog();
                var result = await dialog.ShowAsync();

                if (result == ContentDialogResult.Primary)
                {
                    await Launcher.LaunchUriAsync(new Uri(url));
                }
            }
        }

        private void RbTheme_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isInitializing) return;

            string value = RbTheme.SelectedIndex switch
            {
                1 => "Light",
                2 => "Dark",
                _ => "System"
            };

            localSettings.Values["AppTheme"] = value;

            var theme = RbTheme.SelectedIndex switch
            {
                1 => ElementTheme.Light,
                2 => ElementTheme.Dark,
                _ => ElementTheme.Default
            };

            if (Window.Current.Content is FrameworkElement rootElement)
            {
                rootElement.RequestedTheme = theme;
            }
            CustomizeTitleBarDirect();
        }

        private void RbMaterial_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isInitializing) return;

            string value = RbMaterial.SelectedIndex == 1 ? "Acrylic" : "Mica";

            localSettings.Values["AppMaterial"] = value;

            var rootFrame = Window.Current.Content as Frame;
            if (rootFrame == null) return;

            if (value == "Mica")
            {
                if (rootFrame is FrameworkElement el)
                    el.ActualThemeChanged -= RootElement_ActualThemeChanged;

                rootFrame.Background = null;
                Microsoft.UI.Xaml.Controls.BackdropMaterial.SetApplyToRootOrPageBackground(rootFrame, true);
            }
            else
            {
                Microsoft.UI.Xaml.Controls.BackdropMaterial.SetApplyToRootOrPageBackground(rootFrame, false);

                var isDark = GetCurrentThemeIsDark();
                var tintColor = isDark
                    ? Windows.UI.Color.FromArgb(255, 32, 32, 32)
                    : Windows.UI.Color.FromArgb(255, 243, 243, 243);

                rootFrame.Background = new Windows.UI.Xaml.Media.AcrylicBrush
                {
                    BackgroundSource = Windows.UI.Xaml.Media.AcrylicBackgroundSource.HostBackdrop,
                    TintColor = tintColor,
                    TintOpacity = 0.8,
                    FallbackColor = tintColor
                };

                if (rootFrame is FrameworkElement element)
                {
                    element.ActualThemeChanged -= RootElement_ActualThemeChanged;
                    element.ActualThemeChanged += RootElement_ActualThemeChanged;
                }
            }
        }

        //刷新标题栏颜色
        private void CustomizeTitleBarDirect()
        {
            try
            {
                var titleBar = Windows.UI.ViewManagement.ApplicationView.GetForCurrentView().TitleBar;

                titleBar.ButtonBackgroundColor = Windows.UI.Colors.Transparent;
                titleBar.ButtonInactiveBackgroundColor = Windows.UI.Colors.Transparent;

                var isDark = GetCurrentThemeIsDark();
                var fg = isDark ? Windows.UI.Colors.White : Windows.UI.Colors.Black;
                var inactiveFg = Windows.UI.Color.FromArgb(128, fg.R, fg.G, fg.B);
                var hoverBg = isDark
                    ? Windows.UI.Color.FromArgb(20, 255, 255, 255)
                    : Windows.UI.Color.FromArgb(20, 0, 0, 0);

                titleBar.ButtonForegroundColor = fg;
                titleBar.ButtonInactiveForegroundColor = inactiveFg;
                titleBar.ButtonHoverBackgroundColor = hoverBg;
                titleBar.ButtonHoverForegroundColor = fg;
                titleBar.ButtonPressedBackgroundColor = Windows.UI.Color.FromArgb(30, hoverBg.R, hoverBg.G, hoverBg.B);
                titleBar.ButtonPressedForegroundColor = fg;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"CustomizeTitleBarDirect 失败: {ex.Message}");
            }
        }

        // 获取当前主题是否为深色
        private bool GetCurrentThemeIsDark()
        {
            if (Window.Current.Content is FrameworkElement rootElement)
            {
                return rootElement.ActualTheme == ElementTheme.Dark;
            }
            return false;
        }

        //主题变化监听
        private void RootElement_ActualThemeChanged(FrameworkElement sender, object args)
        {
            Debug.WriteLine("主题变化，刷新 Acrylic 颜色");

            var material = localSettings.Values["AppMaterial"] as string;
            if (material == "Acrylic")
            {
                // 重新应用 Acrylic（触发 RbMaterial_SelectionChanged 的逻辑）
                var rootFrame = Window.Current.Content as Frame;
                if (rootFrame == null) return;

                var isDark = GetCurrentThemeIsDark();
                var tintColor = isDark
                    ? Windows.UI.Color.FromArgb(255, 32, 32, 32)
                    : Windows.UI.Color.FromArgb(255, 243, 243, 243);

                if (rootFrame.Background is Windows.UI.Xaml.Media.AcrylicBrush brush)
                {
                    brush.TintColor = tintColor;
                    brush.FallbackColor = tintColor;
                }
            }
        }

        private void PanePositionCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isInitializing) return;

            // 通过索引获取值，更可靠
            string selected = PanePositionCombo.SelectedIndex == 1 ? "Top" : "Left";
            localSettings.Values["PanePosition"] = selected;

            // 立即应用设置
            var frame = Window.Current.Content as Frame;
            if (frame?.Content is MainPage mainPage)
            {
                mainPage.ApplySettings();
            }
        }

        private void SoundToggle_Toggled(object sender, RoutedEventArgs e)
        {
            if (_isInitializing) return;

            bool isOn = SoundToggle.IsOn;
            localSettings.Values["EnableSound"] = isOn;

            ElementSoundPlayer.State = isOn
                ? ElementSoundPlayerState.On
                : ElementSoundPlayerState.Off;

            var frame = Window.Current.Content as Frame;
            if (frame?.Content is MainPage mainPage)
            {
                mainPage.ApplySettings();
            }
        }
    }
}