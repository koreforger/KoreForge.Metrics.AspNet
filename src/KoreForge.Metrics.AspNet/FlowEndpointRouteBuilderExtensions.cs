using KoreForge.Metrics.Flow;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace KoreForge.Metrics.AspNet;

public static class FlowEndpointRouteBuilderExtensions
{
    public static IEndpointRouteBuilder MapFlowEndpoints(
        this IEndpointRouteBuilder endpoints,
        string pattern = "/monitoring/flow")
    {
        ArgumentNullException.ThrowIfNull(endpoints);
        ArgumentException.ThrowIfNullOrWhiteSpace(pattern);

        endpoints.MapGet(pattern + "/graphs", (IPipelineFlowMonitor monitor) =>
            monitor.GetGraphs());

        endpoints.MapGet(pattern + "/{graphId}", (IPipelineFlowMonitor monitor, string graphId) =>
        {
            try
            {
                return Results.Ok(monitor.GetSnapshot(graphId));
            }
            catch (ArgumentException)
            {
                return Results.NotFound();
            }
        });

        endpoints.MapGet(pattern + "/{graphId}/nodes/{nodeId}", (IPipelineFlowMonitor monitor, string graphId, string nodeId) =>
        {
            try
            {
                var snapshot = monitor.GetSnapshot(graphId);
                var node = snapshot.Nodes.FirstOrDefault(n => n.NodeId == nodeId);
                if (node is null)
                {
                    return Results.NotFound();
                }

                var children = snapshot.Nodes
                    .Where(n => n.ParentNodeId == nodeId)
                    .OrderBy(n => n.NodeId)
                    .ToList();

                return Results.Ok(new
                {
                    Node = node,
                    Children = children,
                });
            }
            catch (ArgumentException)
            {
                return Results.NotFound();
            }
        });

        endpoints.MapGet(pattern + "/{graphId}/nodes/{nodeId}/children",
            (IPipelineFlowMonitor monitor, string graphId, string nodeId) =>
        {
            try
            {
                var snapshot = monitor.GetSnapshot(graphId);
                var children = snapshot.Nodes
                    .Where(n => n.ParentNodeId == nodeId)
                    .OrderBy(n => n.NodeId)
                    .ToList();

                return Results.Ok(children);
            }
            catch (ArgumentException)
            {
                return Results.NotFound();
            }
        });

        endpoints.MapGet(pattern + "/{graphId}/bottlenecks",
            (IPipelineFlowMonitor monitor, string graphId) =>
        {
            try
            {
                var snapshot = monitor.GetSnapshot(graphId);
                var bottlenecks = snapshot.Nodes
                    .Where(n => n.CurrentInFlight > 0 || n.BacklogCount > 100 || n.P95LatencyMs > 50)
                    .Select(n => new
                    {
                        NodeId = n.NodeId,
                        Name = n.Name,
                        Reason = DetermineBottleneckReason(n),
                        Severity = n.P95LatencyMs > 100 ? "critical" : n.BacklogCount > 100 ? "warning" : "info",
                        CurrentRatePerSecond = n.CurrentRatePerSecond,
                        P95LatencyMs = n.P95LatencyMs,
                        BacklogCount = n.BacklogCount,
                    })
                    .OrderByDescending(b => b.P95LatencyMs)
                    .ToList();

                return Results.Ok(new
                {
                    GeneratedAt = snapshot.GeneratedAt,
                    Bottlenecks = bottlenecks,
                });
            }
            catch (ArgumentException)
            {
                return Results.NotFound();
            }
        });

        return endpoints;
    }

    private static string DetermineBottleneckReason(PipelineNodeSnapshot node)
    {
        if (node.P95LatencyMs > 100 && node.BacklogCount > 100)
            return "High p95 latency and growing backlog";
        if (node.P95LatencyMs > 100)
            return "High p95 latency";
        if (node.BacklogCount > 100)
            return "Growing backlog";
        if (node.CurrentInFlight > 10)
            return "High in-flight count";
        return "Potential bottleneck";
    }
}
