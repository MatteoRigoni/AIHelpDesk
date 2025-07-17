using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OpenTelemetry.Logs;

var builder = DistributedApplication.CreateBuilder(args);
var otlpEndpoint = builder.Configuration["OpenTelemetry:OtlpEndpoint"];

builder.AddProject<Projects.AIHelpDesk_WebUI>("webui")
       .WithEnvironment("ASPNETCORE_ENVIRONMENT", builder.EnvironmentName)
       .WithEnvironment("ConnectionStrings__ApplicationDbContextConnection",
                       builder.Configuration.GetConnectionString("ApplicationDbContextConnection") ?? string.Empty);

builder.AddOpenTelemetry("telemetry")
       .WithTracing(options =>
       {
           options.ConfigureResource(r => r.AddService("AIHelpDesk"));
           options.AddAspNetCoreInstrumentation();
           options.AddHttpClientInstrumentation();
           options.AddEntityFrameworkCoreInstrumentation();
           options.AddOtlpExporter(o =>
           {
               if (!string.IsNullOrWhiteSpace(otlpEndpoint))
               {
                   o.Endpoint = new Uri(otlpEndpoint);
               }
           });
       })
       .WithLogging(options =>
       {
           options.ConfigureResource(r => r.AddService("AIHelpDesk"));
           options.IncludeScopes = true;
           options.IncludeFormattedMessage = true;
           options.ParseStateValues = true;
           options.AddOtlpExporter(o =>
           {
               if (!string.IsNullOrWhiteSpace(otlpEndpoint))
               {
                   o.Endpoint = new Uri(otlpEndpoint);
               }
           });
       });

var app = builder.Build();
app.Run();

