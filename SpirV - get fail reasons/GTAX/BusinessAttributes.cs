using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SpirV___get_fail_reasons.GTAX
{
    public class BusinessAttributes
    {
        [JsonProperty(PropertyName = "gta.planning.item.owner")]
        public String ItemOwner { get; set; }

        [JsonProperty(PropertyName = "gta.planning.item.itemId")]
        public String ItemID { get; set; }

        [JsonProperty(PropertyName = "gta.planning.item.itemKey")]
        public String ItemKey { get; set; }

        [JsonProperty(PropertyName = "gta.planning.item.itemName")]
        public String ItemName { get; set; }

        [JsonProperty(PropertyName = "gta.planning.execution.args")]
        public String ExecutionArguments { get; set; }

        [JsonProperty(PropertyName = "gta.planning.item.namespace")]
        public String ItemNamespace { get; set; }

        [JsonProperty(PropertyName = "gta.planning.item.timestamp")]
        public String ItemTimeStamp { get; set; }
    }
}
