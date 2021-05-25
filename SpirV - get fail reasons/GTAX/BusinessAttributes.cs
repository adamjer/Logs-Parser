using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SpirV___get_fail_reasons
{
    public class BusinessAttributes
    {
        [JsonProperty(PropertyName = "gta.planning.item.itemName")]
        public String ItemName { get; set; }

        [JsonProperty(PropertyName = "gta.planning.execution.args")]
        public String ExecutionArguments { get; set; }
    }
}
