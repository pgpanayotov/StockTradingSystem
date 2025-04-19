using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace ApiGateway
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add OpenTelemetry
            void ConfigureResource(ResourceBuilder r)
            {
                r.AddService("ApiGateway",
                    serviceVersion: "1.0.0",
                    serviceInstanceId: Environment.MachineName);
            }
            builder.Services.AddOpenTelemetry()
                .ConfigureResource(ConfigureResource)
                .WithTracing(b => b
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddOtlpExporter()
            );

            // Load proxy config from appsettings.json
            builder.Services.AddReverseProxy()
                .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

            var app = builder.Build();

            app.MapReverseProxy();

            app.Run();
        }
    }
}
