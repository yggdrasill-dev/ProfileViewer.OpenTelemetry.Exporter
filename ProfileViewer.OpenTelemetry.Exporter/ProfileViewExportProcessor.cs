using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Diagnostics;
using OpenTelemetry.Exporter.ProfileViewer.Filters;

namespace OpenTelemetry.Exporter.ProfileViewer;

internal class ProfileViewExportProcessor : BaseProcessor<Activity>
{
	public static ICircularBuffer<ProfileSession> SessionBuffer = new CircularBuffer<ProfileSession>();
	private static readonly ConcurrentDictionary<string, ProfileSession> _Store = new();
	private readonly ImmutableArray<IProfileFilter> m_ProfilerFilters;

	public ProfileViewExportProcessor(IEnumerable<IProfileFilter> filters)
	{
		m_ProfilerFilters = filters.ToImmutableArray();
	}

	public override void OnEnd(Activity data)
	{
		if (data.RootId == null)
			return;

		AddSessionSpan(data);

		if (data.Parent == null)
			CompleteSession(data);
	}

	public override void OnStart(Activity data)
	{
		if (data.Parent == null && data.RootId != null)
		{
			_ = _Store.TryAdd(
				data.RootId,
				new ProfileSession
				{
					TraceId = data.RootId
				});
		}
	}

	private static void AddSessionSpan(Activity activity)
	{
		if (_Store.TryGetValue(activity.RootId!, out var session))
		{
			session.AddSpan(new ProfileSpan()
			{
				ParentId = activity.ParentId,
				StartTimeUtc = activity.StartTimeUtc,
				Id = activity.Id!,
				Tags = activity.TagObjects,
				Baggage = activity.Baggage,
				Duration = activity.Duration,
				DisplayName = activity.DisplayName,
				ActivitySourceName = activity.Source.Name
			});
		}
	}

	private void CompleteSession(Activity activity)
	{
		if (_Store.TryGetValue(activity.RootId!, out var session))
		{
			session.RootId = activity.Id!;
			session.Tags = activity.TagObjects;
			session.Baggage = activity.Baggage;
			session.Duration = activity.Duration;
			session.DisplayName = activity.DisplayName;
			session.StartTimeUtc = activity.StartTimeUtc;

			if (!m_ProfilerFilters.Any(filter => filter.Filtering(activity)))
				SessionBuffer.Add(session);

			_ = _Store.TryRemove(activity.RootId!, out _);
		}
	}
}
