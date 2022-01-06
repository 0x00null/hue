using Serilog;

namespace ZeroNull.Hue.Control.Midi
{
    /// <summary>
    /// Creates MIDI Control Event Sources
    /// </summary>
    public class MidiDeviceControlEventSourceFactory : IControlEventSourceFactory
    {
        private readonly ILogger log;
        public MidiDeviceControlEventSourceFactory(ILogger log) { this.log = log; }

        /// <summary>
        /// Starts a new MIDI event source which sends events to the specified sink
        /// </summary>
        /// <param name="targetSink"></param>
        /// <param name="cancelToken"></param>
        /// <returns></returns>
        public Task StartNew(IControlEventSink targetSink, CancellationToken cancelToken)
        {
            var source = new MidiDeviceControlEventSource();
            return source.Start(targetSink, log, cancelToken);
        }
    }
}
