using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;

namespace OpenTelemetry.Contrib.Extensions.ProfileViewer
{
	public class FileExtensionFilter : IProfileFilter
	{
		private ImmutableArray<string> m_FilterExtensions;

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
}
