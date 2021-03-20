using System.Collections.Concurrent;
using System.Diagnostics;

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
	}
}
