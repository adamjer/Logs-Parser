using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SpirV___get_fail_reasons
{
    public class Result : IComparable
    {
        [JsonProperty(PropertyName = "gta_result_key")]
        public String GtaResultKey { get; set; }

        [JsonProperty(PropertyName = "job_id")]
        public String JobID { get; set; }

        [JsonProperty(PropertyName = "gta_status")]
        public String GTAStatus { get; set; }
        [JsonProperty(PropertyName = "status")]
        public String Status { get; set; }

        [JsonProperty(PropertyName = "id")]
        public String ID { get; set; }

        [JsonProperty(PropertyName = "artifacts")]
        public Artifacts Artifacts { get; set; }

        [JsonProperty(PropertyName = "phase_name")]
        public String PhaseName { get; set; }

        [JsonProperty(PropertyName = "business_attributes")]
        public BusinessAttributes BusinessAttributes { get; set; }

        public int CompareTo(object obj)
        {
            Result other = (Result)obj;
            return this.BusinessAttributes.ItemName.CompareTo(other.BusinessAttributes.ItemName);
        }
    }
}
