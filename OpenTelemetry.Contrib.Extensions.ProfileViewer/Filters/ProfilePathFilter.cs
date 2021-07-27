using System.Diagnostics;

namespace OpenTelemetry.Contrib.Extensions.ProfileViewer
{
	public class ProfilePathFilter : IProfileFilter
	{
		public bool Filtering(Activity activity)
			=> activity.DisplayName.StartsWith("/profiler/");
	}
}
