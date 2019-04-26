using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SpirV___get_fail_reasons
{
    public class Job
    {
        [JsonProperty(PropertyName = "id")]
        public String ID { get; set; }

        [JsonProperty(PropertyName = "results")]
        public IList<Result> Results { get; set; }
    }
}
