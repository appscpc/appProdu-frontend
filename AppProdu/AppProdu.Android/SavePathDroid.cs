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
using Environment = Android.OS.Environment;

namespace AppProdu.Droid
{
    public class SavePathDroid : SavePath
    {
        public string getSavePath() {


            return Environment.GetExternalStoragePublicDirectory(Environment.DirectoryDownloads).AbsolutePath;

        }

    }
}