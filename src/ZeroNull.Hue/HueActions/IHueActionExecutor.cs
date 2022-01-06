using Newtonsoft.Json.Linq;
using ZeroNull.Hue.Control;

namespace ZeroNull.Hue.HueActions
{
    /// <summary>
    /// An object capable of executing hue actions
    /// </summary>
    public interface IHueActionExecutor
    {
        /// <summary>
        /// Executes the specified action
        /// </summary>
        /// <param name="actionName"></param>
        /// <param name="options"></param>
        /// <param name="sourceEvent"></param>
        /// <returns></returns>
        Task Execute(string actionName, JObject options, ControlEvent sourceEvent);

        /// <summary>
        /// Returns a list of all supported actions
        /// </summary>
        /// <returns></returns>
        IEnumerable<HueActionAttribute> GetAllActions();
    }
}
