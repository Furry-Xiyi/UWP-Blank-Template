using System;
using UWP.Pages;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Core;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using NavigationView = Microsoft.UI.Xaml.Controls.NavigationView;
using NavigationViewBackRequestedEventArgs = Microsoft.UI.Xaml.Controls.NavigationViewBackRequestedEventArgs;
using NavigationViewPaneDisplayMode = Microsoft.UI.Xaml.Controls.NavigationViewPaneDisplayMode;

namespace UWP
{
    public sealed partial class MainPage : Page
    {
        private ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;

        public MainPage()
        {
            this.InitializeComponent();
            TitleBarAppName.Text = Package.Current.DisplayName;
            ImgAppIcon.Source = new BitmapImage(Package.Current.Logo);

            // 最原生的标题栏扩展
            var coreTitleBar = CoreApplication.GetCurrentView().TitleBar;
            coreTitleBar.ExtendViewIntoTitleBar = true;
            Window.Current.SetTitleBar(TitleBarArea);

            // 默认选中 Home
            NavView.SelectedItem = NavView.MenuItems[0];

            // 默认显示 HomePage
            ContentFrame.Navigate(typeof(Pages.HomePage));

            // 监听导航事件
            ContentFrame.Navigated += ContentFrame_Navigated;

            // 延迟应用设置，确保控件完全初始化
            this.Loaded += MainPage_Loaded;
        }

        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            // 在页面加载完成后应用设置
            ApplySettings();
            UpdateBackButton();
        }

        private void NavView_ItemInvoked(NavigationView sender, Microsoft.UI.Xaml.Controls.NavigationViewItemInvokedEventArgs args)
        {
            string tag = args.IsSettingsInvoked ? "settings" : args.InvokedItemContainer.Tag?.ToString();
            Type targetPage = null;

            switch (tag)
            {
                case "home":
                    targetPage = typeof(Pages.HomePage);
                    break;
                case "settings":
                    targetPage = typeof(Pages.SettingsPage);
                    break;
            }

            if (targetPage != null && ContentFrame.CurrentSourcePageType != targetPage)
            {
                ContentFrame.Navigate(targetPage);
            }
        }

        private void NavView_BackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs args)
        {
            if (ContentFrame.CanGoBack)
            {
                ContentFrame.GoBack();
            }
        }

        private void ContentFrame_Navigated(object sender, Windows.UI.Xaml.Navigation.NavigationEventArgs e)
        {
            UpdateBackButton();
            UpdateSelectedItem(e.SourcePageType);
        }

        private void UpdateBackButton()
        {
            NavView.IsBackEnabled = ContentFrame.CanGoBack;
        }

        private void UpdateSelectedItem(Type pageType)
        {
            if (pageType == typeof(Pages.HomePage))
            {
                NavView.SelectedItem = NavView.MenuItems[0];
            }
            else if (pageType == typeof(Pages.SettingsPage))
            {
                NavView.SelectedItem = NavView.SettingsItem;
            }
        }

        public void ApplySettings()
        {
            try
            {
                // 读取导航栏位置设置
                string position = "Left"; // 默认值
                if (localSettings.Values.ContainsKey("PanePosition"))
                {
                    position = localSettings.Values["PanePosition"] as string ?? "Left";
                }
                else
                {
                    // 第一次启动，设置默认值
                    localSettings.Values["PanePosition"] = "Left";
                }

                // 应用导航栏位置
                NavView.PaneDisplayMode = position switch
                {
                    "Top" => NavigationViewPaneDisplayMode.Top,
                    "Left" => NavigationViewPaneDisplayMode.Left,
                    _ => NavigationViewPaneDisplayMode.Left
                };

                // 读取声音设置
                bool soundEnabled = true; // 默认开启
                if (localSettings.Values.ContainsKey("EnableSound"))
                {
                    soundEnabled = (bool)localSettings.Values["EnableSound"];
                }
                else
                {
                    // 第一次启动，设置默认值
                    localSettings.Values["EnableSound"] = true;
                }

                // 应用声音设置
                ElementSoundPlayer.State = soundEnabled
                    ? ElementSoundPlayerState.On
                    : ElementSoundPlayerState.Off;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ApplySettings Error: {ex.Message}");
            }
        }
    }
}
