using OpenTelemetry.Exporter.ProfileViewer;

namespace Microsoft.Extensions.DependencyInjection
{
	public static class ServiceCollectionExtensions
	{
		public static IServiceCollection AddProfileViewer(this IServiceCollection services)
		{
			return services.AddTransient<IResourceFileProvider, ResourceFileProvider>();
		}
	}
}
