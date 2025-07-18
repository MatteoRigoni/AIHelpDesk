using AIHelpDesk.AppHost;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

var builder = DistributedApplication.CreateBuilder(args);

var qdrant = builder.AddQdrant("qdrant")                        // nome logico "qdrant"
                    .WithLifetime(ContainerLifetime.Persistent) // evita restart lenti
                    .WithDataVolume();                          // persistenza locale

builder.Configuration
  .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
  .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
  .AddCommandLine(args);

builder.AddProject<Projects.AIHelpDesk_WebUI>("aihelpdesk-webui")
  .WithEnvironment("ConnectionStrings__sql", builder.Configuration.GetConnectionString("sql"))
  .WithEnvironment("OpenAI__ApiKey", builder.Configuration["OPENAI_API_KEY"])
  .WithReference(qdrant).WaitFor(qdrant)   
  .WithHttpHealthCheck("/health");

builder.Build().Run();
