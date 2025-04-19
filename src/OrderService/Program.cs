using MassTransit;
using MassTransit.Logging;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OrderService.Data;
using OrderService.Services;
using Serilog;
using Shared.Contracts;

namespace OrderService
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
                r.AddService("OrderService",
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
            builder.Services.AddDbContext<OrderDbContext>(options =>
                options.UseNpgsql(builder.Configuration.GetConnectionString("Postgres")));

            builder.Services.AddSingleton<PriceCacheService>();

            builder.Services.AddMassTransit(x =>
            {
                x.AddConsumer<PriceCacheService>();

                x.UsingRabbitMq((context, cfg) =>
                {
                    cfg.Host(builder.Configuration["RabbitMq:Host"], "/", h =>
                    {
                        h.Username("guest");
                        h.Password("guest");
                    });

                    cfg.ReceiveEndpoint("order-price-cache", e =>
                    {
                        e.ConfigureConsumer<PriceCacheService>(context);
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
                var db = scope.ServiceProvider.GetRequiredService<OrderDbContext>();
                db.Database.Migrate(); // Applies any pending migrations
            }

            app.Run();
        }
    }
}
