using Newtonsoft.Json.Linq;
using Serilog;
using ZeroNull.Hue.Api.ClipV2;
using ZeroNull.Hue.StateStorage;

namespace ZeroNull.Hue.HueActions
{
    public class DimActionOptions
    {
        /// <summary>
        /// The target level of the light(s), in percent
        /// </summary>
        public int? Level { get; set; }
    }

    [HueAction("dim", "Set the brightness level of the lights (0-100)", true, "set-brightness")]
    public class DimAction : HueActionBase
    {
        public DimAction(IAppStateStore stateStore, ILogger log)
            : base(stateStore, log)
        {
        }

        protected override async Task OnExecute(HueActionContext context, ResourceIdentifier targetResource)
        {
            // Was a value provided?
            var options = context.Options.ToObject<DimActionOptions>();
            if (options.Level.HasValue)
            {
                if (options.Level > 100)
                {
                    options.Level = 100;
                }

                if (options.Level < 0)
                {
                    options.Level = 0;
                }
            }
            else if (context.SourceEvent != null && context.SourceEvent.Type == Control.ControlEventType.ScalarChanged)
            {
                options.Level = (int)(((float)context.SourceEvent.Value / 255f) * 100f); // Fetch the value from the scalar event
            }
            else
            {
                options.Level = 10; // Default to 10%
            }

            var client = await DemandApiClient();

            // turn on the target
            var patch = new JObject();
            var onPropVal = new JObject();
            onPropVal.Add("on", true);
            patch.Add("on", onPropVal);

            var dimmingPropValue = new JObject();
            dimmingPropValue.Add("brightness", options.Level.Value);
            patch.Add("dimming", dimmingPropValue);

            // patch the lights individually (can't use the group light...which is annoying!)
            var targetServices = await DemandTargetServices(client, targetResource);

            await Task.WhenAll(
                targetServices
                .Where(s => s.ResourceType == Api.ClipV2.ResourceType.light)
                .Select(l => client.Put(l.ResourceType, l.Id, patch))
            );

            // done!
            $"Set brightness to {options.Level.Value}".Dump();
        }
    }
}
