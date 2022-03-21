using System.Linq;
using TychoClient.Effects;
using TychoClient.Views;
using Xamarin.Forms;

namespace TychoClient.AttachedProperties
{
    public static class Attached
    {
        public static bool GetOpensMenu(BindableObject target) => (bool)target.GetValue(OpensMenuProperty);
        public static void SetOpensMenu(BindableObject target, bool value) => target.SetValue(OpensMenuProperty, value);
        public static readonly BindableProperty OpensMenuProperty =
                BindableProperty.CreateAttached("OpensMenu", typeof(bool), typeof(Attached), false, BindingMode.OneWay, null, OnOpensMenuChanged);

        public static void OnOpensMenuChanged(BindableObject sender, object oldValue, object newValue)
        {
            if (((bool)newValue) && sender is Button b)
            {
                b.Clicked += (s, e) => MainPage.Instance.IsPresented = true;
            }
        }


        public static bool GetHasShadow(BindableObject target) => (bool)target.GetValue(HasShadowProperty);
        public static void SetHasShadow(BindableObject target, bool value) => target.SetValue(HasShadowProperty, value);
        public static readonly BindableProperty HasShadowProperty =
                BindableProperty.CreateAttached("HasShadow", typeof(bool), typeof(Attached), false, BindingMode.OneWay, null, OnHasShadowChanged);

        public static void OnHasShadowChanged(BindableObject sender, object oldValue, object newValue)
        {
            if (((bool)newValue) && sender is View view)
            {
                view.Effects.Add(new ShadowEffect());
            }
            if (!((bool)newValue) && sender is View view2)
            {
                if (view2.Effects.FirstOrDefault(e => e is ShadowEffect) is ShadowEffect effect)
                    view2.Effects.Remove(effect);
            }
        }
    }
}
