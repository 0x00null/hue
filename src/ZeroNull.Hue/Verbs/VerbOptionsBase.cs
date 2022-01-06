using CommandLine;

namespace ZeroNull.Hue.Verbs
{
    /// <summary>
    /// Base class for all verb options
    /// </summary>
    public abstract class VerbOptionsBase
    {
        [Option("debug", HelpText = "Enables detailed debug logging")]
        public bool DebugLogging { get; set; }
    }
}
