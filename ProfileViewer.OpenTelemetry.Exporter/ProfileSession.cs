#pragma warning disable CS8618 // 退出建構函式時，不可為 Null 的欄位必須包含非 Null 值。請考慮宣告為可為 Null。

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace OpenTelemetry.Exporter.ProfileViewer
{
	internal class ProfileSession
	{
		private ConcurrentBag<ProfileSpan> m_Spans = new ConcurrentBag<ProfileSpan>();
		public string DisplayName { get; internal set; } = string.Empty;

		public TimeSpan Duration { get; internal set; }

		public string RootId { get; internal set; }

		public IEnumerable<ProfileSpan> Spans => m_Spans.ToArray();

		public DateTime StartTimeUtc { get; internal set; }

		public IEnumerable<KeyValuePair<string, object?>> Tags { get; internal set; } = new KeyValuePair<string, object?>[0];

		public string TraceId { get; internal set; } = string.Empty;

		public void AddSpan(ProfileSpan span)
		{
			m_Spans.Add(span);
		}
	}
}

#pragma warning restore CS8618 // 退出建構函式時，不可為 Null 的欄位必須包含非 Null 值。請考慮宣告為可為 Null。
