using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace ZeroNull.Hue.Api.ClipV2
{
    public class OnModel
    {
        [JsonProperty("on")]
        public bool On { get; set; }
    }
}
