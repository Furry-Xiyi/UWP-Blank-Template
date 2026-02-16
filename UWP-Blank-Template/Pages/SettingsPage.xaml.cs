using Microsoft.UI.Xaml.Controls;
using System;
using Windows.ApplicationModel;
using Windows.Storage;
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

        private void LoadAppInfo()
        {
            try
            {
                // 获取应用名称
                TxtAppName.Text = Package.Current.DisplayName;

                // 获取应用版本
                var version = Package.Current.Id.Version;
                TxtVersion.Text = $"版本 {version.Major}.{version.Minor}.{version.Build}.{version.Revision}";

                // 获取应用图标
                var logoUri = Package.Current.Logo;
                ImgAppIcon.Source = new BitmapImage(logoUri);

                // 获取发布者信息并生成版权文本
                string publisher = GetPublisherName();
                int currentYear = DateTime.Now.Year;
                TxtCopyright.Text = $"©{currentYear} {publisher}。保留所有权利。";
            }
            catch (Exception ex)
            {
                // 如果获取失败，使用默认值
                TxtAppName.Text = "应用名称";
                TxtVersion.Text = "版本号获取失败";
                TxtCopyright.Text = $"©{DateTime.Now.Year} 开发者。保留所有权利。";
                System.Diagnostics.Debug.WriteLine($"LoadAppInfo failed: {ex.Message}");
            }
        }

        private string GetPublisherName()
        {
            try
            {
                // 尝试获取 PublisherDisplayName（友好名称）
                // 注意：这个属性在某些情况下可能不可用，取决于清单配置
                var package = Package.Current;

                // 方法1：尝试使用 Id.Publisher（这是证书主题名称，通常是 CN=xxx 格式）
                string publisher = package.Id.Publisher;

                // 如果是 CN= 格式，提取实际名称
                if (publisher.StartsWith("CN="))
                {
                    publisher = publisher.Substring(3);

                    // 如果包含逗号，取第一部分
                    int commaIndex = publisher.IndexOf(',');
                    if (commaIndex > 0)
                    {
                        publisher = publisher.Substring(0, commaIndex);
                    }
                }

                return string.IsNullOrWhiteSpace(publisher) ? "开发者" : publisher;
            }
            catch
            {
                return "开发者";
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