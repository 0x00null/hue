namespace ZeroNull.Hue.Control
{
    /// <summary>
    /// Factory interface for Control Event Sources
    /// </summary>
    public interface IControlEventSourceFactory
    {
        /// <summary>
        /// Starts a new ControlEvent Source
        /// </summary>
        /// <param name="cancelToken"></param>
        /// <returns></returns>
        Task StartNew(IControlEventSink targetSink, CancellationToken cancelToken);
    }
}
