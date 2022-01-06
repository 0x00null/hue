using Serilog;
using ZeroNull.Hue.Api.ClipV2;
using ZeroNull.Hue.StateStorage;

namespace ZeroNull.Hue.HueActions
{
    /// <summary>
    /// AN Action which does something with a Profile. Profile validation is provided.
    /// </summary>
    public abstract class ProfileActionBase : HueActionBase
    {
        public ProfileActionBase(IAppStateStore stateStore, ILogger log) : base(stateStore, log)
        {
        }

        protected override Task OnExecute(HueActionContext context, ResourceIdentifier targetResource)
        {
            var options = context.Options.ToObject<ProfileActionOptions>();

            if (string.IsNullOrEmpty(options.Name))
            {
                options.Name = context.SourceEvent?.InputId;
            }

            if (string.IsNullOrEmpty(options.Name))
            {
                // Missing Name
                log.Error("You must specify the Name of the profile");
                return Task.CompletedTask;
            }

            options.Name = options.Name.ToLower();

            // ok - carry on!
            return OnExecute(context, targetResource, options);
        }

        protected abstract Task OnExecute(HueActionContext context, ResourceIdentifier targetResource, ProfileActionOptions options);
    }
}
