using CommandLine;
using Serilog;
using ZeroNull.Hue.StateStorage;

namespace ZeroNull.Hue.Verbs
{
    [Verb("disconnect")]
    public class DisconnectOptions : VerbOptionsBase
    {
    }

    public class DisconnectHandler : VerbHandler<DisconnectOptions>
    {
        private readonly ILogger log;
        private readonly IAppStateStore stateStore;
        public DisconnectHandler(ILogger log, IAppStateStore stateStore)
        {
            this.stateStore = stateStore;
            this.log = log;
        }
        protected override Task OnHandleAsync(DisconnectOptions options, CancellationToken cancelToken)
        {
            var state = stateStore.Get();
            if (state.IsConnectedToBridge == false)
            {
                log.Error("You are not currently connected to a Bridge. Use 'hue connect <url>' to connect to a Bridge.");
            }
            else
            {
                state.BridgeUrl = string.Empty;
                state.BridgeAppKey = String.Empty;

                stateStore.Put(state);
                log.Information("Dosconnected from Bridge. Reconnect with 'hue connect <url>'");
            }
            return Task.CompletedTask;

        }
    }

}
