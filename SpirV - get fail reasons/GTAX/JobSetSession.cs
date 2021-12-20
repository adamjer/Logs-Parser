using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace JobSetSessionNS
{
    namespace BusinessAttributesNS.GTAX
    {
        public class BusinessAttributes
        {
            [JsonProperty(PropertyName = "planning")]
            public PlanningNS.Planning Planning { get; set; }
        }

        namespace PlanningNS
        {
            public class Planning
            {
                [JsonProperty(PropertyName = "attributes")]
                public Attributes Attributes { get; set; }
            }

            public class Attributes
            {
                [JsonProperty(PropertyName = "Timeout")]
                public String Timeout { get; set; }

                [JsonProperty(PropertyName = "Platform")]
                public String Platform { get; set; }

                [JsonProperty(PropertyName = "Environment")]
                public String Environment { get; set; }

                [JsonProperty(PropertyName = "Operating_System")]
                public String OperatingSystem { get; set; }
            }
        }
    }

    public class JobSetSession
    {
        [JsonProperty(PropertyName = "id")]
        public int ID { get; set; }

        [JsonProperty(PropertyName = "status")]
        public String Status { get; set; }

        [JsonProperty(PropertyName = "business_attr")]
        public BusinessAttributesNS.GTAX.BusinessAttributes BusinessAttributes { get; set; }
    }
}
