using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace ZeroNull.Hue.Api.ClipV2
{
    public class LightGetModel : GetModel
    {
        [JsonProperty("on")]
        public OnModel On { get; set; }

        [JsonProperty("dimming")]
        public DimmingModel Dimming { get; set; }

        [JsonProperty("color")]
        public ColorModel Color { get; set; }

        /// <summary>
        /// Returns a JObject which can be used to patch a light
        /// </summary>
        /// <returns></returns>
        public JObject AsPatch()
        {
            var patch = new JObject();
            if (On != null)
            {
                patch.Add("on", JObject.FromObject(this.On));
            }

            if (Dimming != null)
            {
                patch.Add("dimming", JObject.FromObject(this.Dimming));
            }

            if (Color != null)
            {
                patch.Add("color", JObject.FromObject(this.Color));
            }

            return patch;
        }
    }


}
