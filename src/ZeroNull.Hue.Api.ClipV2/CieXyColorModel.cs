using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace ZeroNull.Hue.Api.ClipV2
{
    public class CieXyColorModel
    {
        [JsonProperty("x")]
        public decimal X { get; set; }

        [JsonProperty("y")]
        public decimal Y { get; set; }
    }
}
