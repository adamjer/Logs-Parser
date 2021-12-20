using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpirV___get_fail_reasons.GTAX
{
    class JobOutput
    {
        [JsonProperty(PropertyName = "data")]
        public Data Data { get; set; }

        [JsonProperty(PropertyName = "total_messages")]
        public Data TotalMessages { get; set; }
    }

    class Data
    {
        [JsonProperty(PropertyName = "runs")]
        public IList<Run> Runs { get; set; }
    }

    class Run
    {
        [JsonProperty(PropertyName = "header")]
        public IList<Information> Header { get; set; }

        [JsonProperty(PropertyName = "messages")]
        public IList<IList<Information>> Messages { get; set; }
    }

    class Information
    {
        [JsonProperty(PropertyName = "message")]
        public String Message { get; set; }

        [JsonProperty(PropertyName = "indicator")]
        public String Indicator { get; set; }

        [JsonProperty(PropertyName = "levelname")]
        public String LevelName { get; set; }

        [JsonProperty(PropertyName = "source")]
        public String Source { get; set; }

        [JsonProperty(PropertyName = "task_no")]
        public String TaskNo { get; set; }

        [JsonProperty(PropertyName = "task_cmd")]
        public String TaskCMD { get; set; }

        [JsonProperty(PropertyName = "asctime")]
        public String AscTime { get; set; }

        [JsonProperty(PropertyName = "msg")]
        public String MSG { get; set; }

        [JsonProperty(PropertyName = "args")]
        public IList<String> Args { get; set; }
    }
}
