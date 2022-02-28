namespace OpenTelemetry.Exporter.ProfileViewer;

public interface ICircularBuffer<T> : IEnumerable<T>
{
	void Add(T item);
}
