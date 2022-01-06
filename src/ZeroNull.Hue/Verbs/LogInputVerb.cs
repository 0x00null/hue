using CommandLine;
using Serilog;
using ZeroNull.Hue.Control;

namespace ZeroNull.Hue.Verbs
{
    [Verb("log-input", HelpText = "Logs input from control event sources so you can see what is going on")]
    public class LogInputOptions : VerbOptionsBase
    {
    }

    public class LogInputHandler : VerbHandler<LogInputOptions>
    {
        private readonly ILogger log;
        private readonly IEnumerable<IControlEventSourceFactory> controlEventSourceFactories;
        private readonly IControlEventSink sink;

        public LogInputHandler(ILogger log, IEnumerable<IControlEventSourceFactory> controlEventSourceFactories, IControlEventSink sink)
        {
            this.sink = sink;
            this.controlEventSourceFactories = controlEventSourceFactories;
            this.log = log;
        }
        protected override async Task OnHandleAsync(LogInputOptions options, CancellationToken cancelToken)
        {
            var sinkTask = sink.Start(cancelToken);

            var sourceTasks = new List<Task>();

            foreach (var factory in controlEventSourceFactories)
            {
                log.Information($"Starting source {factory.GetType().Name}...");
                sourceTasks.Add(factory.StartNew(sink, cancelToken));
            }

            "Listening for control events".DumpHeading();
            "Press some buttons to see the output here.".Dump();
            "No events? Check that your MIDI or other input device works in other apps".Dump();
            "Press CTRL+C to stop".Dump();

            while (cancelToken.IsCancellationRequested == false)
            {
                var ev = await sink.WaitForInput(50);
                if (ev != null)
                {
                    log.Information(ev.ToString());
                }
            }

            await Task.WhenAll(
                sinkTask,
                Task.WhenAll(sourceTasks)
            );

            log.Information("Sources stopped");
        }
    }
}
