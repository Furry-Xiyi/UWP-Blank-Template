using Microsoft.UI.Xaml.Controls;
using System;
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
        private bool _isInitializing = true; // 初始化标志

        public SettingsPage()
        {
            this.InitializeComponent();
            LoadUI();
            LoadAppInfo();
            _isInitializing = false; // 初始化完毕
        }

        private void LoadUI()
        {
            // 主题 - 通过索引设置，不会触发多次事件
            string theme = localSettings.Values["AppTheme"] as string ?? "System";
            RbTheme.SelectedIndex = theme switch
            {
                "Light" => 1,
                "Dark" => 2,
                _ => 0 // "System" 或其他
            };

            // 材质 - 通过索引设置
            string material = localSettings.Values["AppMaterial"] as string ?? "Mica";
            RbMaterial.SelectedIndex = material == "Acrylic" ? 1 : 0;

            // 导航栏位置
            string pos = localSettings.Values["PanePosition"] as string ?? "Left";
            PanePositionCombo.SelectedIndex = pos == "Top" ? 1 : 0;

            // 声音
            bool sound = (localSettings.Values["EnableSound"] as bool?) ?? true;
            SoundToggle.IsOn = sound;
        }

        public static string GetAppDisplayName()
        {
            try
            {
                return Package.Current.DisplayName;
            }
            catch
            {
                return "应用名称";
            }
        }

        public void LoadAppInfo()
        {
            try
            {
                // 应用名称
                TxtAppName.Text = AppInfoHelper.GetAppDisplayName();

                // 应用版本
                TxtVersion.Text = $"版本 {AppInfoHelper.GetAppVersion()}";

                // 应用图标
                ImgAppIcon.Source = AppInfoHelper.GetAppLogo();

                // 版权信息
                string publisher = AppInfoHelper.GetPublisherName();
                int year = DateTime.Now.Year;
                TxtCopyright.Text = $"©{year} {publisher}。保留所有权利。";
            }
            catch (Exception ex)
            {
                TxtAppName.Text = "应用名称";
                TxtVersion.Text = "版本号获取失败";
                ImgAppIcon.Source = null;
                int year = DateTime.Now.Year;
                string publisher = "开发者";
                System.Diagnostics.Debug.WriteLine($"LoadAppInfo failed: {ex.Message}");

                // 版权
                TxtCopyright.Text = $"©{year} {publisher}。保留所有权利。";
            }
        }

        public static class AppInfoHelper
        {
            public static string GetAppDisplayName()
            {
                try
                {
                    return Package.Current.DisplayName;
                }
                catch
                {
                    return "应用名称";
                }
            }

            public static string GetAppVersion()
            {
                try
                {
                    var v = Package.Current.Id.Version;
                    return $"{v.Major}.{v.Minor}.{v.Build}.{v.Revision}";
                }
                catch
                {
                    return "版本号获取失败";
                }
            }

            public static BitmapImage GetAppLogo()
            {
                try
                {
                    return new BitmapImage(Package.Current.Logo);
                }
                catch
                {
                    return null;
                }
            }

            public static string GetPublisherName()
            {
                try
                {
                    return Package.Current.PublisherDisplayName;
                }
                catch
                {
                    return "开发者";
                }
            }
        }

        private string GetPublisherName()
        {
            try
            {
                string displayName = Package.Current.PublisherDisplayName;
                return string.IsNullOrWhiteSpace(displayName) ? "开发者" : displayName;
            }
            catch
            {
                return "开发者";
            }
        }

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

            // 从选中的 RadioButton 获取 Tag
            var selectedItem = RbTheme.SelectedItem as RadioButton;
            string value = selectedItem?.Tag?.ToString() ?? "System";

            localSettings.Values["AppTheme"] = value;

            AppThemeManager.LoadSettings();
            AppThemeManager.ApplyTheme();
        }

        private void RbMaterial_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isInitializing) return;

            // 从选中的 RadioButton 获取 Tag
            var selectedItem = RbMaterial.SelectedItem as RadioButton;
            string value = selectedItem?.Tag?.ToString() ?? "Mica";

            localSettings.Values["AppMaterial"] = value;

            AppThemeManager.LoadSettings();
            AppThemeManager.ApplyMaterial();
        }

        private void PanePositionCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isInitializing) return;

            string selected = (PanePositionCombo.SelectedItem as ComboBoxItem)?.Content.ToString();
            localSettings.Values["PanePosition"] = selected;

            var frame = Window.Current.Content as Frame;
            if (frame?.Content is MainPage mainPage)
                mainPage.ApplySettings();
        }

        private void SoundToggle_Toggled(object sender, RoutedEventArgs e)
        {
            if (_isInitializing) return;

            localSettings.Values["EnableSound"] = SoundToggle.IsOn;

            var frame = Window.Current.Content as Frame;
            if (frame?.Content is MainPage mainPage)
                mainPage.ApplySettings();
        }
    }
}