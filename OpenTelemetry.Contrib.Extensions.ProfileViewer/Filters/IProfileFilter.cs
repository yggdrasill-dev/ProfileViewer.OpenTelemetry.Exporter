using System.Diagnostics;

namespace OpenTelemetry.Contrib.Extensions.ProfileViewer
{
	public interface IProfileFilter
	{
		bool Filtering(Activity activity);
	}
}
