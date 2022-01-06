namespace ZeroNull.Hue.Control
{
    /// <summary>
    /// Defines the different types of event
    /// </summary>
    public enum ControlEventType
    {
        /// <summary>
        /// A value has changed
        /// </summary>
        ScalarChanged = 0,

        /// <summary>
        /// A Button style input was briefly pressed and released
        /// </summary>
        ShortPress = 1,

        /// <summary>
        /// A Button style input was pressed and held for a period
        /// </summary>
        LongPress = 2
    }
}
