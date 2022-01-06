using ZeroNull.Hue.Control;

namespace ZeroNull.Hue.EventRouting
{
    /// <summary>
    /// Routes incoming events to target Hue Action(s) in a fire-and-forget manner
    /// (it's kinda a bus, really...)
    /// </summary>
    public interface IEventRouter
    {
        /// <summary>
        /// Starts the action bus. Stop the bus by cancelling the token.
        /// </summary>
        /// <param name="cancelToken"></param>
        Task Start(CancellationToken cancelToken);

        /// <summary>
        /// Accepts an event and routes to an appropiate action
        /// </summary>
        /// <param name="ev">The event to service</param>
        void Route(ControlEvent ev);


    }
}
