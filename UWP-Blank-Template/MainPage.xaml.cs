using System;
using UWP.Pages;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.ApplicationModel.Core;
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
            TitleBarAppName.Text = SettingsPage.GetAppDisplayName();
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
            var localSettings = ApplicationData.Current.LocalSettings;

            // PanePosition
            if (localSettings.Values["PanePosition"] is string position)
            {
                NavView.PaneDisplayMode = position == "Top"
                    ? NavigationViewPaneDisplayMode.Top
                    : NavigationViewPaneDisplayMode.Left;
            }

            // Sound
            if (localSettings.Values["EnableSound"] is bool soundEnabled)
            {
                ElementSoundPlayer.State = soundEnabled
                    ? ElementSoundPlayerState.On
                    : ElementSoundPlayerState.Off;
            }
        }
    }
}