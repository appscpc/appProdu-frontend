using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

using AppProdu.Droid;
using Xamarin.Forms;


namespace AppProdu.Droid
{
    public class PlaceholderEditor : Editor
    {
        public static readonly BindableProperty PlaceholderProperty =
        BindableProperty.Create(nameof(Placeholder), typeof(string), typeof(PlaceholderEditor));

        public static readonly BindableProperty PlaceholderColorProperty =
        BindableProperty.Create(nameof(PlaceholderColor), typeof(Color), typeof(PlaceholderEditor), Color.Gray);

        public PlaceholderEditor()
        {
        }

        public string Placeholder
        {
            get
            {
                return (string)GetValue(PlaceholderProperty);
            }

            set
            {
                SetValue(PlaceholderProperty, value);
            }
        }

        public Color PlaceholderColor
        {
            get
            {
                return (Color)GetValue(PlaceholderColorProperty);
            }

            set
            {
                SetValue(PlaceholderColorProperty, value);
            }
        }
    }
}