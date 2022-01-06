namespace ZeroNull.Hue.Control
{
    /// <summary>
    /// A sink for control events
    /// </summary>
    public interface IControlEventSink
    {
        /// <summary>
        /// Starts the Sink. Set the cancellation token to stop.
        /// </summary>
        /// <param name="cancelToken"></param>
        /// <returns></returns>
        Task Start(CancellationToken cancelToken);

        /// <summary>
        /// Notifies the Sink that there is a new event availabke
        /// </summary>
        /// <param name="e"></param>
        void NotifyEvent(ControlEvent e);

        /// <summary>
        /// Returns a task which completes with an event when one becomes available, or null if the timeoutms is reached.
        /// </summary>
        /// <param name="timeoutMs">Number of ms to wait for an event before returning null. Set to 0 to wait forever.</param>
        /// <returns></returns>
        Task<ControlEvent> WaitForInput(int timeoutMs = 0);
    }
}
