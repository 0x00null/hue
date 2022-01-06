using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Multimedia;
using Serilog;

namespace ZeroNull.Hue.Control.Midi
{
    /// <summary>
    /// Uses DryWetMidi to listen for incoming midi device messages, and turns them into Control Events
    /// </summary>
    public class MidiDeviceControlEventSource
    {
        private ILogger log;
        private IControlEventSink sink = null;
        private ButtonStateTracker buttons = null;

        /// <summary>
        /// Starts this Source. Fire the cancellation token to stop.
        /// </summary>
        /// <param name="sink"></param>
        /// <param name="log"></param>
        /// <param name="cancelToken"></param>
        /// <returns></returns>
        public Task Start(IControlEventSink sink, ILogger log, CancellationToken cancelToken)
        {
            this.log = log;
            this.sink = sink;

            log.Debug($"{typeof(MidiDeviceControlEventSource).Name}: Enumerating MIDI devices:");
            var allDevices = InputDevice.GetAll();
            foreach (var device in allDevices)
            {
                device.EventReceived += Device_EventReceived;
                device.StartEventsListening();
                log.Debug($"{typeof(MidiDeviceControlEventSource).Name}: Listening to events from '{device.Name}'");
            }

            // Start the button tracker
            buttons = new ButtonStateTracker(sink);
            var buttonTask = buttons.Start(cancelToken);
            log.Debug($"{typeof(MidiDeviceControlEventSource).Name}: Button tracker started");




            var workertask = Task.WhenAll(buttonTask, cancelToken.WaitForCancel())
                .ContinueWith(t =>
                {
                    log.Debug($"{typeof(MidiDeviceControlEventSource).Name}: Stop requested");
                    foreach (var device in allDevices)
                    {
                        device.StopEventsListening();
                        device.EventReceived -= Device_EventReceived;
                        log.Debug($"{typeof(MidiDeviceControlEventSource).Name}: Stopped listening to '{device.Name}'");
                    }
                    log.Debug($"{typeof(MidiDeviceControlEventSource).Name}: Stopped");
                });

            return workertask;
        }

        /// <summary>
        /// Handles a midi input event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Device_EventReceived(object? sender, MidiEventReceivedEventArgs e)
        {
            if (e.Event is NoteOnEvent onEvent)
            {
                var inputId = $"button-{onEvent.NoteNumber}";
                var value = (byte)(onEvent.Velocity * 2);

                // if the value (veolcity) is 0, we've lifted off the input
                if (value == 0)
                {
                    log.Debug($"{typeof(MidiDeviceControlEventSource).Name}: MIDI {inputId} Released ({value:X})");
                    buttons.Release(inputId);
                }
                else
                {
                    log.Debug($"{typeof(MidiDeviceControlEventSource).Name}: MIDI {inputId} Pressed ({value:X})");
                    buttons.Press(inputId);
                }
            }
            else if (e.Event is NoteOffEvent offEvent)
            {
                var inputId = $"button-{offEvent.NoteNumber}";
                var value = (byte)(offEvent.Velocity * 2);

                log.Debug($"{typeof(MidiDeviceControlEventSource).Name} : MIDI {inputId} Released ({value:X})");
                buttons.Release(inputId);
            }
            else if (e.Event is ControlChangeEvent cce)
            {
                // it's a scalar event
                var inputId = $"scalar-{cce.ControlNumber}";
                var value = (byte)(cce.ControlValue * 2);
                log.Debug($"{typeof(MidiDeviceControlEventSource).Name}: MIDI {inputId} Scalar ({value:X})");
                sink.NotifyEvent(new ControlEvent() { InputId = inputId, Type = ControlEventType.ScalarChanged, Value = value });
            }
        }
    }
}