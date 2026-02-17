using Microsoft.UI.Xaml.Controls;
using System;
using System.Diagnostics;
using UWP.Dialogs;
using Windows.ApplicationModel;
using Windows.Storage;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;

namespace UWP.Pages
{
    public sealed partial class SettingsPage : Page
    {
        private ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
        private bool _isInitializing = true;


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
                root.ActualThemeChanged -= AppThemeManager.OnActualThemeChanged;
                root.ActualThemeChanged += AppThemeManager.OnActualThemeChanged;
            }
        }

        private void LoadUI()
        {
            string theme = localSettings.Values["AppTheme"] as string ?? "System";
            RbTheme.SelectedIndex = theme switch
            {
                "Light" => 1,
                "Dark" => 2,
                _ => 0
            };

            string material = localSettings.Values["AppMaterial"] as string ?? "Mica";
            RbMaterial.SelectedIndex = material == "Acrylic" ? 1 : 0;

            string pos = localSettings.Values["PanePosition"] as string ?? "Left";
            PanePositionCombo.SelectedIndex = pos == "Top" ? 1 : 0;

            bool sound = true;
            if (localSettings.Values.ContainsKey("EnableSound"))
                sound = (bool)localSettings.Values["EnableSound"];
            else
                localSettings.Values["EnableSound"] = true;

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
                TxtCopyright.Text = $"©{DateTime.Now.Year} {Package.Current.PublisherDisplayName}。保留所有权利。";
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
                    await Launcher.LaunchUriAsync(new Uri(url));
            }
        }

        private async void RbTheme_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isInitializing) return;

            // ✅ 不转型、不读Tag，直接用索引
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
        }

        private void RbMaterial_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isInitializing) return;

            string value = RbMaterial.SelectedIndex == 1 ? "Acrylic" : "Mica";

            localSettings.Values["AppMaterial"] = value;
            AppThemeManager.CurrentMaterial = value == "Acrylic"
                ? BackgroundMaterial.Acrylic
                : BackgroundMaterial.Mica;

            var rootFrame = Window.Current.Content as Frame;
            if (rootFrame == null) return;

            if (value == "Mica")
            {
                if (rootFrame is FrameworkElement el)
                    el.ActualThemeChanged -= AppThemeManager.OnActualThemeChanged;

                rootFrame.Background = null;
                Microsoft.UI.Xaml.Controls.BackdropMaterial.SetApplyToRootOrPageBackground(rootFrame, true);

                if (rootFrame is FrameworkElement el2)
                    el2.ActualThemeChanged += AppThemeManager.OnActualThemeChanged;
            }
            else
            {
                Microsoft.UI.Xaml.Controls.BackdropMaterial.SetApplyToRootOrPageBackground(rootFrame, false);

                var isDark = AppThemeManager.GetIsDarkTheme(); // 统一调 AppThemeManager 的方法
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
                    element.ActualThemeChanged -= AppThemeManager.OnActualThemeChanged;
                    element.ActualThemeChanged += AppThemeManager.OnActualThemeChanged;
                }
            }
        }

        private void PanePositionCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isInitializing) return;

            string selected = PanePositionCombo.SelectedIndex == 1 ? "Top" : "Left";
            localSettings.Values["PanePosition"] = selected;

            var frame = Window.Current.Content as Frame;
            if (frame?.Content is MainPage mainPage)
                mainPage.ApplySettings();
        }

        private void SoundToggle_Toggled(object sender, RoutedEventArgs e)
        {
            if (_isInitializing) return;

            bool isOn = SoundToggle.IsOn;
            localSettings.Values["EnableSound"] = isOn;
            ElementSoundPlayer.State = isOn ? ElementSoundPlayerState.On : ElementSoundPlayerState.Off;

            var frame = Window.Current.Content as Frame;
            if (frame?.Content is MainPage mainPage)
                mainPage.ApplySettings();
        }
    }
}