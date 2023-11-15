using System;
using System.Diagnostics;
using System.Linq;

namespace OpenTelemetry.Exporter.ProfileViewer.Filters;

public class FileExtensionFilter : IProfileFilter
{
    private readonly string[] m_FilterExtensions;

    public FileExtensionFilter(params string[] filterExtensions)
    {
#if NET8_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(filterExtensions);
#else
		if (filterExtensions is null)
			throw new ArgumentNullException(nameof(filterExtensions));
#endif

        m_FilterExtensions = filterExtensions;
    }

    public bool Filtering(Activity activity)
        => m_FilterExtensions.Any(
            extension => activity.DisplayName.EndsWith($".{extension.TrimStart('.')}"));
}
