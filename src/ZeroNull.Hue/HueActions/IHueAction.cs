namespace ZeroNull.Hue.HueActions
{
    /// <summary>
    /// A Hue action
    /// </summary>
    public interface IHueAction
    {
        /// <summary>
        /// Executes the action
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        Task Execute(HueActionContext context);
    }
}
