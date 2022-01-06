using Autofac;
using CommandLine;
using System.Text;
using ZeroNull.Hue.HueActions;
using ZeroNull.Hue.StateStorage;

namespace ZeroNull.Hue.Verbs
{
    [Verb("route", HelpText = "Manages the Action routes you've configured")]
    public class RouteVerbOptions : VerbOptionsBase
    {
        [Value(0, Required = true, HelpText = "'list' for a list of all routes, 'clear' to remove all routes, 'del' to delete a route")]
        public string Subcommand { get; set; }

        [Value(1, Required = false, HelpText = "the index of the route, if you're using the 'del' or 'test' commands")]
        public int? Index { get; set; }
    }

    public class RouteVerbHandler : VerbHandler<RouteVerbOptions>
    {
        private readonly IAppStateStore stateStore;
        private readonly ILifetimeScope scope;
        public RouteVerbHandler(IAppStateStore stateStore, ILifetimeScope scope)
        {
            this.scope = scope;
            this.stateStore = stateStore;
        }
        protected override async Task OnHandleAsync(RouteVerbOptions options, CancellationToken cancelToken)
        {
            options.Subcommand = options.Subcommand.ToLower();
            if (options.Subcommand == "list")
            {
                OnListRoutes();
            }
            else if (options.Subcommand == "clear")
            {
                OnClearRoutes();
            }
            else if (options.Subcommand == "del")
            {
                OnDeleteRoute(options);
            }
            else if (options.Subcommand == "test")
            {
                await OnTestRoute(options);
            }
            else
            {
                "Unknown command".Dump(color: ConsoleColor.Red);
            }
        }


        private void OnListRoutes()
        {
            var state = stateStore.Get();
            if (state.Routes.Any() == false)
            {
                "There are no registered routes. Use 'hue map <action>' to create a route".Dump();
                return;
            }

            "Registered Action Routes".DumpHeading();

            int indexColumnWidth = 3;
            int inputColWidth = state.Routes.Max(r => r.InputId.Length) + 1;
            int eventTypeColWidth = state.Routes.Where(r => r.EventType.HasValue).Max(r => r.EventType.Value.ToString().Length) + 1;
            int aboveColWidth = 0;
            if (state.Routes.Any(r => r.TriggerAbove.HasValue))
            {
                aboveColWidth = state.Routes.Where(r => r.TriggerAbove.HasValue).Max(r => r.TriggerAbove.Value.ToString().Length) + 1;
            }

            int belowColWidth = 0;
            if (state.Routes.Any(r => r.TriggerBelow.HasValue))
            {
                aboveColWidth = state.Routes.Where(r => r.TriggerBelow.HasValue).Max(r => r.TriggerBelow.Value.ToString().Length) + 1;
            }

            int actionColWidth = state.Routes.Max(r => r.TargetAction.Length) + 1;
            if (actionColWidth < 7)
            {
                actionColWidth = 7;
            }

            int optionsColWidth = Console.WindowWidth - (
                +(indexColumnWidth + 2)
                + (inputColWidth + 2)
                + (eventTypeColWidth + 2)
                + (aboveColWidth > 0 ? aboveColWidth + 2 : 0)
                + (belowColWidth > 0 ? belowColWidth + 2 : 0)
                + (actionColWidth + 2)
                + 3
            );



            if (optionsColWidth < 0)
            {
                optionsColWidth = 0;
            }

            var sb = new StringBuilder();

            // Header row
            sb.Append("| ").Append("ix".PadRight(indexColumnWidth, ' '))
                .Append("| ").Append("input".PadRight(inputColWidth, ' '))
                .Append("| ").Append("event".PadRight(eventTypeColWidth, ' '));

            if (aboveColWidth > 0)
            {
                sb.Append("| ").Append(">=".PadRight(aboveColWidth, ' '));
            }

            if (belowColWidth > 0)
            {
                sb.Append("| ").Append("<".PadRight(belowColWidth, ' '));
            }

            sb.Append("| ").Append("action".PadRight(actionColWidth, ' '));

            if (optionsColWidth > 0)
            {
                sb.Append("| ").Append("options".PadRight(optionsColWidth, ' '));
            }
            sb.AppendLine("|");


            // Separator
            sb.Append("|-").Append(string.Empty.PadRight(indexColumnWidth, '-'))
                .Append("|-").Append(string.Empty.PadRight(inputColWidth, '-'))
                .Append("|-").Append(string.Empty.PadRight(eventTypeColWidth, '-'));

            if (aboveColWidth > 0)
            {
                sb.Append("|-").Append(string.Empty.PadRight(aboveColWidth, '-'));
            }

            if (belowColWidth > 0)
            {
                sb.Append("|-").Append(string.Empty.PadRight(belowColWidth, '-'));
            }

            sb.Append("|-").Append(string.Empty.PadRight(actionColWidth, '-'));

            if (optionsColWidth > 0)
            {
                sb.Append("|-").Append(string.Empty.PadRight(optionsColWidth, '-'));
            }
            sb.AppendLine("|");


            int index = 0;
            foreach (var route in state.Routes)
            {
                sb.Append("| ").Append(index.ToString().PadRight(indexColumnWidth, ' '))
                .Append("| ").Append(route.InputId.PadRight(inputColWidth, ' '));

                if (route.EventType.HasValue)
                {
                    sb.Append("| ").Append(route.EventType.Value.ToString().PadRight(eventTypeColWidth, ' '));
                }
                else
                {
                    sb.Append("| ").Append(string.Empty.PadRight(eventTypeColWidth, ' '));
                }


                if (route.TriggerAbove.HasValue)
                {
                    sb.Append("| ").Append(route.TriggerAbove.Value.ToString().PadRight(aboveColWidth, ' '));
                }
                else if (aboveColWidth > 0)
                {
                    sb.Append("| ").Append(string.Empty.PadRight(aboveColWidth, ' '));
                }

                if (route.TriggerBelow.HasValue)
                {
                    sb.Append("| ").Append(route.TriggerBelow.Value.ToString().PadRight(belowColWidth, ' '));
                }
                else if (belowColWidth > 0)
                {
                    sb.Append("| ").Append(string.Empty.PadRight(belowColWidth, ' '));
                }

                sb.Append("| ").Append(route.TargetAction.PadRight(actionColWidth, ' '));

                if (optionsColWidth > 0)
                {
                    var optionsString = route.Options.ToString(Newtonsoft.Json.Formatting.None).Replace("{", string.Empty).Replace("}", string.Empty);
                    if (optionsString.Length > optionsColWidth)
                    {
                        optionsString = optionsString.Substring(0, optionsColWidth);
                    }


                    sb.Append("| ").Append(optionsString.PadRight(optionsColWidth, ' '));
                }

                sb.AppendLine("|");

                index++;
            }



            Console.WriteLine(sb.ToString());
            "Remove routes with 'hue route del <ix>'".Dump();
            "Test routes with 'hue route test <ix>'".Dump();
        }

