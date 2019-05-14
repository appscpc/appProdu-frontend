using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppProdu.UWP
{
    class SavePathUwp : SavePath
    {
        public string getSavePath()
        {
            return Environment.GetFolderPath(Environment.SpecialFolder.Personal);

        }

    }
}
