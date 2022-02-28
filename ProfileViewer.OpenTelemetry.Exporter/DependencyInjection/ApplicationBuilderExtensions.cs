#if NETSTANDARD2_0

using OpenTelemetry.Exporter.ProfileViewer;

namespace Microsoft.Extensions.DependencyInjection;

public static class ApplicationBuilderExtensions
{
	public static IApplicationBuilder UseProfileViewer(this IApplicationBuilder app)
		=> app.UseRouter(builder => builder
			.MapGet(
				"/profiler/view",
				ProfilerRequestDelegates.MainPathInvokeAsync)
			.MapGet(
				"/profiler/view/{traceId}",
				ProfilerRequestDelegates.DetailPathInvokeAsync)
			.MapGet(
				"/profiler/view/profiler-resources/icons",
				ProfilerRequestDelegates.IconPathInvokeAsync)
			.MapGet(
				"/profiler/view/profiler-resources/css",
				ProfilerRequestDelegates.CssPathInvokeAsync));
}

#endif
