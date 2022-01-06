using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace ZeroNull.Hue.Api.ClipV2
{
    public class GetModel
    {
        [JsonProperty("type")]
        public ResourceType Type { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("metadata")]
        public ResourceMetadata Metadata { get; set; }

        [JsonProperty("services")]
        public IEnumerable<ResourceIdentifier> Services { get; set; }

        [JsonProperty("children")]
        public IEnumerable<ResourceIdentifier> Children { get; set; }

        public static implicit operator ResourceIdentifier(GetModel s)
        {
            if (s == null)
            {
                return s;
            }

            return new ResourceIdentifier() { Id = s.Id, ResourceType = s.Type, Name = s.Metadata?.Name };
        }
    }
}
