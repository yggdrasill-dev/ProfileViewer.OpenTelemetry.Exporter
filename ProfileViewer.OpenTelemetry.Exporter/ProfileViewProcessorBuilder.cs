using OpenTelemetry.Exporter.ProfileViewer.Filters;

namespace OpenTelemetry.Exporter.ProfileViewer;

public class ProfileViewProcessorBuilder
{
	private readonly List<IProfileFilter> m_ProfileFilters = new();

	internal IEnumerable<IProfileFilter> ProfileFilters => m_ProfileFilters.ToArray();

	public ProfileViewProcessorBuilder AddFilter(IProfileFilter filter)
	{
		m_ProfileFilters.Add(filter);

		return this;
	}
}
