using System.Collections.Generic;

namespace OpenTelemetry.Contrib.Extensions.ProfileViewer
{
	public class ProfileViewProcessorBuilder
	{
		private List<IProfileFilter> m_ProfileFilters = new List<IProfileFilter>();

		internal IEnumerable<IProfileFilter> ProfileFilters => m_ProfileFilters.ToArray();

		public ProfileViewProcessorBuilder AddFilter(IProfileFilter filter)
		{
			m_ProfileFilters.Add(filter);

			return this;
		}
	}
}
