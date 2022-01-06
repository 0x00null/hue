using Autofac;
using Newtonsoft.Json.Linq;
using ZeroNull.Hue.Control;

namespace ZeroNull.Hue.HueActions
{
    /// <summary>
    /// Context passed to a hue action
    /// </summary>
    public class HueActionContext
    {
        /// <summary>
        /// The Scope in which the action is executing
        /// </summary>
        public ILifetimeScope Scope { get; set; }

        /// <summary>
        /// Gets the target action type
        /// </summary>
        public string TargetAction { get; set; }

        /// <summary>
        /// Gets the (optional) ControlEvent which caused this event to execute
        /// </summary>
        public ControlEvent SourceEvent { get; set; }

        /// <summary>
        /// Gets an options object containing any action configuration
        /// </summary>
        public JObject Options { get; set; }
    }
}
