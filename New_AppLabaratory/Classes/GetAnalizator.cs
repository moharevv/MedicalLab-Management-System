using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static AppLaboratory.Labs.Laborant2.AnalyserWork;

namespace New_AppLabaratory.Classes
{
    public class GetAnalizator
    {
        public string patient { get; set; }
        public List<Service> services { get; set; } 
        public int? progress { get; set; }
    }
}
