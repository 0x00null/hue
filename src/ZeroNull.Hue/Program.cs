// See https://aka.ms/new-console-template for more information
using Autofac;
using CommandLine;
using Serilog;
using System.Reflection;
using ZeroNull.Hue;
using ZeroNull.Hue.Control;
using ZeroNull.Hue.EventRouting;
using ZeroNull.Hue.HueActions;
using ZeroNull.Hue.StateStorage;
using ZeroNull.Hue.Verbs;

"Hue: Control your lights".Dump();
"https://github.com/0x00null/hue".Dump();
"-------------------------------".Dump();
Console.WriteLine();

var cst = new CancellationTokenSource();

// A rather nasty hack to lift out the 'debug' command line argument
var logConfig = new LoggerConfiguration()
        .WriteTo.Console();

if (args != null && args.Length > 0 && args.Any(a => a.Equals("--debug", StringComparison.InvariantCultureIgnoreCase)))
{
    logConfig.MinimumLevel.Debug();
}

// This is the global shared log instance
var log = logConfig.CreateLogger();

Console.CancelKeyPress += (s, e) =>
{
    cst.Cancel();
    e.Cancel = true;
    "Exiting...".Dump();
};


var builder = new ContainerBuilder();

var thisAssembly = typeof(Program).Assembly;

// Register core services
builder
    .RegisterInstance(log)
    .As<ILogger>();

builder.RegisterType<OnDiskAppStateStore>()
    .AsImplementedInterfaces();

builder.RegisterType<CollectingSink>()
    .AsImplementedInterfaces();

// Register all verbs
var optionsTypes = thisAssembly.GetTypes().Where(t => t.GetCustomAttribute<VerbAttribute>() != null);
var optionHandlerTypes = thisAssembly.GetTypes().Where(t => t.GetInterfaces().Any(it => it.Equals(typeof(IVerbHandler))));

foreach (var optionsType in optionsTypes)
{
    builder.RegisterInstance(optionsType)
        .Keyed<Type>("verb");

    var handlerType = optionHandlerTypes.First(t => t.GetInterfaces().Contains(typeof(IVerbHandler<>).MakeGenericType(optionsType)));

    builder.RegisterType(handlerType)
        .AsSelf()
        .As<IVerbHandler>()
        .Keyed<IVerbHandler>(optionsType);

}

// Register all Control Event Sources
builder.RegisterAssemblyTypes(thisAssembly)
    .Where(t => t.IsClass && t.IsPublic && t.IsAbstract == false && t.GetInterface(typeof(IControlEventSourceFactory).FullName) != null)
    .As<IControlEventSourceFactory>()
    .SingleInstance();

// Register all hue actions
var actionTypes = thisAssembly.GetTypes().Where(t => t.GetCustomAttribute<HueActionAttribute>() != null);
foreach (var actionType in actionTypes)
{
    var attrib = actionType.GetCustomAttribute<HueActionAttribute>();
    builder.RegisterInstance(attrib)
        .As<HueActionAttribute>()
        .Keyed<HueActionAttribute>(attrib.Id);

    builder.RegisterType(actionType)
        .As<IHueAction>()
        .Keyed<IHueAction>(attrib.Id);

}

// Register action execution infrastructure
builder.RegisterType<HueActionExecutor>()
    .AsImplementedInterfaces();

// Register the event routing infrastructure
builder.RegisterType<DefaultEventRouter>()
    .AsImplementedInterfaces();

builder.RegisterType<AppStateRouteSource>()
    .AsImplementedInterfaces();

var container = builder.Build();

// the verb is the first argument
if (args.Length == 0)
{
    // show help
    Console.WriteLine("Use 'hue help' for usage");
    return;
}

var verbName = args[0].ToLower();
var allVerbTypes = container.ResolveKeyed<IEnumerable<Type>>("verb");

var parseResult = CommandLine.Parser.Default.ParseArguments(args, allVerbTypes.ToArray());

await parseResult.WithParsedAsync(o =>
{
    var handler = container.ResolveKeyed<IVerbHandler>(o.GetType());
    return handler.HandleAsync(o, cst.Token);
});
