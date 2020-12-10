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
        public String Status { get; set; }
        public String GTAXlink { get; set; }
        public List<String> SubTask { get; set; }

        public SpirVTask()
        {
            Name = "";
            SubTask = new List<String>();
            GTAXlink = "";
            Status = "";
        }

        public void SetLink(String jobID, String ID)
        {
            if (Program.environment == EnvironmentType.Silicon)
                GTAXlink = $"http://gtax-igk.intel.com/#/jobs/{jobID}/task_results/{ID}";
            else if (Program.environment == EnvironmentType.Simulation)
                GTAXlink = $"http://gtax-presi-igk.intel.com/#/jobs/{jobID}/task_results/{ID}";
            else if (Program.environment == EnvironmentType.Emulation)
                throw new Exception("Emulation not supported yet!");
        }
    }
}
