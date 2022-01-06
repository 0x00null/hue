namespace ZeroNull.Hue
{
    /// <summary>
    /// Provides extensions methods to async wait for a cancellation token
    /// </summary>
    public static class CancellationTokenExtensions
    {
        /// <summary>
        /// Returns a task which completes when the cancellation token is set
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public static Task WaitForCancel(this CancellationToken token)
        {
            var completionSource = new TaskCompletionSource();
            token.Register(() => completionSource.SetResult());
            return completionSource.Task;
        }
    }
}
