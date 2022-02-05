using TychoClient.Models;
using System;
using System.Collections.Generic;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using System.Linq;
using TychoClient.Services;
using TychoClient.ViewModels;

namespace TychoClient.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MenuPage : ContentPage
    {
        MainPage RootPage { get => Application.Current.MainPage as MainPage; }
        List<HomeMenuItem> menuItems;
        public MenuPage()
        {
            this.Log("Initializing MenuPage...");
            InitializeComponent();

            LoginData.UsernameChanged += (s,e) => UpdateGreetingLabel();
            LoginData.AdminStatusChanged += (s, e) => UpdateAdminItems();
            UpdateAdminItems();
            UpdateGreetingLabel();

            this.Log("MenuPage initialized!");

            menuItems = new List<HomeMenuItem>
            {
                //new HomeMenuItem {Id = MenuItemType.Browse, Title="Browse" },
                new HomeMenuItem {Id = MenuItemType.News, Title="News", Watcher = new SelectionWatcher(MenuItemType.News) },
                new HomeMenuItem {Id = MenuItemType.ReadCard, Title="Read Card", Watcher = new SelectionWatcher(MenuItemType.ReadCard) },
                new HomeMenuItem {Id = MenuItemType.Transaction, Title="Transaction", Watcher = new SelectionWatcher(MenuItemType.Transaction) },
                new HomeMenuItem {Id = MenuItemType.BarMode, Title="Bar Mode", Watcher = new SelectionWatcher(MenuItemType.BarMode), AdminOnly = true, IsVisible = false },
                new HomeMenuItem {Id = MenuItemType.Login, Title="Login", Watcher = new SelectionWatcher(MenuItemType.Login) }
            };

            ListViewMenu.ItemsSource = menuItems;

            ListViewMenu.SelectedItem = menuItems[0];
            ListViewMenu.ItemSelected += async (sender, e) =>
            {
                if (e.SelectedItem == null)
                    return;

                this.Log("Trying to change menu...");

                var oldSelectedItem = menuItems.FirstOrDefault(i => i.Watcher.IsSelected);
                var newSelectedItem = ((HomeMenuItem)e.SelectedItem);
                this.Log($"Selected Menu item changed from {(oldSelectedItem is null ? "None" : Enum.GetName(typeof(MenuItemType), oldSelectedItem.Watcher.WatchedMenuItem))} to {Enum.GetName(typeof(MenuItemType), newSelectedItem.Watcher.WatchedMenuItem)}");

                if(oldSelectedItem != null)
                    oldSelectedItem.Watcher.IsSelected = false;
                newSelectedItem.Watcher.IsSelected = true;
                try
                {
                    this.Log("Navigating now");
                    await RootPage.NavigateFromMenu((int)newSelectedItem.Id);
                    this.Log("Done Navigating");
                }
                catch(Exception ex)
                {
                    this.Log("Exception in MenuPage.xaml.cs: " + ex.ToString());
                }
            };
        }

        private void UpdateAdminItems()
        {
            if (ListViewMenu.ItemsSource is null)
                return;
            foreach (var homeMenuItem in ListViewMenu.ItemsSource?.Cast<HomeMenuItem>().Where(h => h.AdminOnly))
                if (homeMenuItem is HomeMenuItem h)
                    h.IsVisible = LoginData.IsAdmin;

        }

        public void UpdateGreetingLabel()
        {
            if (string.IsNullOrEmpty(LoginData.Username))
            {
                greetingLabel.IsVisible = false;
                return;
            }
            greetingLabel.IsVisible = true;
            greetingLabel.Text = $"Hello, {LoginData.Username}!";
        }
    }
}