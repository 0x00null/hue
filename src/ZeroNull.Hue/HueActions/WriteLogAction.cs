using Serilog;

namespace ZeroNull.Hue.HueActions
{

    public class WriteLogOptions
    {
        /// <summary>
        /// Items to write to the log
        /// </summary>
        public IEnumerable<string> Items { get; set; }
    }

    [HueAction("write-log", "Write a Log entry", "log", "debug", "dump")]
    public class WriteLogAction : IHueAction
    {
        private readonly ILogger log;
        public WriteLogAction(ILogger log)
        {
            this.log = log;
        }
        public Task Execute(HueActionContext context)
        {
            var options = context.Options.ToObject<WriteLogOptions>();
            if (options?.Items?.Any() ?? false)
            {
                foreach (var item in options.Items)
                {
                    log.Information(item);
                }
            }
            else
            {
                log.Information("Hue Action Executed");
            }

            return Task.CompletedTask;
        }
    }
}
