using CommandLine;
using Newtonsoft.Json.Linq;
using Serilog;
using ZeroNull.Hue.HueActions;
using ZeroNull.Hue.StateStorage;

namespace ZeroNull.Hue.Verbs
{
    [Verb("select", HelpText = "Selects the control target")]
    public class SelectOptions : VerbOptionsBase
    {
        [Value(0, Required = true, HelpText = "The Name of the target room or zone to select. 'none' to clear the current target")]
        public string Target { get; set; }
    }

    public class SelectHandler : VerbHandler<SelectOptions>
    {
        private readonly ILogger log;
        private readonly IAppStateStore stateStorage;
        private readonly IHueActionExecutor actionExecutor;
        public SelectHandler(ILogger log, IAppStateStore stateStorage, IHueActionExecutor actionExecutor)
        {
            this.actionExecutor = actionExecutor;
            this.log = log;
            this.stateStorage = stateStorage;
        }
        protected override async Task OnHandleAsync(SelectOptions options, CancellationToken cancelToken)
        {
            if (string.IsNullOrEmpty(options.Target))
            {
                log.Error("You must provide the name of the control target you want to select, or the value 'none' to clear the default");
                return;
            }

            var state = stateStorage.Get();
            var target = options.Target?.ToLower();


            if (target == "none" || target == "clear")
            {
                if (state.ControlTarget == null)
                {
                    log.Error("No default control target is currently selected");
                    return;
                }
                else
                {
                    state.ControlTarget = null;
                    state.LastServicesUpdateUtc = null;
                    state.ControlTargetServices = null;

                    stateStorage.Put(state);
                    $"Default control target cleared".Dump();
                    return;
                }
            }

            // execute the select target action
            await actionExecutor.Execute(SelectTargetAction.ACTION_NAME, JObject.FromObject(options), null);
        }
    }
}