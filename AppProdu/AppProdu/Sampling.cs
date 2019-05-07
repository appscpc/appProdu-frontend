using System;
using System.Collections.Generic;
using System.Text;

namespace AppProdu
{
    class Sampling
    {
        public int id { get; set; }
        public string nombre { get; set; }
        public int? cantMuestras { get; set; }
        public int? cantMuestrasTotal { get; set; }
        public string descripcion { get; set; }
        public int fase { get; set; }
        public int project_id { get; set; }
        public int? sampling_type_id { get; set; }
        public int? muestrasActual { get; set; }
        public string token { get; set; }
    }
}
