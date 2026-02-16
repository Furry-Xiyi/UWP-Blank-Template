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

            // 延迟到 Loaded 事件，确保所有控件都已完全初始化
            this.Loaded += SettingsPage_Loaded;
        }

        private void SettingsPage_Loaded(object sender, RoutedEventArgs e)
        {
            // 在页面完全加载后才加载设置
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

            // 导航栏位置 - 关键修复：通过索引设置，而不是查找 Tag
            string pos = localSettings.Values["PanePosition"] as string ?? "Left";
            PanePositionCombo.SelectedIndex = pos == "Top" ? 1 : 0;

            // 声音 - 确保正确读取和设置
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
            catch { }
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

            // 立即应用设置
            ElementSoundPlayer.State = isOn
                ? ElementSoundPlayerState.On
                : ElementSoundPlayerState.Off;

            // 同时通知 MainPage 应用设置
            var frame = Window.Current.Content as Frame;
            if (frame?.Content is MainPage mainPage)
            {
                mainPage.ApplySettings();
            }
        }
    }
}
