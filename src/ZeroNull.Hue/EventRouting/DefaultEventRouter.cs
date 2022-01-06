using Autofac;
using Serilog;
using ZeroNull.Hue.Control;
using ZeroNull.Hue.HueActions;

namespace ZeroNull.Hue.EventRouting
{
    /// <summary>
    /// The default event router
    /// </summary>
    public class DefaultEventRouter : IEventRouter
    {
        private readonly object syncLock = new object();
        private IEnumerable<EventRoute> routes = null;
        private readonly Queue<ControlEvent> pendingEvents = new Queue<ControlEvent>();
        private TaskCompletionSource pendingEventCompletionSource = null;
        private readonly ILifetimeScope scope;
        private readonly IEnumerable<IEventRouteSource> routeSources;
        private readonly ILogger log;
        private readonly IHueActionExecutor actionExecutor;
        public DefaultEventRouter(ILifetimeScope scope, IEnumerable<IEventRouteSource> routeSources, ILogger log, IHueActionExecutor actionExecutor)
        {
            this.actionExecutor = actionExecutor;
            this.log = log;
            this.routeSources = routeSources;
            this.scope = scope;
        }

        public void Route(ControlEvent ev)
        {
            lock (syncLock)
            {
                pendingEvents.Enqueue(ev);
                if (pendingEventCompletionSource != null && pendingEventCompletionSource.Task.IsCompleted == false)
                {
                    pendingEventCompletionSource.SetResult();
                }
            }
        }

        public Task Start(CancellationToken cancelToken)
        {
            log.Debug($"{typeof(DefaultEventRouter).Name}: Starting");
            return Task.WhenAll(
                OnMonitorQueue(cancelToken),
                OnPumpQueue(cancelToken)
            );
        }

        private async Task OnPumpQueue(CancellationToken cancelToken)
        {
            log.Debug($"{typeof(DefaultEventRouter).Name}: Queue Pump Started");
            while (cancelToken.IsCancellationRequested == false)
            {
                await Task.Delay(100);
                pendingEventCompletionSource.SetResult();
            }
            log.Debug($"{typeof(DefaultEventRouter).Name}: Queue Pump Stopped");
        }

        private async Task OnMonitorQueue(CancellationToken cancelToken)
        {
            log.Debug($"{typeof(DefaultEventRouter).Name}: Queue Monitor Started");

            lock (syncLock)
            {
                pendingEventCompletionSource = new TaskCompletionSource();
            }
            while (cancelToken.IsCancellationRequested == false)
            {
                await pendingEventCompletionSource.Task;

                // pop an event off the queue
                ControlEvent ev = null;
                lock (syncLock)
                {
                    if (pendingEvents.Count == 0)
                    {
                        pendingEventCompletionSource = new TaskCompletionSource();
                        continue;
                    }

                    ev = pendingEvents.Dequeue();
                }

                // Have we loaded routes?
                if (routes == null)
                {
                    log.Debug($"{typeof(DefaultEventRouter).Name}: Finding Routes");
                    // Fetch routes from all sources
                    var routeResults = await Task.WhenAll(routeSources.Select(s => s.GetRoutes()));
                    routes = routeResults.SelectMany(s => s).ToArray();

                    log.Debug($"{typeof(DefaultEventRouter).Name}: Found {routes.Count()} routes");
                }

                log.Debug($"{typeof(DefaultEventRouter).Name}: Match routes for '{ev}'");

                // Let's see if we can find a route
                IEnumerable<EventRoute> matchedRoutes;
                lock (syncLock)
                {
                    matchedRoutes = routes.Where(m =>
                        {
                            if (m.InputId != ev.InputId)
                            {
                                return false;
                            }

                            if (m.EventType.HasValue)
                            {
                                if (ev.Type != m.EventType.Value)
                                {
                                    return false;
                                }
                            }

                            if (m.TriggerBelow.HasValue)
                            {
                                if (ev.Value >= m.TriggerBelow.Value)
                                {
                                    return false;
                                }
                            }

                            if (m.TriggerAbove.HasValue)
                            {
                                if (ev.Value < m.TriggerAbove.Value)
                                {
                                    return false;
                                }
                            }

                            return true;
                        });
                }

                if (matchedRoutes.Any() == false)
                {
                    log.Debug($"{typeof(DefaultEventRouter).Name}: No matching routes");
                    continue;
                }

                log.Debug($"{typeof(DefaultEventRouter).Name}: {matchedRoutes.Count()} routes matched");

                // We've got one or more actions to run. Run them via Tasks
                // (though not in parallel just yet - maybe in the future! If we do run them in parallel, we'll need a
                // 'runSynchronous' setting on actions such as the 'select' action, so we can effectively control
                // execution order in certain situations)
                foreach (var route in matchedRoutes)
                {
                    await actionExecutor.Execute(route.TargetAction, route.Options, ev);
                }
            }
            log.Debug($"{typeof(DefaultEventRouter).Name}: Queue Monitor Stopped");
        }
    }
}
