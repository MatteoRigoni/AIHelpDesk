# AIHelpDesk

This project is an ASP.NET Core application using MudBlazor and Identity. 

## Observability

Telemetry and logging are configured through a .NET Aspire AppHost. The OTLP endpoint is controlled via the `OpenTelemetry:OtlpEndpoint` setting found in the host `appsettings.json`.

Tracing and logging use OpenTelemetry and are centralized in the Aspire host so the WebUI project remains minimal.

## Building

```
dotnet build
```

## Running

```
dotnet run --project AIHelpDesk.AppHost
```
