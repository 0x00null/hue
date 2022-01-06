namespace ZeroNull.Hue.HueActions
{
    /// <summary>
    /// Marks a HueAction class as being available for execution from a route or the 'action' verb
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class HueActionAttribute : Attribute
    {
        /// <summary>
        /// The ID of the action
        /// </summary>
        public string Id { get; }
        /// <summary>
        /// A human-readable description of the action
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// Aliases that can be used to execute this action
        /// </summary>
        public string[] Aliases { get; }

        /// <summary>
        /// Whether this is a 'scalar' style action - ie. an action which uses the 'value' property of events.
        /// If a 'non scalar style' action is mapped to a scalar input via the 'map' verb, a minvalue constraint of 25% is added to the resulting route
        /// </summary>
        public bool IsScalarStyle { get; }


        public HueActionAttribute(string id, string description, params string[] aliases)
            : this(id, description, false, aliases)
        { }

        public HueActionAttribute(string id, string description, bool isScalarStyle, params string[] aliases)
        {
            Description = description;
            Aliases = aliases;
            Id = id;
            IsScalarStyle = isScalarStyle;
        }
    }
}
