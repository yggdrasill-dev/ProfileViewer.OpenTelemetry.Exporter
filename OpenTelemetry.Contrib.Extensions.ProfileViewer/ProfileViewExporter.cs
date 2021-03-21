using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;

namespace OpenTelemetry.Contrib.Extensions.ProfileViewer
{
	internal class ProfileViewExporter : BaseExporter<Activity>
	{
		public static ICircularBuffer<ProfileSession> SessionBuffer = new CircularBuffer<ProfileSession>();
		private static ConcurrentDictionary<string, ProfileSession> _Store = new ConcurrentDictionary<string, ProfileSession>();

		public override ExportResult Export(in Batch<Activity> batch)
		{
			foreach (var activity in batch)
			{
				var rootActivity = FindRootActivity(activity);

				if (rootActivity.Duration > TimeSpan.Zero && !_Store.ContainsKey(activity.RootId!))
				{
					var findedSession = SessionBuffer
						.Where(session => session.RootId == rootActivity.RootId)
						.FirstOrDefault();

					if (findedSession != null)
						findedSession.AddSpan(new ProfileSpan()
						{
							ParentId = activity.ParentId,
							StartTimeUtc = activity.StartTimeUtc,
							Id = activity.Id!,
							Tags = activity.TagObjects,
							Duration = activity.Duration,
							DisplayName = activity.DisplayName
						});

					continue;
				}

				var session = _Store.GetOrAdd(
					activity.RootId!,
					key => new ProfileSession
					{
						TraceId = key
					});

				session.AddSpan(new ProfileSpan()
				{
					ParentId = activity.ParentId,
					StartTimeUtc = activity.StartTimeUtc,
					Id = activity.Id!,
					Tags = activity.TagObjects,
					Duration = activity.Duration,
					DisplayName = activity.DisplayName
				});

				if (activity.Parent == null)
				{
					session.RootId = activity.Id!;
					session.Tags = activity.TagObjects;
					session.Duration = activity.Duration;
					session.DisplayName = activity.DisplayName;
					session.StartTimeUtc = activity.StartTimeUtc;

					SessionBuffer.Add(session);

					_Store.TryRemove(activity.RootId!, out _);
				}
			}

			return ExportResult.Success;
		}

		private Activity FindRootActivity(Activity activity)
		{
			if (activity.Parent == null)
				return activity;

			return FindRootActivity(activity.Parent);
		}
	}
}
