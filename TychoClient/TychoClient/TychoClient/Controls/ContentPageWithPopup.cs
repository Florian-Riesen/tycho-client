using System;
using System.Collections.Generic;
using System.Text;
using TychoClient.Services;
using Xamarin.Forms;

namespace TychoClient.Controls
{
    public class ContentPageWithPopup : ContentPage
    {
        public IPopupViewModel CurrentPopup
        {
            get => (IPopupViewModel)GetValue(CurrentPopupProperty);
            set => SetValue(CurrentPopupProperty, value);
        }

        public static readonly BindableProperty CurrentPopupProperty =
            BindableProperty.Create("CurrentPopup", typeof(IPopupViewModel), typeof(ContentPageWithPopup), null, BindingMode.TwoWay, null, CurrentPopupChanged );

        public static void CurrentPopupChanged(BindableObject bindable, object oldValue, object newValue)
        {

        }
    }
}
