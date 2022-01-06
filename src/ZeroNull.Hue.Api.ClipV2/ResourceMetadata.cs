using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace ZeroNull.Hue.Api.ClipV2
{
    public class ResourceMetadata
    {
        
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("archetype")]
        public string Archetype { get; set; }

    }
}
