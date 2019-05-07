using System;
using System.Collections.Generic;
using System.Text;

namespace AppProdu
{
    public class RegistroUsuario
    {
        public int path_id { get; set; }
        public int activity_id { get; set; }
        public string nombre_activity { get; set; }
        public int activity_type_id { get; set; }
        public string nombre_activity_type { get; set; }
        public string token { get; set; }
    }
}
