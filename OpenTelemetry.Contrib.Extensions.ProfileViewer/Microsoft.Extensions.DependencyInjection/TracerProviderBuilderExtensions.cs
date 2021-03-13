using OpenTelemetry;
using OpenTelemetry.Contrib.Extensions.ProfileViewer;
using OpenTelemetry.Trace;

namespace Microsoft.Extensions.DependencyInjection
{
	public static class TracerProviderBuilderExtensions
	{
		public static TracerProviderBuilder AddProfileViewExporter(this TracerProviderBuilder builder)
		{
			return builder
				.AddProcessor(new SimpleActivityExportProcessor(new ProfileViewExporter()));
		}
	}
}
