using AIHelpDesk.AppHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

var builder = DistributedApplication.CreateBuilder(args);

builder.Configuration
  .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
  .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
  .AddCommandLine(args);

builder.AddProject<Projects.AIHelpDesk_WebUI>("aihelpdesk-webui")
  .WithEnvironment("ConnectionStrings__sql", builder.Configuration.GetConnectionString("sql"))
  .WithHttpHealthCheck("/health");

builder.Build().Run();
