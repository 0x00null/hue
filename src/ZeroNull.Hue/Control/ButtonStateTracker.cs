namespace ZeroNull.Hue.Control
{
    /// <summary>
    /// Tracks the status of a collection of buttons, emitting press and longpress events as needed
    /// Designed for use with the MIDI event source, but will work for any 'raw' style input device
    /// </summary>
    internal class ButtonStateTracker
    {
        // Number of ms to wait befor a held button becomes a 'long press'
        private const int longPressDurationMs = 1000;

        private readonly IControlEventSink sink;
        private readonly Dictionary<string, DateTime> pressedButtons = new Dictionary<string, DateTime>();
        private readonly List<string> longPressedButtons = new List<string>();

        public ButtonStateTracker(IControlEventSink sink)
        {
            this.sink = sink;
        }

        public async Task Start(CancellationToken cancelToken)
        {
            while (cancelToken.IsCancellationRequested == false)
            {
                SendPendingLongPressEvents();
                await Task.Delay(10);
            }

        }

        /// <summary>
        /// Capture that a given input has just been pressed down
        /// </summary>
        /// <param name="id"></param>
        public void Press(string id)
        {
            // Store that the button is pressed, if it isnt already
            if (pressedButtons.ContainsKey(id))
            {
                // duplicate press event!
                return;
            }
            pressedButtons.Add(id, DateTime.Now);
        }

        /// <summary>
        /// Capture that a given input has just been released
        /// </summary>
        /// <param name="id"></param>
        public void Release(string id)
        {
            if (pressedButtons.ContainsKey(id))
            {
                // Button is no longer pressed
                pressedButtons.Remove(id);

                if (longPressedButtons.Contains(id))
                {
                    // We've fired a longpress event for this already - don't refire!
                    longPressedButtons.Remove(id);
                }
                else
                {
                    // We've just released after a short press
                    sink.NotifyEvent(new ControlEvent() { InputId = id, Type = ControlEventType.ShortPress });

                }
            }
        }

        /// <summary>
        /// Sends out any pending long press events
        /// </summary>
        public void SendPendingLongPressEvents()
        {
            foreach (var button in pressedButtons)
            {
                if (longPressedButtons.Contains(button.Key) == false && (DateTime.Now - button.Value).TotalMilliseconds > longPressDurationMs)
                {
                    longPressedButtons.Add(button.Key);

                    // Fire the longpress event
                    sink.NotifyEvent(new ControlEvent() { InputId = button.Key, Type = ControlEventType.LongPress });
                }
            }
        }


    }



    /// <summary>
    /// Keeps track of the 'press' status of an input
    /// </summary>
    internal class ButtonPressData
    {
        public string Id { get; set; }
        public DateTime Pressed { get; set; }
    }

}
