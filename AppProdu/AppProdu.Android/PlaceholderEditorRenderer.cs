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
using AppProdu.Droid;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(PlaceholderEditor), typeof(PlaceholderEditorRenderer))]
namespace AppProdu.Droid
{
    public class PlaceholderEditorRenderer : EditorRenderer
    {
        public PlaceholderEditorRenderer(Context context):base(context)
        {

        }
        protected override void OnElementChanged(ElementChangedEventArgs<Editor> e)
        {
            base.OnElementChanged(e);
            if (Control != null)
            {
                if (Element == null)
                    return;

                var element = (PlaceholderEditor)Element;
                Control.Hint = element.Placeholder;
                Control.SetHintTextColor(element.PlaceholderColor.ToAndroid());
            }
        }
    }
}