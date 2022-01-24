﻿using TychoClient.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using TychoClient.Services;

namespace TychoClient.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MainPage : MasterDetailPage
    {
        Dictionary<int, NavigationPage> MenuPages = new Dictionary<int, NavigationPage>();
        public MainPage()
        {
            InitializeComponent();

            MasterBehavior = MasterBehavior.Popover;

            MenuPages.Add((int)MenuItemType.News, (NavigationPage)Detail);
        }

        public async Task NavigateFromMenu(int id)
        {
            Log.Line("Trying to navigate in MainPage");
            if (!MenuPages.ContainsKey(id))
            {
                Log.Line("Page not yet loaded. Loading.");
                switch (id)
                {
                    //case (int)MenuItemType.Browse:
                    //    MenuPages.Add(id, new NavigationPage(new ItemsPage()));
                    //    break;
                    case (int)MenuItemType.News:
                        MenuPages.Add(id, new NavigationPage(new NewsPage()));
                        break;
                    case (int)MenuItemType.ReadCard:
                        MenuPages.Add(id, new NavigationPage(new ReadCardPage()));
                        break;
                    case (int)MenuItemType.Login:
                        MenuPages.Add(id, new NavigationPage(new LoginPage()));
                        break;
                    case (int)MenuItemType.BarMode:
                        MenuPages.Add(id, new NavigationPage(new BarScannerPage()));
                        break;
                    case (int)MenuItemType.Transaction:
                        MenuPages.Add(id, new NavigationPage(new TransactionPage()));
                        break;
                }
            }

            var newPage = MenuPages[id];

            if (newPage != null && Detail != newPage)
            {

                Log.Line("Navigating now.");
                Detail = newPage;

                if (Device.RuntimePlatform == Device.Android)
                    await Task.Delay(100);

                IsPresented = false;
            }
        }
    }
}