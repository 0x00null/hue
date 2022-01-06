using CommandLine;
using Serilog;
using ZeroNull.Hue.Api.ClipV2;
using ZeroNull.Hue.StateStorage;

namespace ZeroNull.Hue.Verbs
{

    [Verb("connect", HelpText = "Connects to a Hue Bridge")]
    public class ConnectOptions : VerbOptionsBase
    {
        [Value(0, MetaName = "url", Required = true, HelpText = "URL of the Hue Bridge")]
        public string Url { get; set; }

        [Value(1, MetaName = "key", Required = false, HelpText = "Your existing Application Key, if you've got one for the Bridge you're connecting to")]
        public string Key { get; set; }
    }


    public class ConnectHandler : VerbHandler<ConnectOptions>
    {
        private readonly ILogger log;
        private readonly IAppStateStore stateStore;
        public ConnectHandler(ILogger log, IAppStateStore stateStore)
        {
            this.stateStore = stateStore;
            this.log = log;
        }
        protected override async Task OnHandleAsync(ConnectOptions options, CancellationToken cancelToken)
        {
            var state = stateStore.Get();
            if (state.IsConnectedToBridge)
            {
                log.Error("You are already connected to a Hue Bridge. Disconnect first (with 'hue disconnect', then try again.");
                return;
            }

            var client = new ClipV2Client(options.Url);


            // Did we provide an existing key?
            if (!string.IsNullOrEmpty(options.Key))
            {
                // Give the existing key a whirl
                try
                {
                    client.SetApplicationAccessKey(options.Key);
                    // try to retrieve devices
                    var devicesResponse = await client.Get(ResourceType.device);
                    if (devicesResponse.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        // seems ok!
                        state.IsConnectedToBridge = true;
                        state.BridgeAppKey = options.Key;
                        state.BridgeUrl = options.Url;
                        stateStore.Put(state);

                        log.Information("Connected to Bridge using the Key you provided");
                    }
                    else
                    {
                        log.Warning("Could not connect using the key you provided. Either check the key, or pair with the Bridge by using 'hue connect' after pressing the Link button");
                    }
                }
                catch (Exception ex)
                {
                    log.Error(ex.Message);
                }

                return;
            }

            // No key provided, pair using the link button
            try
            {
                var response = (await client.GetApplicationAccessKey()).Data.FirstOrDefault();
                if (response.Error != null)
                {
                    if (response.Error.Description.Contains("link button"))
                    {
                        log.Warning("Press the Link button on the top of your Bridge, then try again");
                    }
                    else
                    {
                        log.Error(response.Error.Description);
                    }
                    return;
                }
            }
            catch (Exception ex)
            {
                // ermm...
                log.Error(ex.Message);
            }
        }
    }
}
