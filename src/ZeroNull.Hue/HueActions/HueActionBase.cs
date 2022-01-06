using RestSharp;
using Serilog;
using ZeroNull.Hue.Api.ClipV2;
using ZeroNull.Hue.StateStorage;

namespace ZeroNull.Hue.HueActions
{
    /// <summary>
    /// Base class for all Hur Actions
    /// </summary>
    public abstract class HueActionBase : IHueAction
    {
        private readonly IAppStateStore stateStore;
        protected readonly ILogger log;
        private AppState cachedState = null;

        public HueActionBase(IAppStateStore stateStore, ILogger log)
        {
            this.log = log;
            this.stateStore = stateStore;
        }

        /// <summary>
        /// Executes the action
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public async Task Execute(HueActionContext context)
        {
            ResourceIdentifier targetResource = null;

            // Do we need to resolve a provided target?
            TargetSpec targetSpec = context.Options.ToObject<TargetSpec>();
            if (targetSpec != null && !string.IsNullOrEmpty(targetSpec.Target))
            {
                // We're going for an explicit target!
                var client = await DemandApiClient();

                // was a type specified?
                if (targetSpec.TargetType.HasValue)
                {
                    targetResource = await DemandResource(client, targetSpec.Target, targetSpec.TargetType.Value);
                }
                else
                {
                    // A type was not specified. Try likely resources in order of priority
                    targetResource = await DemandResource(client, targetSpec.Target, ResourceType.room);

                    if (targetResource == null)
                    {
                        targetResource = await DemandResource(client, targetSpec.Target, ResourceType.zone);
                    }

                    if (targetResource == null)
                    {
                        targetResource = await DemandResource(client, targetSpec.Target, ResourceType.grouped_light);
                    }

                    if (targetResource == null)
                    {
                        targetResource = await DemandResource(client, targetSpec.Target, ResourceType.light);
                    }

                    if (targetResource == null)
                    {
                        targetResource = await DemandResource(client, targetSpec.Target, ResourceType.device);
                    }
                }

                // did we manage to find a resource?
                if (targetResource == null)
                {
                    throw new InvalidOperationException("The Target you specified could not be found. Try also setting 'targetType', or specify the ID rather than the name");
                }

                $"Targetting resource {targetResource}".Dump(color: ConsoleColor.Green);
            }
            else
            {
                // We're using the default target. Is it set?
                var state = DemandState();
                if (state.ControlTarget == null)
                {
                    if (state.ControlTarget == null)
                    {
                        throw new InvalidOperationException("You have not selected a control target. Use 'hue select' or specify a target with the 'target' option, then try again");
                    }
                }
                targetResource = state.ControlTarget;
            }


            try
            {
                await OnExecute(context, targetResource);
            }
            catch (Exception ex)
            {
                log.Error(ex, "An error occurred execiting the action");
            }
        }

        protected abstract Task OnExecute(HueActionContext context, ResourceIdentifier targetResource);

        /// <summary>
        /// Demands App State
        /// </summary>
        /// <returns></returns>
        protected AppState DemandState()
        {
            if (cachedState == null)
            {
                cachedState = stateStore.Get();
            }

            return cachedState;
        }

        /// <summary>
        /// Demands that the provided state is persisted
        /// </summary>
        /// <param name="state"></param>
        protected void DemandPutState(AppState state)
        {
            stateStore.Put(state);
        }

        /// <summary>
        /// Demands an API client. Throws an exception if not connected.
        /// </summary>
        protected Task<ClipV2Client> DemandApiClient()
        {
            var state = stateStore.Get();

            // Check we're connected and have a target selected
            if (state.IsConnectedToBridge == false)
            {
                throw new InvalidOperationException("You are not connected to a Bridge. Use 'hue connect' and try again");
            }

            // Looks good. Let's send off to the Bridge...
            var client = new ClipV2Client(state.BridgeUrl);
            client.SetApplicationAccessKey(state.BridgeAppKey);

            return Task.FromResult(client);
        }

        /// <summary>
        /// Demands that an ID is resolved to a Resource Identifier
        /// </summary>
        /// <param name="id"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        protected async Task<ResourceIdentifier> DemandResource(ClipV2Client client, string idOrName, ResourceType type)
        {
            if (string.IsNullOrEmpty(idOrName))
            {
                throw new InvalidOperationException("You must specify either name or id");
            }

            // does it look like an id?
            bool resolveAsId = false;
            try
            {
                Guid id;
                resolveAsId = Guid.TryParse(idOrName, out id);
            }
            catch
            {
                // ...it's not a guid
            }

            IRestResponse<ApiResponse<GetModel>> getResponse;

            if (resolveAsId)
            {
                getResponse = await client.Get<GetModel>(type, idOrName);
            }
            else
            {
                getResponse = await client.Get<GetModel>(type);
            }

            var results = getResponse.Data?.Data ?? Enumerable.Empty<GetModel>();

            // narrow it down
            GetModel finalResult = null;

            if (resolveAsId)
            {
                finalResult = results.FirstOrDefault(r => r.Id == idOrName);
            }
            else
            {
                finalResult = results.FirstOrDefault(r => r.Metadata?.Name?.ToLower() == idOrName.ToLower());
            }

            if (finalResult == null)
            {
                return null;
            }
            else
            {
                return (ResourceIdentifier)finalResult;
            }
        }

        /// <summary>
        /// Demamds the target light and grouped_light resource IDs, refreshing if needed
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        protected async Task<IEnumerable<ResourceIdentifier>> DemandTargetServices(ClipV2Client client, ResourceIdentifier resource)
        {
            var state = DemandState();

            // Is this the default target we're trying to resolve services for?
            if (state.ControlTarget != null && resource.Id == state.ControlTarget.Id)
            {
                // Do we need to refresh the list of services available under the default control target?
                if (state.LastServicesUpdateUtc.HasValue == false
                    || (DateTime.UtcNow - state.LastServicesUpdateUtc.Value).TotalSeconds > 30)
                {
                    var controlTargetResponse = await client.Get<GetModel>(state.ControlTarget.ResourceType, state.ControlTarget.Id);
                    var controlTarget = controlTargetResponse.Data.Data.FirstOrDefault();
                    var targetLights = controlTarget.Services.Where(s => s.ResourceType == ResourceType.light || s.ResourceType == ResourceType.grouped_light);
                    state.ControlTargetServices = targetLights;
                    state.LastServicesUpdateUtc = DateTime.UtcNow;
                    stateStore.Put(state);
                    log.Debug("Updated target services data");
                }

                return state.ControlTargetServices;
            }
            else
            {
                // Resolve directly - we're not caching the result
                var controlTargetResponse = await client.Get<GetModel>(resource.ResourceType, resource.Id);
                var controlTarget = controlTargetResponse.Data.Data.FirstOrDefault();
                if (controlTarget.Type == ResourceType.light || controlTarget.Type == ResourceType.grouped_light)
                {
                    // we've directly resolved a light or group light. Just use it.
                    return new ResourceIdentifier[] { controlTarget };
                }
                else
                {
                    // return services linked to the target
                    return controlTarget.Services?.Where(s => s.ResourceType == ResourceType.light || s.ResourceType == ResourceType.grouped_light) ?? Enumerable.Empty<ResourceIdentifier>();

                }
            }
        }
    }
}
