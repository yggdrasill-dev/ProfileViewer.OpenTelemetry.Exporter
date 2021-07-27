using System;
using OpenTelemetry.Contrib.Extensions.ProfileViewer;
using OpenTelemetry.Trace;

namespace Microsoft.Extensions.DependencyInjection
{
	public static class TracerProviderBuilderExtensions
	{
		public static TracerProviderBuilder AddProfileViewExporter(
			this TracerProviderBuilder builder,
			Action<ProfileViewProcessorBuilder>? configure = null)
		{
			var processorBuiler = new ProfileViewProcessorBuilder();

			processorBuiler.AddFilter(new ProfilePathFilter());

			configure?.Invoke(processorBuiler);

			return builder
				.AddProcessor(new ProfileViewExportProcessor(processorBuiler.ProfileFilters));
		}
	}
}
