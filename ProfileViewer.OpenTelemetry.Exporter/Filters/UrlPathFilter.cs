using System.Collections.Immutable;
using System.Diagnostics;

namespace OpenTelemetry.Exporter.ProfileViewer.Filters;

public class UrlPathFilter : IProfileFilter
{
	private readonly ImmutableArray<PathString> m_Paths;

	public UrlPathFilter(params PathString[] paths)
	{
		m_Paths = paths.ToImmutableArray();
	}

	public bool Filtering(Activity activity)
		=> m_Paths.Any(path => activity.DisplayName.StartsWith(path));
}
