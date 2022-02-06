using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ResolutionGroupName("CompanyName")]
[assembly: ExportEffect(typeof(TychoClient.Droid.ShadowEffect), nameof(TychoClient.Droid.ShadowEffect))]
namespace TychoClient.Droid
{
    public class ShadowEffect : PlatformEffect
    {
        Android.Graphics.Color originalBackgroundColor = new Android.Graphics.Color(0, 0, 0, 0);
        Android.Graphics.Color backgroundColor;

        protected override void OnAttached()
        {
            try
            {
                var effect = (TychoClient.Effects.ShadowEffect)Element.Effects.FirstOrDefault(e => e is Effects.ShadowEffect);
                if (effect != null)
                {
                    float radius = effect.IsDefaultShadow ? 5 : effect.Radius;
                    float distanceX = effect.IsDefaultShadow ? 5 : effect.DistanceX; ;
                    float distanceY = effect.IsDefaultShadow ? 5 : effect.DistanceY; ;
                    Android.Graphics.Color color = effect.IsDefaultShadow ? new Android.Graphics.Color(255, 42, 0) : effect.Color.ToAndroid();
                    if (Control is TextView txt)
                        txt.SetShadowLayer(radius, distanceX, distanceY, color);
                    else if (Control is Android.Widget.Button button)
                        button.SetShadowLayer(radius, distanceX, distanceY, color);
                    else
                    { }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Cannot set property on attached control. Error: ", ex.Message);
            }


            //try
            //{
            //    backgroundColor = Android.Graphics.Color.LightGreen;
            //    Control.SetBackgroundColor(backgroundColor);
            //}
            //catch (Exception ex)
            //{
            //    Console.WriteLine("Cannot set property on attached control. Error: ", ex.Message);
            //}
        }

        protected override void OnDetached()
        {
        }

        //protected override void OnElementPropertyChanged(System.ComponentModel.PropertyChangedEventArgs args)
        //{
        //    base.OnElementPropertyChanged(args);
        //    try
        //    {
        //        if (args.PropertyName == "IsFocused")
        //        {
        //            if (((Android.Graphics.Drawables.ColorDrawable)Control.Background).Color == backgroundColor)
        //            {
        //                Control.SetBackgroundColor(originalBackgroundColor);
        //            }
        //            else
        //            {
        //                Control.SetBackgroundColor(backgroundColor);
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine("Cannot set property on attached control. Error: ", ex.Message);
        //    }
        //}
    }
}