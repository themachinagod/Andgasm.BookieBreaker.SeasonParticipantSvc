﻿using Andgasm.BB.Harvest;
using Andgasm.BB.Harvest.Interfaces;
using Andgasm.BB.SeasonParticipant.Core;
using Andgasm.BB.SeasonParticipant.Interfaces;
using Andgasm.Http;
using Andgasm.Http.Interfaces;
using Andgasm.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;

namespace Andgasm.BB.SeasonParticipant.Extractor.Svc
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
                services.AddSingleton(sp =>
                {
                    return new BusSettings()
                    {
                        ServiceBusHost = Configuration.GetSection("ServiceBus")["ServiceBusHost"],
                        ServiceBusConnectionString = Configuration.GetSection("ServiceBus")["ServiceBusConnectionString"],
                        NewClubSeasonAssociationSubscriptionName = Configuration.GetSection("ServiceBus")["NewSeasonSubscriptionName"],
                        NewClubSeasonAssociationTopicName = Configuration.GetSection("ServiceBus")["NewSeasonTopicName"]
                    };
                });
                services.AddSingleton(sp =>
                {
                    return new ApiSettings()
                    {
                        SeasonsDbApiRootKey = Configuration.GetSection("API")["SeasonsDbApiRootKey"],
                        ClubSeasonRegistrationsApiPath = Configuration.GetSection("API")["ClubSeasonRegistrationsApiPath"]
                    };
                });

                services.AddLogging(loggingBuilder => loggingBuilder
                .AddConsole()
                    .SetMinimumLevel(LogLevel.Debug));

                services.AddTransient<IHttpRequestManager, HttpRequestManager>();
                services.AddTransient<ICookieInitialiser, CookieInitialiser>();
                services.AddTransient<ISeasonParticipantHarvester, SeasonParticipantHarvester>();
                //services.AddSingleton<IHarvestRequestManager>((ctx) =>
                //{
                //    return new HarvestRequestManager(ctx.GetService<ILogger<HarvestRequestManager>>(),
                //                                     ctx.GetService<HttpRequestManager>(),
                //                                     Convert.ToInt32(Configuration["MaxRequestsPerSecond"]));
                //});

                services.AddSingleton<IHarvestRequestManager, HarvestRequestManager>();
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
