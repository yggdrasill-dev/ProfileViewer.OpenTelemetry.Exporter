#pragma warning disable CS8618 // 退出建構函式時，不可為 Null 的欄位必須包含非 Null 值。請考慮宣告為可為 Null。

using System;
using System.Collections.Generic;

namespace OpenTelemetry.Exporter.ProfileViewer
{
	internal class ProfileSpan
	{
		public string DisplayName { get; internal set; }

		public TimeSpan Duration { get; internal set; }

		public string Id { get; internal set; }

		public string? ParentId { get; internal set; }

		public DateTime StartTimeUtc { get; internal set; }

		public IEnumerable<KeyValuePair<string, object?>> Tags { get; internal set; }
	}
}

#pragma warning restore CS8618 // 退出建構函式時，不可為 Null 的欄位必須包含非 Null 值。請考慮宣告為可為 Null。
