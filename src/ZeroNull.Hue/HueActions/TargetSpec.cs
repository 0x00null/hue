using Newtonsoft.Json;
using ZeroNull.Hue.Api.ClipV2;

namespace ZeroNull.Hue.HueActions
{
    /// <summary>
    /// Overlay used to specify an explicit target for an Action
    /// </summary>
    public class TargetSpec
    {
        /// <summary>
        /// The name of an explicit action target
        /// </summary>
        [JsonProperty("target")]
        public string Target { get; set; }

        /// <summary>
        /// The (optional) type of an explicit action target
        /// </summary>
        [JsonProperty("targetType")]
        public ResourceType? TargetType { get; set; }
    }
}
