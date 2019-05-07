using System;
using System.Collections.Generic;
using System.Text;

namespace AppProdu
{
    public class Fase
    {
        public int id { get; set; }
        public float p { get; set; }
        public float q { get; set; }
        public float error { get; set; }
        public float z { get; set; }
        public int sampling_id { get; set; }
        public int fase_type_id { get; set; }  
        public int extraFlag { get; set; }
        public string token { get; set; }
    }
}
