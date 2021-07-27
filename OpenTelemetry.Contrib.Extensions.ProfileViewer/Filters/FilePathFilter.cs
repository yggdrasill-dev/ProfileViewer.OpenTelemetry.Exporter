using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;

namespace OpenTelemetry.Contrib.Extensions.ProfileViewer.Filters
{
	public class FilePathFilter : IProfileFilter
	{
		private ImmutableArray<string> m_Paths;

		public FilePathFilter(params string[] paths)
		{
			m_Paths = paths.ToImmutableArray();
		}

		public bool Filtering(Activity activity)
			=> m_Paths.Any(path => activity.DisplayName.StartsWith(path));
	}
}
