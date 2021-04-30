using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpirV___get_fail_reasons
{
    class GtaxResult
    {
        public String Name { get; set; }
        public String Status { get; set; }
        public String Link { get; set; }
        public List<String> ParsedResults { get; set; }

        public GtaxResult()
        {
            Name = "";
            ParsedResults = new List<String>();
            Link = "";
            Status = "";
        }

        public void SetLink(String jobID, String ID)
        {
            if (Program.environment == EnvironmentType.Silicon)
                Link = $"http://gtax-igk.intel.com/#/jobs/{jobID}/task_results/{ID}";
            else if (Program.environment == EnvironmentType.Simulation)
                Link = $"http://gtax-presi-igk.intel.com/#/jobs/{jobID}/task_results/{ID}";
            else if (Program.environment == EnvironmentType.Emulation)
                throw new Exception("Emulation not supported yet!");
        }
    }
}
