using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;

namespace AppProdu.iOS
{
    class SavePathIos : SavePath
    {
        public string getSavePath()
        {
            return Environment.GetFolderPath(Environment.SpecialFolder.Personal);

        }

    }
}