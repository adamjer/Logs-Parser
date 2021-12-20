using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SpirV___get_fail_reasons.GTAX
{
    public class Artifacts
    {
        [JsonProperty(PropertyName = "task_logs")]
        public IList<String> TaskLogs { get; set; }

        [JsonProperty(PropertyName = "task_dumps")]
        public IList<String> TaskDumps { get; set; }

        [JsonProperty(PropertyName = "task_results")]
        public IList<String> TaskEesults { get; set; }

        [JsonProperty(PropertyName = "artifacts_scheme_version")]
        public String ArtifactsSchemeVersion { get; set; }
    }
}
