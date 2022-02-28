using System.Collections.Concurrent;

namespace OpenTelemetry.Exporter.ProfileViewer;

/// <summary>
/// A ConcurrentQueue based simple circular buffer implementation.
/// </summary>
internal class CircularBuffer<T> : ICircularBuffer<T>
{
	private readonly ConcurrentQueue<T> m_Queue = new();
	private readonly Func<T, bool> m_ShouldBeExcluded;
	private readonly int m_Size;

	/// <summary>
	/// Initializes a <see cref="CircularBuffer{T}"/>.
	/// </summary>
	/// <param name="size">The size of the circular buffer.</param>
	/// <param name="shouldBeExcluded">Whether or not, an item should not be saved in circular buffer.</param>
	public CircularBuffer(int size = 100, Func<T, bool>? shouldBeExcluded = null)
	{
		m_Size = size;
		m_ShouldBeExcluded = shouldBeExcluded ?? (_ => true);
	}

	/// <summary>
	/// Adds an item to buffer.
	/// </summary>
	/// <param name="item"></param>
	public void Add(T item)
	{
		if (m_Size <= 0)
			return;

		if (m_ShouldBeExcluded(item))
		{
			m_Queue.Enqueue(item);
			if (m_Queue.Count > m_Size)
			{
				_ = m_Queue.TryDequeue(out _);
			}
		}
	}

	System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => m_Queue.GetEnumerator();

	IEnumerator<T> IEnumerable<T>.GetEnumerator() => m_Queue.GetEnumerator();
}
