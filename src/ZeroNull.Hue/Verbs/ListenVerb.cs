using CommandLine;
using Serilog;
using ZeroNull.Hue.Control;
using ZeroNull.Hue.EventRouting;
using ZeroNull.Hue.StateStorage;

namespace ZeroNull.Hue.Verbs
{
    [Verb("listen", HelpText = "Listens for mapped inputs and sends commands to the connected Bridge")]
    public class ListenOptions : VerbOptionsBase
    {

    }

    public class ListenHandler : VerbHandler<ListenOptions>
    {
        private readonly IEnumerable<IControlEventSourceFactory> eventSourceFactories;
        private readonly ILogger log;
        private readonly IAppStateStore stateStorage;
        private readonly IEventRouter router;
        private readonly IControlEventSink sink;



        public ListenHandler(ILogger log, IEnumerable<IControlEventSourceFactory> eventSourceFactories, IAppStateStore stateStorage, IEventRouter router, IControlEventSink sink)
        {
            this.sink = sink;
            this.router = router;
            this.stateStorage = stateStorage;
            this.log = log;
            this.eventSourceFactories = eventSourceFactories;
        }
        protected override async Task OnHandleAsync(ListenOptions options, CancellationToken cancelToken)
        {
            var state = stateStorage.Get();

            // Check we're connected and have a target selected
            if (state.IsConnectedToBridge == false)
            {
                log.Error("You are not connected to a Bridge. Use 'hue connect' and try again");
                return;
            }

            if (state.ControlTarget == null)
            {
                log.Error("You have not selected a control target. Use 'hue status' and 'hue select', then try again");
                return;
            }


            var sinkTask = sink.Start(cancelToken);
            var sourceTasks = new List<Task>();
            foreach (var factory in eventSourceFactories)
            {
                sourceTasks.Add(factory.StartNew(sink, cancelToken));
            }

            var routerTask = router.Start(cancelToken);

            "Listening for events".DumpHeading();
            "Use any of your mapped inputs to execute actions".Dump();


            while (!cancelToken.IsCancellationRequested)
            {
                // Fetch the next event
                var ev = await sink.WaitForInput(500);
                if (ev != null)
                {
                    router.Route(ev);
                }
            }



            /*
            await Task.WhenAll(
                sinkTask,
                Task.WhenAll(sourceTasks),
                routerTask
            );*/

        }
    }
}
