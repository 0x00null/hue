using Serilog;
using ZeroNull.Hue.Api.ClipV2;
using ZeroNull.Hue.StateStorage;

namespace ZeroNull.Hue.HueActions
{

    [HueAction(ACTION_NAME, "Select the default target for subsequent actions")]
    public class SelectTargetAction : HueActionBase
    {
        public const string ACTION_NAME = "select";

        public SelectTargetAction(IAppStateStore stateStore, ILogger log) : base(stateStore, log)
        {

        }

        protected override Task OnExecute(HueActionContext context, ResourceIdentifier targetResource)
        {
            var state = DemandState();
            state.ControlTarget = targetResource;
            DemandPutState(state);

            $"Control Target set to {targetResource.ToString()}".Dump();

            return Task.CompletedTask;

        }
    }
}
