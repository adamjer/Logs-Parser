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
        public String Machine { get; set; }
        public List<String> ParsedResults { get; set; }

        public GtaxResult()
        {
            Name = "";
            ParsedResults = new List<String>();
            Link = "";
            Machine = "";
            Status = "";
        }

        public void SetLink(String jobID, String ID)
        {
            Link = $"http://{Program.instance}#/jobs/{jobID}/task_results/{ID}";
        }
    }
}
