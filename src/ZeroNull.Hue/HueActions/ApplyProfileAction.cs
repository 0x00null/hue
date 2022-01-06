using Serilog;
using ZeroNull.Hue.Api.ClipV2;
using ZeroNull.Hue.StateStorage;

namespace ZeroNull.Hue.HueActions
{
    [HueAction("apply", "Applies a saved Profile", "apply-profile")]
    public class ApplyProfileAction : ProfileActionBase
    {
        public ApplyProfileAction(IAppStateStore stateStore, ILogger log) : base(stateStore, log)
        {
        }

        protected override async Task OnExecute(HueActionContext context, ResourceIdentifier targetResource, ProfileActionOptions options)
        {
            var state = DemandState();

            // Does this profile exist?
            var profile = state.Profiles.FirstOrDefault(p => p.Name == options.Name);

            if (profile == null)
            {
                log.Error($"No profile was found with the name '{options.Name}'");
                return;
            }


            var client = await DemandApiClient();

            // Go apply all the light states
            await Task.WhenAll(profile.Lights.Select(l => client.Put(ResourceType.light, l.Id, l.AsPatch())));

            $"Applied profile '{options.Name}'".Dump();
        }
    }
}
