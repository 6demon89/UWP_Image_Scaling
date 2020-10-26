using Microsoft.Advertising.WinRT.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace RescaleHelper
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {

        public MainPage()
        {
            this.InitializeComponent();
        }


        private Type currentPage;

        private readonly IList<(string Tag, Type Page)> _pages = new List<(string Tag, Type Page)>
        {
            ("Scale", typeof(View.ScalingPage)),
            //Add info page later on 
            ("info", typeof(View.InfoView)),
        };

        private void NavView_Loaded(object sender, RoutedEventArgs e)
        {

            ContentFrame.Navigated += On_Navigated;

            // NavView doesn't load any page by default: you need to specify it
            NavView_Navigate("Scale");

            // Add keyboard accelerators for backwards navigation
            var goBack = new KeyboardAccelerator { Key = VirtualKey.GoBack };
            goBack.Invoked += BackInvoked;
            this.KeyboardAccelerators.Add(goBack);

            // ALT routes here
            var altLeft = new KeyboardAccelerator
            {
                Key = VirtualKey.Left,
                Modifiers = VirtualKeyModifiers.Menu
            };
            altLeft.Invoked += BackInvoked;
            this.KeyboardAccelerators.Add(altLeft);
        }

        private void NavView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            if (args.InvokedItem == null)
                return;

            if (args.IsSettingsInvoked)
                ContentFrame.Navigate(typeof(View.ScalingPage));
            else
            {
                // Getting the Tag from Content (args.InvokedItem is the content of NavigationViewItem)
                var navItemTag = NavView.MenuItems
                    .OfType<NavigationViewItem>()
                    .First(i => args.InvokedItem.Equals(i.Content))
                    .Tag.ToString();

                NavView_Navigate(navItemTag);
            }
        }

        private void NavView_Navigate(string navItemTag)
        {
            var item = _pages.First(p => p.Tag.Equals(navItemTag));
            if (currentPage == item.Page)
                return;
            ContentFrame.Navigate(item.Page);

            currentPage = item.Page;
        }

        private void BackInvoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
        {
            On_BackRequested();
            args.Handled = true;
        }

        private bool On_BackRequested()
        {
            if (!ContentFrame.CanGoBack)
                return false;

            // Don't go back if the nav pane is overlayed
            if (NavView.IsPaneOpen &&
                (NavView.DisplayMode == NavigationViewDisplayMode.Compact ||
                NavView.DisplayMode == NavigationViewDisplayMode.Minimal))
                return false;

            ContentFrame.GoBack();
            return true;
        }

        private void On_Navigated(object sender, NavigationEventArgs e)
        {
            NavView.IsBackEnabled = ContentFrame.CanGoBack;

            if (ContentFrame.SourcePageType == typeof(View.ScalingPage))
            {
                // SettingsItem is not part of NavView.MenuItems, and doesn't have a Tag
                NavView.SelectedItem = (NavigationViewItem)NavView.SettingsItem;
            }
            else
            {
                var item = _pages.First(p => p.Page == e.SourcePageType);

                NavView.SelectedItem = NavView.MenuItems
                    .OfType<NavigationViewItem>()
                    .First(n => n.Tag.Equals(item.Tag));
            }
        }

        private void AdControl_ErrorOccurred(object sender, Microsoft.Advertising.WinRT.UI.AdErrorEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("AdControl error (" + ((AdControl)sender).Name +
                    "): " + e.ErrorMessage + " ErrorCode: " + e.ErrorCode.ToString());

        }
    }
}
