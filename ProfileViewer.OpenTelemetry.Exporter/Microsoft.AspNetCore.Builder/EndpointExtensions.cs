#if NETCOREAPP3_1_OR_GREATER

#if NETCOREAPP3_1
using Microsoft.AspNetCore.Routing;
#endif
using OpenTelemetry.Exporter.ProfileViewer;

namespace Microsoft.AspNetCore.Builder;

public static class EndpointRouteExtensions
{
    public static void MapProfileViewer(this IEndpointRouteBuilder endpoints)
    {
        _ = endpoints.MapGet(
            "/profiler/view",
            ProfilerRequestDelegates.MainPathInvokeAsync);

        _ = endpoints.MapGet(
            "/profiler/view/{traceId}",
            ProfilerRequestDelegates.DetailPathInvokeAsync);

        _ = endpoints.MapGet(
            "/profiler/view/profiler-resources/icons",
            ProfilerRequestDelegates.IconPathInvokeAsync);

        _ = endpoints.MapGet(
            "/profiler/view/profiler-resources/css",
            ProfilerRequestDelegates.CssPathInvokeAsync);
    }
}

#endif
