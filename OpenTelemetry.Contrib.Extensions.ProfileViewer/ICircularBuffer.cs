using System.Collections.Generic;

namespace OpenTelemetry.Contrib.Extensions.ProfileViewer
{
	public interface ICircularBuffer<T> : IEnumerable<T>
	{
		void Add(T item);
	}
}
