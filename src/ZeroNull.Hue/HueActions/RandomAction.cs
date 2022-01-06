using Serilog;
using ZeroNull.Hue.Api.ClipV2;
using ZeroNull.Hue.StateStorage;

namespace ZeroNull.Hue.HueActions
{
    [HueAction("random", "Assigns a random colour to each light")]
    public class RandomAction : HueActionBase
    {
        private static readonly Random random;

        static RandomAction()
        {
            random = new Random(Guid.NewGuid().GetHashCode());
        }
        public RandomAction(IAppStateStore stateStore, ILogger log)
            : base(stateStore, log)
        { }

        protected override async Task OnExecute(HueActionContext context, ResourceIdentifier targetResource)
        {
            var client = await DemandApiClient();
            var targets = await DemandTargetServices(client, targetResource);

            // Create a patch for each light

            foreach (var light in targets.Where(t => t.ResourceType == Api.ClipV2.ResourceType.light))
            {
                var patch = new LightGetModel()
                {
                    Color = new ColorModel()
                    {
                        XY = new CieXyColorModel()
                        {
                            X = (decimal)random.NextDouble(),
                            Y = (decimal)random.NextDouble()
                        }
                    },
                    Dimming = new DimmingModel()
                    {
                        Brightness = 100
                    }
                }.AsPatch();

                await client.Put(ResourceType.light, light.Id, patch);
            }

            $"Random colours assigned".Dump();

        }
    }
}
