using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace OpenTelemetry.Contrib.Extensions.ProfileViewer.Filters
{
	public class UrlPathFilter : IProfileFilter
	{
		private ImmutableArray<PathString> m_Paths;

		public UrlPathFilter(params PathString[] paths)
		{
			m_Paths = paths.ToImmutableArray();
		}

		public bool Filtering(Activity activity)
			=> m_Paths.Any(path => activity.DisplayName.StartsWith(path));
	}
}
