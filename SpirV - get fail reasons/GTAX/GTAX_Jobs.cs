using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SpirV___get_fail_reasons.GTAX
{
    public class GTAX_Jobs
    {
        [JsonProperty(PropertyName = "jobs")]
        public IList<Job> Jobs { get; set; }
    }
}
