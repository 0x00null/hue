using Autofac;
using Newtonsoft.Json.Linq;
using ZeroNull.Hue.Control;

namespace ZeroNull.Hue.HueActions
{
    /// <summary>
    /// Executes Hue Actions
    /// </summary>
    public class HueActionExecutor : IHueActionExecutor
    {
        private readonly ILifetimeScope scope;
        private readonly IEnumerable<HueActionAttribute> actions;
        public HueActionExecutor(ILifetimeScope scope, IEnumerable<HueActionAttribute> actions)
        {
            this.actions = actions;
            this.scope = scope;
        }
        /// <summary>
        /// Executes the specified Hue Action
        /// </summary>
        /// <param name="actionName"></param>
        /// <param name="options"></param>
        /// <param name="sourceEvent"></param>
        /// <returns></returns>
        public async Task Execute(string actionName, JObject options, ControlEvent sourceEvent)
        {
            actionName = actionName.ToLower();
            // Find an action with the correct id
            var targetAction = actions.FirstOrDefault(t => t.Id == actionName || t.Aliases.Contains(actionName));
            if (targetAction == null)
            {
                $"Unknown action '{actionName}'".Dump(color: ConsoleColor.Red);
                return;
            }

            using (var actionExecutionScope = scope.BeginLifetimeScope())
            {
                var action = actionExecutionScope.ResolveKeyed<IHueAction>(targetAction.Id);

                // Create a context
                var ctx = new HueActionContext()
                {
                    Scope = actionExecutionScope,
                    TargetAction = actionName,
                    Options = options,
                    SourceEvent = sourceEvent
                };

                $"Executing Action '{targetAction.Id}'".Dump(color: ConsoleColor.Yellow);
                try
                {
                    await action.Execute(ctx);
                    $"Finished executing action".Dump(color: ConsoleColor.Yellow);
                }
                catch (Exception ex)
                {
                    $"There was a problem running the action:".Dump(color: ConsoleColor.Red);
                    ex.Message.Dump(color: ConsoleColor.White);
                }

            }
        }

        /// <summary>
        /// Gets all available actions
        /// </summary>
        /// <returns></returns>
        public IEnumerable<HueActionAttribute> GetAllActions()
        {
            return actions;
        }
    }
}
