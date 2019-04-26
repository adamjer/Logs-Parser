using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SpirV___get_fail_reasons
{
    public class JobSet
    {
        [JsonProperty(PropertyName = "jobs")]
        public IList<Job> Jobs { get; set; }
    }
}
