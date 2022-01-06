using CommandLine;
using Serilog;
using ZeroNull.Hue.Control;
using ZeroNull.Hue.EventRouting;
using ZeroNull.Hue.HueActions;
using ZeroNull.Hue.StateStorage;

namespace ZeroNull.Hue.Verbs
{
    [Verb("map", HelpText = "Maps an input to an action by creating a new route")]
    public class MapOptions : VerbOptionsBase
    {
        [Value(0, Required = true, HelpText = "Target Action")]
        public string Action { get; set; }

        [CommandLine.Value(1, Required = false, HelpText = "Options to pass to the action")]
        public IEnumerable<string> Options { get; set; }
    }

    public class MapHandler : VerbHandler<MapOptions>
    {
        private readonly IEnumerable<IControlEventSourceFactory> controlEventSourceFactories;
        private readonly ILogger log;
        private readonly IAppStateStore stateStorage;
        private readonly IEnumerable<HueActionAttribute> actions;
        private readonly IControlEventSink sink;


        public MapHandler(ILogger log, IEnumerable<IControlEventSourceFactory> controlEventSourceFactories, IAppStateStore stateStorage, IControlEventSink sink, IEnumerable<HueActionAttribute> actions)
        {
            this.sink = sink;
            this.actions = actions;
            this.stateStorage = stateStorage;
            this.log = log;
            this.controlEventSourceFactories = controlEventSourceFactories;
        }

        protected override async Task OnHandleAsync(MapOptions options, CancellationToken cancelToken)
        {
            options.Action = options.Action.ToLower();
            HueActionAttribute matchedAction = null;

            // Is the magic 'clear' action?
            if (options.Action == "clear")
            {
                // We're clearing all routes for a given input
                "Warning: You are about to clear all routes associated with an input. For more granular control, use the 'hue route' commands".Dump(color: ConsoleColor.Yellow);
            }
            else
            {
                // We're creating a new route
                matchedAction = actions.FirstOrDefault(a => a.Id == options.Action || a.Aliases.Contains(options.Action));
                if (matchedAction == null)
                {
                    $"Action '{options.Action}' is not a valid action".Dump(color: ConsoleColor.Red);
                    return;
                }

                // looks ok!
                $"Map action: {options.Action}".DumpHeading();

            }


            "Press a button on your controller...".Dump();


            // Create a sink to capture the input we're looking for
            var cst = new CancellationTokenSource();

            var sinkTask = sink.Start(cst.Token);
            var sourceTasks = new List<Task>();
            foreach (var factory in controlEventSourceFactories)
            {
                sourceTasks.Add(factory.StartNew(sink, cst.Token));
            }

            // Wait for the sink to collect a message
            var ev = await sink.WaitForInput();

            // Now we have an input, stop the source tasks
            cst.Cancel();

            // Wait for the sink and source tasks to complete
            await Task.WhenAll(
                sinkTask,
                Task.WhenAll(sourceTasks)
            );

            var state = stateStorage.Get();

            // Are we clearing routes?
            if (options.Action == "clear")
            {
                // Clear all routes related to the specified input
                var removedCount = state.Routes.RemoveAll(r => r.InputId == ev.InputId);
                if (removedCount == 0)
                {
                    "No matching routes were found".Dump();
                }
                else
                {
                    $"Removed {removedCount} routes".Dump();
                }
            }
            else
            {
                // create a route in AppState
                $"Creating new route...".Dump();
                var route = new EventRoute()
                {
                    InputId = ev.InputId,
                    EventType = ev.Type,
                    TargetAction = matchedAction.Id,
                    Options = options.Options.ToJObject()
                };

                // is the target action a 'scalar style' action?
                // if not, and we've mapped a scalar, set the threshold to 0x0F
                if (matchedAction.IsScalarStyle == false && ev.Type == ControlEventType.ScalarChanged)
                {
                    route.TriggerAbove = 0x0F;
                }

                state.Routes.Add(route);
                $"Route added".Dump();

                // Is there more than one route with this same selector?
                if (state.Routes.Count(r =>
                    r.InputId == route.InputId
                    && r.EventType == route.EventType
                    && r.TriggerAbove == route.TriggerAbove
                    && r.TriggerBelow == route.TriggerBelow)
                    > 1)
                {
                    $"Warning: There is more than one route registered for this mapping. Run 'hue map clear' to clear routes for a given input, or 'hue route' commands for more options".Dump(color: ConsoleColor.Yellow);
                }
            }

            stateStorage.Put(state);
        }
    }
}
