using System.Collections.Immutable;
using System.Diagnostics;

namespace OpenTelemetry.Exporter.ProfileViewer.Filters;

public class FileExtensionFilter : IProfileFilter
{
	private readonly ImmutableArray<string> m_FilterExtensions;

	public FileExtensionFilter(params string[] filterExtensions)
	{
		if (filterExtensions is null)
			throw new ArgumentNullException(nameof(filterExtensions));

		m_FilterExtensions = filterExtensions.ToImmutableArray();
	}

	public bool Filtering(Activity activity)
		=> m_FilterExtensions.Any(
			extension => activity.DisplayName.EndsWith($".{extension.TrimStart('.')}"));
}
