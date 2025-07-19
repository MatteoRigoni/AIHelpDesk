using AIHelpDesk.AppHost;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

var builder = DistributedApplication.CreateBuilder(args);

builder.Configuration
  .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
  .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
  .AddCommandLine(args);

var tenantName = builder.AddParameter("tenantName");
var qdrantApiKey = builder.AddParameter("qdrantApiKey", secret: true);

var qdrant = builder.AddQdrant("qdrant", qdrantApiKey)        
                    .WithLifetime(ContainerLifetime.Persistent)
                    .WithDataVolume();                         

builder.AddProject<Projects.AIHelpDesk_WebUI>("aihelpdesk-webui")
  .WithEnvironment("ConnectionStrings__sql", builder.Configuration.GetConnectionString("sql"))
  .WithEnvironment("OpenAI__ApiKey", builder.Configuration["OPENAI_API_KEY"])
  .WithEnvironment("TenantInfo__TenantName", tenantName)
  .WithEnvironment("TenantInfo__AiProvider", "OpenAI")
  .WithReference(qdrant).WaitFor(qdrant)   
  .WithHttpHealthCheck("/health");

builder.Build().Run();
