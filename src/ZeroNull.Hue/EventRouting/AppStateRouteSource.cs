using ZeroNull.Hue.StateStorage;

namespace ZeroNull.Hue.EventRouting
{
    /// <summary>
    /// Sources routes from the app state store
    /// </summary>
    public class AppStateRouteSource : IEventRouteSource
    {
        private readonly IAppStateStore stateStore;
        public AppStateRouteSource(IAppStateStore stateStore)
        {
            this.stateStore = stateStore;
        }
        public Task<IEnumerable<EventRoute>> GetRoutes()
        {
            var state = stateStore.Get();

            return Task.FromResult<IEnumerable<EventRoute>>(state.Routes);
        }
    }
}
