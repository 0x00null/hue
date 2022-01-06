using Newtonsoft.Json.Linq;
using Serilog;
using ZeroNull.Hue.Api.ClipV2;
using ZeroNull.Hue.StateStorage;

namespace ZeroNull.Hue.HueActions
{
    [HueAction("on", "Turn the lights on", "turn-on")]
    public class TurnOnAction : HueActionBase
    {
        public TurnOnAction(IAppStateStore stateStore, ILogger log)
            : base(stateStore, log)
        {
        }

        protected override async Task OnExecute(HueActionContext context, ResourceIdentifier targetResource)
        {
            var client = await DemandApiClient();

            // turn on the target
            var patch = new JObject();
            var onPropVal = new JObject();
            onPropVal.Add("on", true);
            patch.Add("on", onPropVal);

            // patch the lights
            var targetServices = await DemandTargetServices(client, targetResource);

            // Is there a group light we can use?
            var targetGroupLight = targetServices.FirstOrDefault(s => s.ResourceType == Api.ClipV2.ResourceType.grouped_light);
            if (targetGroupLight != null)
            {
                // Turn on via the group light
                await client.Put(targetGroupLight.ResourceType, targetGroupLight.Id, patch);
            }
            else
            {
                // Turn on each light individually
                await Task.WhenAll(targetServices.Select(l => client.Put(l.ResourceType, l.Id, patch)));
            }

            // done!
            $"Turned lights On".Dump();
        }
    }
}
