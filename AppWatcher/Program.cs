using AppWatcher;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services => { services.AddHostedService<Worker>().AddSingleton<IConfig,Config>(); })
    
    .Build();


await host.RunAsync();