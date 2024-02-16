using ClamShell.ClamServer;
using ClamShell.ClamServer.Services;
using ClamShell.MessageBus.Models.Payloads;
using ClamShell.MessageBus;
using ClamShell.ClamServer.Helpers;
using nClam;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services
        .AddHostedService<Worker>()
        .AddSingleton<IConfiguration>(ConfigExtractor.GetConfiguration())
        .AddSingleton(new Consumer<ScanRequestData>("scan_request"))
        .AddSingleton(new ClamClient(@"clamshell_server", 3310))
        .AddSingleton<HasherService>();
    })
    .Build();

await host.RunAsync();
