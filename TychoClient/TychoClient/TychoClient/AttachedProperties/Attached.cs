using TychoClient.Effects;
using Xamarin.Forms;

namespace TychoClient.AttachedProperties
{
    public static class Attached
    {
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
        }
    }
}
