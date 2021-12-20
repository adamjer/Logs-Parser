using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SpirV___get_fail_reasons.GTAX
{
    public class JobSetSessionData
    {
        [JsonProperty(PropertyName = "data")]
        public IList<JobSetSessionNS.JobSetSession> JobSetSessions { get; set; }
    }
}
