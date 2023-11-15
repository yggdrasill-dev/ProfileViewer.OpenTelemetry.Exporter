using System.Diagnostics;

namespace OpenTelemetry.Exporter.ProfileViewer.Filters;

public class ProfilePathFilter : IProfileFilter
{
    public bool Filtering(Activity activity)
        => activity.DisplayName.StartsWith("/profiler/");
}
