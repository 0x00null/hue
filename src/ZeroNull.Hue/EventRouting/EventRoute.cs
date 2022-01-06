using Newtonsoft.Json.Linq;
using ZeroNull.Hue.Control;

namespace ZeroNull.Hue.EventRouting
{
    /// <summary>
    /// Maps a control event to an action
    /// </summary>
    public class EventRoute
    {
        /// <summary>
        /// Gets or sets the Input ID to watch out for
        /// </summary>
        public string InputId { get; set; }

        /// <summary>
        /// If set, trigger only when the Value is equal to or above this threshold
        /// </summary>
        public byte? TriggerAbove { get; set; }

        /// <summary>
        /// If set, trigger only when the Value is below this threshold
        /// </summary>
        public byte? TriggerBelow { get; set; }

        /// <summary>
        /// If set, trigger only for events of the specified type
        /// </summary>
        public ControlEventType? EventType { get; set; }

        /// <summary>
        /// Gets or sets the ID (or alias) of the action to fire when triggered
        /// </summary>
        public string TargetAction { get; set; }

        /// <summary>
        /// Options to pass to the Action
        /// </summary>
        public JObject Options { get; set; }

    }
}
