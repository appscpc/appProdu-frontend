using System;

using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Plugin.CurrentActivity;
using Xamarin.Forms;
using Android;
using Android.Support.V4.App;
using Android.Support.Design.Widget;
using System.Collections.Generic;
using Android.Support.V4.Content;

namespace AppProdu.Droid
{
    [Activity(Label = "AppProdu", Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {

        string[] PermissionsArray = null;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            CrossCurrentActivity.Current.Activity = this;

            base.OnCreate(savedInstanceState);
            Rg.Plugins.Popup.Popup.Init(this, savedInstanceState);
            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);
            Xamarin.Forms.DependencyService.Register<SavePath, SavePathDroid>();

            LoadApplication(new App());

            updateNonGrantedPermissions();

            try
            {
                if (PermissionsArray != null && PermissionsArray.Length > 0)
                {
                    if (Build.VERSION.SdkInt >= BuildVersionCodes.M)
                    {
                        ActivityCompat.RequestPermissions(this, PermissionsArray, 0);
                    }
                }
            }
            catch (Exception oExp)
            {

            }
        }


        private void updateNonGrantedPermissions()
        {
            try
            {
                List<string> PermissionList = new List<string>();
                PermissionList.Add(Manifest.Permission.MediaContentControl);
                
                if (ContextCompat.CheckSelfPermission(Forms.Context, Manifest.Permission.WriteExternalStorage) != (int)Android.Content.PM.Permission.Granted)
                {
                    PermissionList.Add(Manifest.Permission.WriteExternalStorage);
                }
               
                PermissionsArray = new string[PermissionList.Count];
                for (int index = 0; index < PermissionList.Count; index++)
                {
                    PermissionsArray.SetValue(PermissionList[index], index);
                }
            }
            catch (Exception oExp)
            {

            }
        }

    }


}