        private void OnClearRoutes()
        {
            var state = stateStore.Get();
            if (state.Routes.Any() == false)
            {
                "There are no registered routes. Use 'hue map <action>' to create a route".Dump();
                return;
            }

            // Clear all routes
            state.Routes.Clear();
            stateStore.Put(state);
            "Routes cleared".Dump(color: ConsoleColor.Yellow);
        }
        private void OnDeleteRoute(RouteVerbOptions options)
        {
            var state = stateStore.Get();
            state.Routes.RemoveAt(options.Index.Value);
            stateStore.Put(state);
            $"Route {options.Index.Value} deleted".Dump(color: ConsoleColor.Yellow);
        }
        private async Task OnTestRoute(RouteVerbOptions options)
        {
            var state = stateStore.Get();
            if (options.Index.HasValue == false || options.Index.Value < 0 || options.Index.Value >= state.Routes.Count)
            {
                "Invalid route index. Use 'hue route list' to see all registered routes.".Dump(color: ConsoleColor.Red);
                return;
            }

            var route = state.Routes[options.Index.Value];

            // create a context and off we go
            $"Testing route {options.Index}: Action '{route.TargetAction}'".DumpHeading();
            try
            {
                using (var actionScope = scope.BeginLifetimeScope())
                {
                    var context = new HueActionContext()
                    {
                        Scope = actionScope,
                        Options = route.Options,
                        TargetAction = route.TargetAction
                    };

                    var action = actionScope.ResolveKeyed<IHueAction>(route.TargetAction);
                    await action.Execute(context);
                    $"Finished".Dump(color: ConsoleColor.Green);
                }
            }
            catch (Exception ex)
            {
                $"There was a problem running the action: {ex.Message}".Dump(color: ConsoleColor.Red);
            }
        }

    }
}
