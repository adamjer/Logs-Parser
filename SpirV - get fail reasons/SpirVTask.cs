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
        public String GTAXlink { get; set; }
        public List<String> SubTask { get; set; }

        public SpirVTask()
        {
            Name = "";
            SubTask = new List<String>();
            GTAXlink = "";
        }

        public void SetLink(String jobID, String ID)
        {
            GTAXlink = $"https://gtax-igk.intel.com/#/jobs/{jobID}/task_results/{ID}";
        }
    }
}
