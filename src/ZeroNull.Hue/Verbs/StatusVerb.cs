using CommandLine;
using Serilog;
using ZeroNull.Hue.Api.ClipV2;
using ZeroNull.Hue.StateStorage;

namespace ZeroNull.Hue.Verbs
{
    [Verb("status", HelpText = "Shows status")]
    public class StatusOptions : VerbOptionsBase
    {

    }

    public class StatusHandler : VerbHandler<StatusOptions>
    {
        private readonly ILogger log;
        private readonly IAppStateStore stateStorage;
        public StatusHandler(ILogger log, IAppStateStore stateStorage)
        {
            this.log = log;
            this.stateStorage = stateStorage;
        }
        protected override async Task OnHandleAsync(StatusOptions options, CancellationToken cancelToken)
        {
            var state = stateStorage.Get();

            #region Bridge Connection

            if (state.IsConnectedToBridge == false)
            {
                "Bridge: Not Connected".DumpHeading(ConsoleColor.Red);
                "Connect to your Hue Bridge with 'hue connect'".Dump();
                return;

            }
            else
            {
                "Bridge: Connected".DumpHeading();
                state.BridgeUrl.DumpValue("Bridge URL");
            }
            #endregion

            #region Control Target

            // Are we connected to a control target?
            if (state.ControlTarget == null)
            {
                "Default Control Target: Not Selected".DumpHeading(ConsoleColor.Red);
                "You have not selected a defualt control target. To do so, run 'hue select <target>' with one of the following options:".Dump();
                var client = new ClipV2Client(state.BridgeUrl);
                client.SetApplicationAccessKey(state.BridgeAppKey);

                // list rooms
                var roomsResponse = await client.Get<GetModel>(ResourceType.room);
                if (roomsResponse.Data.Data.Any())
                {
                    "Available Rooms:".Dump();
                    foreach (var room in roomsResponse.Data.Data)
                    {
                        room.Metadata.Name.DumpValue(indentLevel: 1, color: ConsoleColor.Gray);
                    }
                }
                else
                {
                    "No Rooms found".Dump();
                }
                Console.WriteLine();

                // list zones
                var zonesResponse = await client.Get<GetModel>(ResourceType.zone);
                if (zonesResponse.Data.Data.Any())
                {
                    "Available Zones:".Dump();
                    foreach (var zone in zonesResponse.Data.Data)
                    {
                        zone.Metadata.Name.DumpValue(indentLevel: 1, color: ConsoleColor.Gray);
                    }
                }
                else
                {
                    "No Zones found".Dump();
                }
            }
            else
            {
                // Target is selected
                "Default Control Target: Selected".DumpHeading();

                state.ControlTarget.Name.DumpValue("Target");
                state.ControlTarget.ResourceType.DumpValue("Type");
                if (state.LastServicesUpdateUtc.HasValue)
                {
                    state.LastServicesUpdateUtc.DumpValue("Last data refresh");
                }
                else
                {
                    "Never".DumpValue("Last data refresh");
                }

                "To choose a different default control target, run 'hue select none'".Dump();
                "To run an individual action against a different control target, add 'target=<target>' as an option".Dump();
            }

            #endregion

        }
    }
}
