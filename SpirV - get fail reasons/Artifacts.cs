using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SpirV___get_fail_reasons
{
    public class Artifacts
    {
        [JsonProperty(PropertyName = "task_logs")]
        public IList<String> TaskLogs { get; set; }
    }
}
