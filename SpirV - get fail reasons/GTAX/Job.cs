using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SpirV___get_fail_reasons.GTAX
{
    public class Job
    {
        [JsonProperty(PropertyName = "id")]
        public String ID { get; set; }

        [JsonProperty(PropertyName = "name")]
        public String Name { get; set; }

        [JsonProperty(PropertyName = "phase_type")]
        public String PhaseType { get; set; }

        [JsonProperty(PropertyName = "results")]
        public IList<Result> Results { get; set; }
    }
}
