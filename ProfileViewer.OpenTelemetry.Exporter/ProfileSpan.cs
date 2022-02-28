namespace OpenTelemetry.Exporter.ProfileViewer;

internal class ProfileSpan
{
	public IEnumerable<KeyValuePair<string, string?>> Baggage { get; internal set; } = Array.Empty<KeyValuePair<string, string?>>();

	public string DisplayName { get; internal set; } = default!;

	public TimeSpan Duration { get; internal set; }

	public string Id { get; internal set; } = default!;

	public string? ParentId { get; internal set; }

	public DateTime StartTimeUtc { get; internal set; }

	public string ActivitySourceName { get; internal set; } = default!;

	public IEnumerable<KeyValuePair<string, object?>> Tags { get; internal set; } = Array.Empty<KeyValuePair<string, object?>>();
}
