using System;
using KF.Metrics;
using KF.Metrics.AspNet;
using Microsoft.AspNetCore.Mvc;

namespace KF.Metrics.AspNet.Tests;

public class MonitoringControllerBaseTests
{
    [Fact]
    public void Constructor_Throws_WhenSnapshotsIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => new TestController(null!));
    }

    [Fact]
    public void GetSnapshotCore_ReturnsSnapshotFromProvider()
    {
        var snapshot = new MonitoringSnapshot(
            DateTimeOffset.UtcNow,
            Array.Empty<OperationSnapshot>());

        var provider = new StubSnapshotProvider(snapshot);
        var controller = new TestController(provider);

        var result = controller.Invoke();

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(snapshot, ok.Value);
    }

    private sealed class TestController : MonitoringControllerBase
    {
        public TestController(IMonitoringSnapshotProvider snapshots) : base(snapshots)
        {
        }

        public ActionResult<MonitoringSnapshot> Invoke() => GetSnapshotCore();
    }

    private sealed class StubSnapshotProvider : IMonitoringSnapshotProvider
    {
        private readonly MonitoringSnapshot _snapshot;

        public StubSnapshotProvider(MonitoringSnapshot snapshot)
        {
            _snapshot = snapshot;
        }

        public MonitoringSnapshot GetSnapshot() => _snapshot;

        public OperationSnapshot? GetOperationSnapshot(string name) => null;
    }
}
