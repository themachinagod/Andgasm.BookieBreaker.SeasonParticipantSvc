﻿using Andgasm.BookieBreaker.Harvest;
using Andgasm.BookieBreaker.SeasonParticipant.Core;
using Andgasm.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;

namespace Andgasm.BookieBreaker.SeasonParticipant.Extractor.Svc
{
    public class Startup
    {
        public IHostBuilder Host {get; internal set;}
        public IConfiguration Configuration { get; internal set; }

        public Startup()
        {
            Host = new HostBuilder()
            .ConfigureAppConfiguration((hostContext, config) =>
            {
                config.SetBasePath(Environment.CurrentDirectory);
                config.AddJsonFile("appsettings.json", optional: false);
                config.AddJsonFile($"appsettings.{hostContext.HostingEnvironment.EnvironmentName}.json", optional: true);
                Configuration = config.Build();
            });
            ConfigureServices();
        }

        public void ConfigureServices()
        {
            Host.ConfigureServices((_hostcontext, services) =>
            {
                services.Configure<BusSettings>(Configuration.GetSection("ServiceBus"));
                services.Configure<ApiSettings>(Configuration.GetSection("API"));
                services.AddTransient(typeof(SeasonParticipantHarvester));
                services.AddTransient(typeof(HarvestRequestManager));
                services.AddLogging(loggingBuilder => loggingBuilder
                    .AddConsole()
                    .SetMinimumLevel(LogLevel.Debug));

                services.AddSingleton(sp =>
                {
                    return ServiceBusFactory.GetBus(Enum.Parse<BusHost>(Configuration.GetSection("ServiceBus")["ServiceBusHost"]),
                                                                        Configuration.GetSection("ServiceBus")["ServiceBusConnectionString"],
                                                                        Configuration.GetSection("ServiceBus")["NewSeasonTopicName"],
                                                                        Configuration.GetSection("ServiceBus")["NewSeasonSubscriptionName"]);
                });
                services.AddScoped<IHostedService, SeasonParticipantExtractorSvc>();
            });
        }
    }
}
