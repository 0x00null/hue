using Newtonsoft.Json.Linq;
using Serilog;
using ZeroNull.Hue.Api.ClipV2;
using ZeroNull.Hue.StateStorage;

namespace ZeroNull.Hue.HueActions
{

    public class SetSceneOptions
    {
        /// <summary>
        /// The ID or name of the scene to select
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The (optional) duration over which lights should transition to the new scene, in ms
        /// </summary>
        public int? DurationMs { get; set; }
    }

    [HueAction("set-scene", "Recalls a Scene by Name", "recall")]
    public class SetSceneAction : HueActionBase
    {
        public SetSceneAction(IAppStateStore stateStore, ILogger log)
            : base(stateStore, log)
        { }

        protected override async Task OnExecute(HueActionContext context, ResourceIdentifier targetResource)
        {
            var options = context.Options.ToObject<SetSceneOptions>();

            var client = await DemandApiClient();

            var scene = await DemandResource(client, options.Name, ResourceType.scene);

            if (scene == null)
            {
                log.Error("The specified Scene was not found");
                return;
            }

            // Now we have a scene, make it active.
            // We do this by PUTting to the 'recall' element of the scene object. Weird? yes!

            var patch = JObject.FromObject(new
            {
                recall = new
                {
                    action = "active",
                    duration = options.DurationMs.GetValueOrDefault(1000)
                }
            });

            await client.Put(ResourceType.scene, scene.Id, patch);
            $"Applied scene '{options.Name}'".Dump();
        }

    }
}
