using System;
using System.Collections.Generic;
using System.Text;

namespace AppProdu
{
    class Path
    {
        public int id { get; set; }
        public int cantOperarios { get; set; }
        public double temperatura { get; set; }
        public int humedad { get; set; }
        public string fecha { get; set; }
        public string hora { get; set; }
        public string comentario { get; set; }
        public string token { get; set; }
        public int fase_id { get; set; }
        public int sampling_id { get; set; }
    }
}
