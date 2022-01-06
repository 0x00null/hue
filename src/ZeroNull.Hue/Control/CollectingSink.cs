using Serilog;
using System.Diagnostics;

namespace ZeroNull.Hue.Control
{
    /// <summary>
    /// Collects Control Events and provides an async method of processing them
    /// </summary>
    public class CollectingSink : IControlEventSink
    {
        private readonly ILogger log;
        private readonly object syncLock = new object();
        private bool isStarted = false;
        private readonly int eventCooloffPeriodMs = 100; // cooloff period during which we discard duplicates
        private readonly int pendingEventPumpFrequencyMs = 50; // how frequently we pump pending messages

        private readonly Queue<ControlEvent> eventQueue = new Queue<ControlEvent>();

        private TaskCompletionSource<ControlEvent> pendingCollectionTaskCompletion = null;
        private DateTime pendingCollectionStarted = DateTime.MinValue;
        private int pendingCollectionTimeoutMs = -1;


        private DateTime lastEventTimestamp = DateTime.MinValue;
        private ControlEvent lastEvent = null;

        private DateTime pendingEventTimestamp = DateTime.MinValue;
        private ControlEvent pendingEvent = null;



        public CollectingSink(ILogger log)
        {
            this.log = log;
        }

        public void NotifyEvent(ControlEvent e)
        {
            lock (syncLock)
            {
                // Is the a new event of the same type for the same input as the last sent event?
                if (lastEvent != null && e.InputId == lastEvent.InputId && e.Type == lastEvent.Type)
                {
                    // Is this also within the cooling off period?
                    if ((DateTime.Now - lastEventTimestamp).TotalMilliseconds < eventCooloffPeriodMs)
                    {
                        // Instead of sending this event, we should set this as the pending event
                        pendingEvent = e;
                        pendingEventTimestamp = DateTime.Now;

                        log.Debug($"{typeof(CollectingSink).Name}: Pending: {e.InputId}");
                        return; // don't send that event!
                    }
                }
                // should we send the pending event?
                if (pendingEvent != null)
                {
                    // An event is pending. Do we queue it?
                    if (e.InputId != pendingEvent.InputId || e.Type != pendingEvent.Type)
                    {
                        // yes - the new event is for a different input.
                        eventQueue.Enqueue(pendingEvent);
                        pendingEvent = null;
                        log.Debug($"{typeof(CollectingSink).Name}: Pending => Queue: {e.InputId}");
                    }
                    else
                    {
                        // If we're still in the cooloff period (100ms), replace the pending event
                        if ((DateTime.Now - pendingEventTimestamp).TotalMilliseconds < eventCooloffPeriodMs)
                        {
                            // We're within the cooloff period. Replace the pending event.
                            pendingEvent = e;
                            pendingEventTimestamp = DateTime.Now;
                            log.Debug($"{typeof(CollectingSink).Name}: Pending Hot: {e.InputId}");
                            return; // don't send it!
                        }
                    }
                }

                // Add the new event to the event queue
                eventQueue.Enqueue(e);
                lastEvent = e;
                lastEventTimestamp = DateTime.Now;

                if (pendingCollectionTaskCompletion != null && pendingCollectionTaskCompletion.Task.IsCompleted == false)
                {
                    // complete the pending event collection!
                    pendingCollectionTaskCompletion.SetResult(eventQueue.Dequeue());
                }
            }
        }

        public Task Start(CancellationToken cancelToken)
        {

            lock (syncLock)
            {
                if (isStarted)
                {
                    throw new InvalidOperationException("This sink is already started");
                }

                isStarted = true;
            }

            // Kick off the core task
            return OnStart(cancelToken);
        }

        private async Task OnStart(CancellationToken cancelToken)
        {
            log.Debug($"{typeof(CollectingSink).Name}: Starting");
            // Process pending events every little while
            while (cancelToken.IsCancellationRequested == false)
            {
                try
                {
                    await Task.Delay(pendingEventPumpFrequencyMs);
                    lock (syncLock)
                    {
                        if (pendingEvent != null && (DateTime.Now - pendingEventTimestamp).TotalMilliseconds >= eventCooloffPeriodMs)
                        {
                            // fire off that pending event!
                            eventQueue.Enqueue(pendingEvent);
                            lastEvent = pendingEvent;
                            lastEventTimestamp = DateTime.Now;

                            pendingEvent = null;

                            if (pendingCollectionTaskCompletion != null && pendingCollectionTaskCompletion.Task.IsCompleted == false)
                            {
                                // complete the pending event collection!
                                pendingCollectionTaskCompletion.SetResult(eventQueue.Dequeue());
                            }
                        }
                    }

                    // are we due to timeout an event collection task?
                    if (pendingCollectionTimeoutMs > 0 && (DateTime.Now - pendingCollectionStarted).TotalMilliseconds >= pendingCollectionTimeoutMs)
                    {
                        // Fire the collection with a null result
                        lock (syncLock)
                        {
                            pendingCollectionTimeoutMs = -1;
                            pendingCollectionTaskCompletion.SetResult(null);
                        }
                    }
                }
                catch (Exception ex)
                {
                    log.Error(ex, $"{typeof(CollectingSink).Name}: Unhandled Exception");
                }
            }

            log.Debug($"{typeof(CollectingSink).Name}: Stopping");

            if (pendingCollectionTaskCompletion != null && pendingCollectionTaskCompletion.Task.IsCompleted == false)
            {
                // force completion of the pending wait task
                pendingCollectionTaskCompletion.SetResult(null);
            }

            lock (syncLock)
            {
                isStarted = false;
            }
        }

        /// <summary>
        /// Provides a task which completes when input is received. The value of the returned task is the input that fired.
        /// </summary>
        /// <param name="timeoutMs">Times out the collection after the specified period. Set to 0 to never time out.</param>
        /// <returns></returns>
        public async Task<ControlEvent> WaitForInput(int timeoutMs = 0)
        {
            pendingCollectionTaskCompletion = new TaskCompletionSource<ControlEvent>();

            // wait for this thing to be started...
            bool waitingForStart = false;
            var startWaitStopwatch = Stopwatch.StartNew();
            lock (syncLock)
            {
                if (isStarted == false)
                {
                    waitingForStart = true;
                }
            }

            while (waitingForStart)
            {
                await (Task.Delay(50));
                lock (syncLock)
                {
                    if (isStarted)
                    {
                        waitingForStart = false;
                    }
                }

                if (startWaitStopwatch.ElapsedMilliseconds > 1000)
                {
                    pendingCollectionTaskCompletion.SetException(new InvalidOperationException("The Sink is not started"));
                    return await pendingCollectionTaskCompletion.Task;
                }
            }

            // anything in the queue?
            lock (syncLock)
            {
                if (eventQueue.Any())
                {
                    pendingCollectionTaskCompletion.SetResult(eventQueue.Dequeue());
                }
                else
                {
                    pendingCollectionTimeoutMs = timeoutMs;
                    pendingCollectionStarted = DateTime.Now;
                }
            }

            return await pendingCollectionTaskCompletion.Task;
        }
    }
}
