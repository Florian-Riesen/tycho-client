﻿using System;
using System.Linq;
using TychoClient.Models;
using TychoClient.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace TychoClient.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class BarScannerPage : ContentPage
    {
        public BarScannerPage()
        {
            InitializeComponent();
            if (BindingContext is NfcAwareViewModel vm)
                vm.Watcher = AllSelectionWatchers.List.FirstOrDefault(w => w.WatchedMenuItem == MenuItemType.BarMode);
        }
    }
}