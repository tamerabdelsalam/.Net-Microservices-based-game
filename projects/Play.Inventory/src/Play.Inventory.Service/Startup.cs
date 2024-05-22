using System;
using System.Net.Http;
using DnsClient.Internal;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Play.Common.MassTransit;
using Play.Common.MongoDB;
using Polly;
using Polly.Timeout;

namespace Play.Inventory.Service
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
            services.AddMongo()
                    .AddMongoRepository<Entities.InventoryItem>("inventoryItems")
                    .AddMongoRepository<Entities.CatalogItem>("catalogItems")
                    .AddMassTransitWithRabbitMQ();

            AddCatalogClient(services);

            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Play.Inventory.Service", Version = "v1" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Play.Inventory.Service v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

        private static void AddCatalogClient(IServiceCollection services)
        {
            Random randNum = new();

            services.AddHttpClient<Clients.CatalogClient>(client =>
            {
                client.BaseAddress = new Uri("https://localhost:5002");
            })
            .AddTransientHttpErrorPolicy(builder => builder.Or<TimeoutRejectedException>()
                                                           .WaitAndRetryAsync(
                                                            5,
                                                            retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))
                                                                          + TimeSpan.FromMilliseconds(randNum.Next(0, 1000))
            //  The following code is used for logging the retry attempts, for debugging purposes only                                                             
            , onRetry: (outcome, timespan, retryAttempt) =>
            {
                var serviceProvider = services.BuildServiceProvider();
                serviceProvider.GetService<ILogger<Clients.CatalogClient>>()?.LogWarning($"Delaying for {timespan.TotalSeconds} seconds, then making retry {retryAttempt}");
            }
            ))
            .AddTransientHttpErrorPolicy(builder => builder.Or<TimeoutRejectedException>().CircuitBreakerAsync(
            // Number of exceptions before breaking the circuit
             handledEventsAllowedBeforeBreaking: 3,
            // Time circuit opened before retrying
            durationOfBreak: TimeSpan.FromSeconds(15),
            // On circuit opened
            onBreak: (outcome, timespan) =>
            {
                var serviceProvider = services.BuildServiceProvider();
                serviceProvider.GetService<ILogger<Clients.CatalogClient>>()?.LogWarning($"Breaking the circuit for {timespan.TotalSeconds} seconds...");
            },
            onReset: () =>
            {
                var serviceProvider = services.BuildServiceProvider();
                serviceProvider.GetService<ILogger<Clients.CatalogClient>>()?.LogWarning($"Closing the circuit...");
            }
            ))
            .AddPolicyHandler(Policy.TimeoutAsync<HttpResponseMessage>(1)); // Timeout after 1 second
        }
    }
}
