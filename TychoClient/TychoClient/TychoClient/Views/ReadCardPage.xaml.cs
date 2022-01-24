using System;
using TychoClient.Models;
using TychoClient.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace TychoClient.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ReadCardPage : ContentPage
    {
        public ReadCardPage()
        {
            InitializeComponent();
        }

        public void SetSelectionWatcher(SelectionWatcher watcher)
        {
            if (BindingContext is BaseViewModel b)
                b.Watcher = watcher;
        }
    }
}