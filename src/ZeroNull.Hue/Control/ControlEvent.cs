namespace ZeroNull.Hue.Control
{
    /// <summary>
    /// An event receievd from an input device used to drive actions via routes
    /// </summary>
    public class ControlEvent
    {
        /// <summary>
        /// The ID of the input this event was emitted from
        /// </summary>
        public string InputId { get; set; }

        /// <summary>
        /// The Type of this event
        /// </summary>
        public ControlEventType Type { get; set; }

        /// <summary>
        /// The Value associated with this event
        /// </summary>
        public byte Value { get; set; }

        public override string ToString()
        {
            return $"Event: {Type}, Input: {InputId}, Value: {Value:X}";
        }
    }
}
