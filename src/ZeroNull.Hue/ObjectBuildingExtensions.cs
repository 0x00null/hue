using Newtonsoft.Json.Linq;

namespace ZeroNull.Hue
{
    public static class ObjectBuildingExtensions
    {
        /// <summary>
        /// Unpacks a commandline style value to a JObject
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        public static JObject ToJObject(this IEnumerable<string> options)
        {
            // build an options object out of any remaining values
            var actionOptions = new JObject();
            JArray itemsPropValue = null;
            foreach (var option in options)
            {
                if (option.Contains("="))
                {
                    var optionParts = option.Split('=');
                    actionOptions.Add(optionParts[0], optionParts[1]);
                }
                else
                {
                    // it's just a value. Add to 'items'.
                    if (itemsPropValue == null)
                    {
                        itemsPropValue = new JArray();
                        actionOptions.Add("items", itemsPropValue);
                    }

                    itemsPropValue.Add(option);
                }
            }

            return actionOptions;
        }
    }
}
