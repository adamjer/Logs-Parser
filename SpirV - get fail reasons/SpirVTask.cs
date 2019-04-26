using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpirV___get_fail_reasons
{
    class SpirVTask
    {
        public String Name { get; set; }
        public List<String> FailedSubTask { get; set; }

        public SpirVTask()
        {
            Name = "";
            FailedSubTask = new List<String>();
        }
    }
}
