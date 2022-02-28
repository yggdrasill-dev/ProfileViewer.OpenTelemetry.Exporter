using System.Collections.Concurrent;

namespace OpenTelemetry.Exporter.ProfileViewer;

internal class ProfileSession
{
	private readonly ConcurrentBag<ProfileSpan> m_Spans = new();

	public IEnumerable<KeyValuePair<string, string?>> Baggage { get; internal set; } = Array.Empty<KeyValuePair<string, string?>>();

	public string DisplayName { get; internal set; } = string.Empty;

	public TimeSpan Duration { get; internal set; }

	public string RootId { get; internal set; } = default!;

	public IEnumerable<ProfileSpan> Spans => m_Spans.ToArray();

	public DateTime StartTimeUtc { get; internal set; }

	public IEnumerable<KeyValuePair<string, object?>> Tags { get; internal set; } = Array.Empty<KeyValuePair<string, object?>>();

	public string TraceId { get; internal set; } = string.Empty;

	public void AddSpan(ProfileSpan span)
	{
		m_Spans.Add(span);
	}
}
