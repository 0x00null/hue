using ZeroNull.Hue.Api.ClipV2;

namespace ZeroNull.Hue.StateStorage
{
    /// <summary>
    /// A Profile which stores the state of one or more lights, for later patching against a control target
    /// </summary>
    public class Profile
    {
        /// <summary>
        /// The Name of the Profile
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The states of all lights when the profile was created
        /// </summary>
        public IEnumerable<LightGetModel> Lights { get; set; }

        public Profile()
        {
            Lights = Enumerable.Empty<LightGetModel>();
        }
    }
}
