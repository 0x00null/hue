using Autofac;
using CommandLine;
using ZeroNull.Hue.HueActions;

namespace ZeroNull.Hue.Verbs
{
    [Verb("action", HelpText = "Lists available Actions")]
    public class ListActionsOptions : VerbOptionsBase
    {
        [Value(0, Required = true, HelpText = "'list' for a list of all actions, or the name of an action to execute")]
        public string Action { get; set; }

        [CommandLine.Value(1, Required = false, HelpText = "Options to pass to the action")]
        public IEnumerable<string> Options { get; set; }
    }

    public class ListActionsHandler : VerbHandler<ListActionsOptions>
    {
        private readonly ILifetimeScope scope;
        private readonly IHueActionExecutor actionExecutor;
        public ListActionsHandler(ILifetimeScope scope, IHueActionExecutor actionExecutor)
        {
            this.actionExecutor = actionExecutor;
            this.scope = scope;
        }
        protected override async Task OnHandleAsync(ListActionsOptions options, CancellationToken cancelToken)
        {
            options.Action = options.Action.ToLower();
            if (options.Action == "list")
            {
                "Run an action with 'hue action <action id> [options]'".Dump();
                "Available actions:".Dump();
                foreach (var action in actionExecutor.GetAllActions())
                {
                    action.Id.DumpHeading();
                    if (action.Aliases.Any())
                    {
                        string.Join(", ", action.Aliases).Dump("Aliases");
                    }
                    action.Description.Dump();
                }
            }
            else
            {
                await actionExecutor.Execute(options.Action, options.Options.ToJObject(), null);
            }
        }
    }
}
