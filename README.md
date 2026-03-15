# KoreForge.Metrics.AspNet

ASP.NET Core opt-in monitoring endpoints and controller base for [KoreForge.Metrics](https://www.nuget.org/packages/KoreForge.Metrics).

## Installation

```bash
dotnet add package KoreForge.Metrics.AspNet
```

## Quick Start

### Minimal API

```csharp
builder.Services.AddKoreForgeMetrics();

app.MapMonitoringEndpoints(); // GET /monitoring/snapshot
```

### MVC / Controller

```csharp
[Route("ops")]
public class MonitoringController : MonitoringControllerBase
{
    public MonitoringController(IMonitoringSnapshotProvider snapshots)
        : base(snapshots) { }

    [HttpGet("snapshot")]
    public IActionResult Snapshot() => GetSnapshotCore();
}
```

## License
MIT
