namespace ZeroNull.Hue.Verbs
{

    /// <summary>
    /// Weakly types verb handler
    /// </summary>
    public interface IVerbHandler
    {
        Task HandleAsync(object options, CancellationToken cancelToken);
    }

    /// <summary>
    /// Strongly typed verb handler
    /// </summary>
    /// <typeparam name="TOptions"></typeparam>
    public interface IVerbHandler<TOptions>
    {
        Task HandleAsync(TOptions options, CancellationToken cancelToken);
    }

    /// <summary>
    /// Base class for all verb handlers
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class VerbHandler<T> : IVerbHandler<T>, IVerbHandler
        where T : VerbOptionsBase
    {
        public Task HandleAsync(T options, CancellationToken cancelToken)
        {
            return OnHandleAsync(options, cancelToken);
        }

        Task IVerbHandler.HandleAsync(object options, CancellationToken cancelToken)
        {
            return HandleAsync((T)options, cancelToken);
        }

        protected abstract Task OnHandleAsync(T options, CancellationToken cancelToken);
    }
}
