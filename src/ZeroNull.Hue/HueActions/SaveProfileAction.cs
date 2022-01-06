using Serilog;
using ZeroNull.Hue.Api.ClipV2;
using ZeroNull.Hue.StateStorage;

namespace ZeroNull.Hue.HueActions
{
    [HueAction("save", "Adds or Updates a Profile", "update-profile")]
    public class SaveProfileAction : ProfileActionBase
    {
        public SaveProfileAction(IAppStateStore stateStore, ILogger log) : base(stateStore, log)
        {
        }

        protected override async Task OnExecute(HueActionContext context, ResourceIdentifier targetResource, ProfileActionOptions options)
        {
            var state = DemandState();

            // build a new profile


            var client = await DemandApiClient();
            var services = await DemandTargetServices(client, targetResource);

            var lightStates = new List<LightGetModel>();


            // Create a profile item for each light
            foreach (var light in services.Where(s => s.ResourceType == Api.ClipV2.ResourceType.light))
            {
                // Read current light state
                var lightStateResponse = await client.Get<LightGetModel>(ResourceType.light, light.Id);
                if (lightStateResponse.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    continue;
                }
                lightStates.Add(lightStateResponse.Data.Data.FirstOrDefault());
            }

            // is there a profile with this name already?
            var profile = state.Profiles.FirstOrDefault(p => p.Name == options.Name);

            if (profile == null)
            {
                profile = new Profile() { Name = options.Name };
                state.Profiles.Add(profile);
            }

            profile.Lights = lightStates;

            // Store the state
            DemandPutState(state);

            $"Saved profile '{options.Name}'".Dump();
        }
    }
}
