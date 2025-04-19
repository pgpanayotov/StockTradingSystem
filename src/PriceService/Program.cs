using MassTransit;
using MassTransit.Logging;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using PriceService.Services;
using Serilog;
using Shared.Contracts;

namespace PriceService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            var log = new LoggerConfiguration().ReadFrom.Configuration(builder.Configuration)
                .CreateLogger();

            builder.Host.UseSerilog(log);

            // Add OpenTelemetry
            void ConfigureResource(ResourceBuilder r)
            {
                r.AddService("PriceService",
                    serviceVersion: "1.0.0",
                    serviceInstanceId: Environment.MachineName);
            }
            builder.Services.AddOpenTelemetry()
                .ConfigureResource(ConfigureResource)
                .WithTracing(b => b
                    .AddAspNetCoreInstrumentation()
                    .AddSource(DiagnosticHeaders.DefaultListenerName) // MassTransit ActivitySource
                    .AddOtlpExporter()
            );

            // Add services to the container.
            builder.Services.AddMassTransit(x =>
            {
                x.UsingRabbitMq((context, cfg) =>
                {
                    cfg.Host(builder.Configuration["RabbitMq:Host"], "/", h =>
                    {
                        h.Username("guest");
                        h.Password("guest");
                    });
                    cfg.Message<PriceUpdated>(x => x.SetEntityName("price.updated"));
                });
            });

            builder.Services.AddHostedService<PriceGeneratorService>();

            var app = builder.Build();

            app.Run();
        }
    }
}
