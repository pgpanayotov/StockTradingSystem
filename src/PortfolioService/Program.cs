using MassTransit;
using MassTransit.Logging;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using PortfolioService.Consumers;
using PortfolioService.Data;
using Serilog;
using Shared.Contracts;

namespace PortfolioService
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
                r.AddService("PortfolioService",
                    serviceVersion: "1.0.0",
                    serviceInstanceId: Environment.MachineName);
            }
            builder.Services.AddOpenTelemetry()
                .ConfigureResource(ConfigureResource)
                .WithTracing(b => b
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddNpgsql()
                    .AddSource(DiagnosticHeaders.DefaultListenerName) // MassTransit ActivitySource
                    .AddOtlpExporter()
            );

            // Add services to the container.
            builder.Services.AddDbContext<PortfolioDbContext>(options =>
                options.UseNpgsql(builder.Configuration.GetConnectionString("Postgres")));

            builder.Services.AddMassTransit(x =>
            {
                x.AddConsumer<OrderExecutedConsumer>();
                x.AddConsumer<PriceUpdatedConsumer>();

                x.UsingRabbitMq((context, cfg) =>
                {
                    cfg.Host(builder.Configuration["RabbitMq:Host"], "/", h =>
                    {
                        h.Username("guest");
                        h.Password("guest");
                    });

                    cfg.ReceiveEndpoint("portfolio-service", e =>
                    {
                        e.ConfigureConsumer<OrderExecutedConsumer>(context);
                        e.ConfigureConsumer<PriceUpdatedConsumer>(context);
                    });

                    cfg.Message<PriceUpdated>(x => x.SetEntityName("price.updated"));
                    cfg.Message<OrderExecuted>(x => x.SetEntityName("order.executed"));

                });
            });

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseAuthorization();

            app.MapControllers();

            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<PortfolioDbContext>();
                db.Database.Migrate(); // Applies any pending migrations
            }

            app.Run();
        }
    }
}
