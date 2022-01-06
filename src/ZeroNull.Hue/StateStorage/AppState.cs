using ZeroNull.Hue.Api.ClipV2;
using ZeroNull.Hue.EventRouting;

namespace ZeroNull.Hue.StateStorage
{
    /// <summary>
    /// The app state, as persisted by a storage provider
    /// </summary>
    public class AppState
    {
        /// <summary>
        /// Whether we are sucessfully connected to a Bridge
        /// </summary>
        public bool IsConnectedToBridge { get; set; }

        /// <summary>
        /// The URL of the Bridge
        /// </summary>
        public string BridgeUrl { get; set; }

        /// <summary>
        /// The App Key used to authenticate with the Brudge
        /// </summary>
        public string BridgeAppKey { get; set; }

        /// <summary>
        /// The default control target
        /// </summary>
        public ResourceIdentifier ControlTarget { get; set; }

        /// <summary>
        /// The date we last updated the list of services provided by the default target
        /// </summary>
        public DateTime? LastServicesUpdateUtc { get; set; }

        /// <summary>
        /// A list of services provided by the default target
        /// </summary>
        public IEnumerable<ResourceIdentifier> ControlTargetServices { get; set; }

        /// <summary>
        /// A list of routes used to match control events to actions
        /// </summary>
        public List<EventRoute> Routes { get; set; }

        /// <summary>
        /// A list of Profiles which can be applied to Targets
        /// </summary>
        public List<Profile> Profiles { get; set; }

        public AppState()
        {
            Routes = new List<EventRoute>();
            Profiles = new List<Profile>();
        }

    }
}
