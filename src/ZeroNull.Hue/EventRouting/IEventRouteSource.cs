namespace ZeroNull.Hue.EventRouting
{
    /// <summary>
    /// A source of event routes
    /// </summary>
    public interface IEventRouteSource
    {
        /// <summary>
        /// Gets routes registered with this source
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<EventRoute>> GetRoutes();
    }
}
