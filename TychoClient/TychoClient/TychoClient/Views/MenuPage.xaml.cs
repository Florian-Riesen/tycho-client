using TychoClient.Models;
using System;
using System.Collections.Generic;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using System.Linq;
using TychoClient.Services;

namespace TychoClient.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MenuPage : ContentPage
    {
        MainPage RootPage { get => Application.Current.MainPage as MainPage; }
        List<HomeMenuItem> menuItems;
        public MenuPage()
        {
            InitializeComponent();

            Log.Line("MenuPage initialized!");

            menuItems = new List<HomeMenuItem>
            {
                //new HomeMenuItem {Id = MenuItemType.Browse, Title="Browse" },
                new HomeMenuItem {Id = MenuItemType.News, Title="News", Watcher = new SelectionWatcher(MenuItemType.News) },
                new HomeMenuItem {Id = MenuItemType.ReadCard, Title="Read Card", Watcher = new SelectionWatcher(MenuItemType.ReadCard) },
                new HomeMenuItem {Id = MenuItemType.Transaction, Title="Transaction", Watcher = new SelectionWatcher(MenuItemType.Transaction) },
                new HomeMenuItem {Id = MenuItemType.BarMode, Title="Bar Mode", Watcher = new SelectionWatcher(MenuItemType.BarMode) },
                new HomeMenuItem {Id = MenuItemType.Login, Title="Login", Watcher = new SelectionWatcher(MenuItemType.Login) }
            };

            ListViewMenu.ItemsSource = menuItems;

            ListViewMenu.SelectedItem = menuItems[0];
            ListViewMenu.ItemSelected += async (sender, e) =>
            {
                if (e.SelectedItem == null)
                    return;

                Log.Line("Trying to change menu...");

                var oldSelectedItem = menuItems.FirstOrDefault(i => i.Watcher.IsSelected);
                var newSelectedItem = ((HomeMenuItem)e.SelectedItem);
                Log.Line($"Selected Menu item changed from {(oldSelectedItem is null ? "None" : Enum.GetName(typeof(MenuItemType), oldSelectedItem.Watcher.WatchedMenuItem))} to {Enum.GetName(typeof(MenuItemType), newSelectedItem.Watcher.WatchedMenuItem)}");

                if(oldSelectedItem != null)
                    oldSelectedItem.Watcher.IsSelected = false;
                newSelectedItem.Watcher.IsSelected = true;
                try
                {
                    Log.Line("Navigating now");
                    await RootPage.NavigateFromMenu((int)newSelectedItem.Id);
                    Log.Line("Done Navigating");
                }
                catch(Exception ex)
                {
                    Log.Line("Exception in MenuPage.xaml.cs: " + ex.ToString());
                }
            };
        }
    }
}