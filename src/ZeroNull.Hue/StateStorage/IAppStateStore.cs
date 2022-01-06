namespace ZeroNull.Hue.StateStorage
{
    /// <summary>
    /// Persists App State to storage
    /// </summary>
    public interface IAppStateStore
    {
        /// <summary>
        /// Retrieves a fresh copy of the App State
        /// </summary>
        /// <returns></returns>
        AppState Get();

        /// <summary>
        /// Stores the App State
        /// </summary>
        /// <param name="state"></param>
        void Put(AppState state);

        /// <summary>
        /// Clears the App State
        /// </summary>
        /// <returns></returns>
        AppState Clear();
    }
}
