using System.Diagnostics;

namespace OpenTelemetry.Exporter.ProfileViewer.Filters;

public interface IProfileFilter
{
	bool Filtering(Activity activity);
}
