using System.Diagnostics;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace OpenTelemetry.Exporter.ProfileViewer.Filters;

public class UrlPathFilter : IProfileFilter
{
    private readonly PathString[] m_Paths;

    public UrlPathFilter(params PathString[] paths)
    {
        m_Paths = paths;
    }

    public bool Filtering(Activity activity)
        => m_Paths.Any(path => activity.DisplayName.StartsWith(path));
}
