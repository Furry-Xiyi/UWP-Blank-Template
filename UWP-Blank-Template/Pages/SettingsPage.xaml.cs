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
            foreach (ComboBoxItem item in PanePositionCombo.Items)
            {
                if (item.Tag?.ToString() == pos)
                {
                    PanePositionCombo.SelectedItem = item;
                    break;
                }
            }

            // 声音
            bool sound = (localSettings.Values["EnableSound"] as bool?) ?? true;
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

            string selected = (PanePositionCombo.SelectedItem as ComboBoxItem)?.Tag?.ToString() ?? "Left";
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