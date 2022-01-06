using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace ZeroNull.Hue.Api.ClipV2
{
    public class ResourceIdentifier
    {
        [JsonProperty("rid")]
        public string Id { get; set; }

        [JsonProperty("rtype")]
        public ResourceType ResourceType { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        public override string ToString()
        {
            return $"{ResourceType}:{(string.IsNullOrEmpty(Name) ? Id : $"{Name} ({Id})" )}";
        }
    }
}
