using System;
using Andgasm.BB.SeasonParticipant.Core;
using Andgasm.BB.SeasonParticipant.API.Models;
using Andgasm.ServiceBus;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Andgasm.BB.SeasonParticipant.API
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddLogging(loggingBuilder => loggingBuilder
                                .AddConsole()
                                .SetMinimumLevel(LogLevel.Debug));
            services.AddDbContext<SeasonParticipantsDb>(options => options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            services.AddMvc(x => x.EnableEndpointRouting = false)
                    .SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
            services.AddCors();
            services.AddSwaggerDocument();
            services.Configure<BusSettings>(Configuration.GetSection("ServiceBus"));
           
            services.AddTransient<Func<string, IBusClient>>(serviceProvider => key =>
            {
                switch (key)
                {
                    case "NewClubSeason":
                        return ServiceBusFactory.GetBus(Enum.Parse<BusHost>(Configuration.GetSection("ServiceBus")["ServiceBusHost"]),
                                                                           Configuration.GetSection("ServiceBus")["ServiceBusConnectionString"],
                                                                           Configuration.GetSection("ServiceBus")["NewClubSeasonAssociationTopicName"],
                                                                           Configuration.GetSection("ServiceBus")["NewClubSeasonAssociationSubscriptionName"]);
                    case "NewSeason":
                        return ServiceBusFactory.GetBus(Enum.Parse<BusHost>(Configuration.GetSection("ServiceBus")["ServiceBusHost"]),
                                                                           Configuration.GetSection("ServiceBus")["ServiceBusConnectionString"],
                                                                           Configuration.GetSection("ServiceBus")["NewSeasonTopicName"],
                                                                           Configuration.GetSection("ServiceBus")["NewSeasonSubscriptionName"]);
                    default:
                        throw new InvalidOperationException("Specified bus type does not exist!");
                }
            });

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app)
        {
            app.UseDeveloperExceptionPage();
            app.UseOpenApi();
            app.UseSwaggerUi3();
            app.UseHsts();
            
            app.UseCors(x => x.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
            app.UseHttpsRedirection();
            app.UseMvc();
            InitialiseData(app.ApplicationServices);
        }

        public static async void InitialiseData(IServiceProvider svcs)
        {
            // but of a hack here but this just ensure that the db exists and that the basic data is configured
            // helps support basic ui tests as well as expected config for the admin web help instructions

            using (var servicescope = svcs.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                var context = servicescope.ServiceProvider.GetService<SeasonParticipantsDb>();
                await context.Database.EnsureCreatedAsync();
            }
        }
    }
}
