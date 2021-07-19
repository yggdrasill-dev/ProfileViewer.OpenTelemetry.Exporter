using System.Collections.Concurrent;
using System.Diagnostics;

namespace OpenTelemetry.Contrib.Extensions.ProfileViewer
{
	internal class ProfileViewExportProcessor : BaseProcessor<Activity>
	{
		public static ICircularBuffer<ProfileSession> SessionBuffer = new CircularBuffer<ProfileSession>();
		private static ConcurrentDictionary<string, ProfileSession> _Store = new ConcurrentDictionary<string, ProfileSession>();

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

		private void AddSessionSpan(Activity activity)
		{
			if (_Store.TryGetValue(activity.RootId!, out var session))
			{
				session.AddSpan(new ProfileSpan()
				{
					ParentId = activity.ParentId,
					StartTimeUtc = activity.StartTimeUtc,
					Id = activity.Id!,
					Tags = activity.TagObjects,
					Duration = activity.Duration,
					DisplayName = activity.DisplayName
				});
			}
		}

		private void CompleteSession(Activity activity)
		{
			if (_Store.TryGetValue(activity.RootId!, out var session))
			{
				session.RootId = activity.Id!;
				session.Tags = activity.TagObjects;
				session.Duration = activity.Duration;
				session.DisplayName = activity.DisplayName;
				session.StartTimeUtc = activity.StartTimeUtc;

				SessionBuffer.Add(session);

				_ = _Store.TryRemove(activity.RootId!, out _);
			}
		}
	}
}
