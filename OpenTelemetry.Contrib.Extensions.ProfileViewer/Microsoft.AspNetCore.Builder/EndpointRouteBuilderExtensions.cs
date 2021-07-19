using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Contrib.Extensions.ProfileViewer;

namespace Microsoft.AspNetCore.Builder
{
	public static class EndpointRouteBuilderExtensions
	{
		public static void MapProfileViewer(this IEndpointRouteBuilder endpoints)
		{
			endpoints.MapGet("/profiler/view", async context =>
			{
				var buffer = ProfileViewExportProcessor.SessionBuffer.ToArray();
				// render result list view
				context.Response.ContentType = "text/html";

				var sb = new StringBuilder();
				sb.Append("<head>");
				sb.Append("<title>OpenTelemetry Latest Profiling Results</title>");
				sb.Append("<style>th { width: 200px; text-align: left; } .gray { background-color: #eee; } .nowrap { white-space: nowrap;padding-right: 20px; vertical-align:top; } </style>");
				sb.Append("</head");
				sb.Append("<body>");
				sb.Append("<h1>OpenTelemetry Latest Profiling Results</h1>");

				var tagFilter = context.Request.Query["tag"];
				if (!string.IsNullOrWhiteSpace(tagFilter))
				{
					sb.Append("<div><strong>Filtered by tag:</strong> ");
					sb.Append(tagFilter);
					sb.Append("<br/><br /></div>");
				}

				sb.Append("<table>");
				sb.Append("<tr><th class=\"nowrap\">Time (UTC)</th><th class=\"nowrap\">Duration (ms)</th><th>Activity</th></tr>");
				var latestResults = buffer
					.OrderByDescending(r => r.StartTimeUtc);

				var i = 0;
				foreach (var result in latestResults)
				{
					if (!string.IsNullOrWhiteSpace(tagFilter) &&
						(result.Tags == null || !result.Tags.Select(p => p.Key).Contains<string>(tagFilter, StringComparer.OrdinalIgnoreCase)))
					{
						continue;
					}

					sb.Append("<tr");
					if ((i++) % 2 == 1)
					{
						sb.Append(" class=\"gray\"");
					}

					sb.Append("><td class=\"nowrap\">");
					sb.Append(result.StartTimeUtc.ToString("yyyy-MM-ddTHH:mm:ss.FFF"));
					sb.Append("</td><td class=\"nowrap\">");
					sb.Append(result.Duration.TotalMilliseconds);
					sb.Append("</td><td><a href=\"/profiler/view/");
					sb.Append(result.TraceId);
					sb.Append("\" target=\"_blank\">");
					sb.Append(result.DisplayName.Replace("\r\n", " "));
					sb.Append("</a></td></tr>");
				}

				sb.Append("</table>");

				sb.Append("</body>");

				await context.Response.WriteAsync(sb.ToString());
			});

			endpoints.MapGet("/profiler/view/{traceId}", async context =>
			{
				var buffer = ProfileViewExportProcessor.SessionBuffer.ToArray();

				context.Response.ContentType = "text/html";

				var sb = new StringBuilder();
				sb.Append("<head>");
				sb.Append("<meta charset=\"utf-8\" />");
				sb.Append("<meta http-equiv=\"X-UA-Compatible\" content=\"IE=edge\" />");
				sb.Append("<title>OpenTelemetry Profiling Result</title>");
				sb.Append("<link rel=\"stylesheet\" href=\"./profiler-resources/css\" />");
				sb.Append("</head");
				sb.Append("<body>");
				sb.Append("<h1>OpenTelemetry Profiling Result</h1>");

				var traceId = (string?)context.Request.RouteValues["traceId"];

				bool TryFindSession(string? id, out ProfileSession? session)
				{
					session = null;

					if (string.IsNullOrEmpty(id))
						return false;

					session = buffer.Where(session => session.TraceId == traceId).FirstOrDefault();

					return session != null;
				}

				if (TryFindSession(traceId, out var result))
				{
					// render result tree
					sb.Append("<div class=\"css-treeview\">");

					// print summary
					sb.Append("<ul>");
					sb.Append("<li class=\"summary\">");
					sb.Append(result!.DisplayName.Replace("\r\n", " "));
					sb.Append("</li>");
					sb.Append("<li class=\"summary\">");
					sb.Append("<b>machine: </b>");
					sb.Append(Environment.MachineName);
					sb.Append(" &nbsp; ");
					sb.Append("</li>");
					sb.Append("</ul>");

					var totalLength = result.Duration.TotalMilliseconds;
					if (totalLength == 0)
					{
						totalLength = 1;
					}
					var factor = 300.0 / totalLength;

					// print ruler
					sb.Append("<ul>");
					sb.Append("<li class=\"ruler\"><span style=\"width:300px\">0</span><span style=\"width:80px\">");
					sb.Append(totalLength);
					sb.Append(
						" (ms)</span><span style=\"width:20px\">&nbsp;</span><span style=\"width:60px\">Start</span><span style=\"width:60px\">Duration</span><span style=\"width:20px\">&nbsp;</span><span>Timing Hierarchy</span></li>");
					sb.Append("</ul>");

					// print timings
					sb.Append("<ul class=\"timing\">");
					PrintTimings(result, null, sb, factor);
					sb.Append("</ul>");
					sb.Append("</div>");

					// print timing data popups
					foreach (var span in result.Spans)
					{
						if (span.Tags == null || !span.Tags.Any()) continue;

						sb.Append("<aside id=\"data_");
						sb.Append(span.Id.ToString());
						sb.Append("\" style=\"display:none\" class=\"modal\">");
						sb.Append("<div>");
						sb.Append("<h4><code>");
						sb.Append(span.DisplayName.Replace("\r\n", " "));
						sb.Append("</code></h4>");
						sb.Append("<textarea>");
						foreach (var keyValue in span.Tags)
						{
							if (keyValue.Value == null)
								continue;

							sb.Append(@$"{keyValue.Key}");
							sb.Append(":\r\n");

							sb.Append(keyValue.Value.ToString());
							sb.Append("\r\n\r\n");
						}
						sb.Append("</textarea>");
						sb.Append(
							"<a href=\"#close\" title=\"Close\" onclick=\"this.parentNode.parentNode.style.display='none'\">Close</a>");
						sb.Append("</div>");
						sb.Append("</aside>");
					}
				}
				else
				{
					sb.Append("Specified result does not exist!");
				}

				sb.Append("</body>");

				await context.Response.WriteAsync(sb.ToString());
			});
			endpoints.MapGet("/profiler/view/profiler-resources/icons", async context =>
			{
				var resourceFileProvider = context.RequestServices.GetRequiredService<IResourceFileProvider>();

				context.Response.ContentType = "image/png";

				var iconsStream = resourceFileProvider.GetFileInfo("icons.png").CreateReadStream();
				using var br = new BinaryReader(iconsStream);

				await context.Response.Body.WriteAsync(br.ReadBytes((int)iconsStream.Length), 0, (int)iconsStream.Length);
			});

			endpoints.MapGet("/profiler/view/profiler-resources/css", async context =>
			{
				var resourceFileProvider = context.RequestServices.GetRequiredService<IResourceFileProvider>();

				context.Response.ContentType = "text/css";
				var cssStream = resourceFileProvider.GetFileInfo("treeview_timeline.css").CreateReadStream();
				using var sr = new StreamReader(cssStream);

				await context.Response.WriteAsync(sr.ReadToEnd());
			});
		}

		private static void PrintTimings(ProfileSession session, string? parentId, StringBuilder sb, double factor)
		{
			var firstLevelSpans = session.Spans.Where(s => s.ParentId == parentId);
			foreach (var span in firstLevelSpans)
			{
				PrintTiming(session, span, sb, factor);
			}
		}

		private static void PrintTiming(ProfileSession session, ProfileSpan span, StringBuilder sb, double factor)
		{
			sb.Append("<li><span class=\"timing\" style=\"padding-left: ");
			var startMilliseconds = (span.StartTimeUtc - session.StartTimeUtc).TotalMilliseconds;
			var start = Math.Floor(startMilliseconds * factor);
			if (start > 300)
			{
				start = 300;
			}
			sb.Append(start);
			sb.Append("px\"><span class=\"bar step");
			sb.Append("\" title=\"");
			sb.Append(WebUtility.HtmlEncode(span.DisplayName.Replace("\r\n", " ")));
			sb.Append("\" style=\"width: ");
			var width = (int)Math.Round(span.Duration.TotalMilliseconds * factor);
			if (width > 300)
			{
				width = 300;
			}
			else if (width == 0)
			{
				width = 1;
			}
			sb.Append(width);
			sb.Append("px\"></span><span class=\"start\">+");
			sb.Append(startMilliseconds);
			sb.Append("</span><span class=\"duration\">");
			sb.Append(span.Duration.TotalMilliseconds);
			sb.Append("</span></span>");
			var hasChildTimings = session.Spans.Any(s => s.ParentId == span.Id);
			if (hasChildTimings)
			{
				sb.Append("<input type=\"checkbox\" id=\"t_");
				sb.Append(span.Id.ToString());
				sb.Append("\" checked=\"checked\" /><label for=\"t_");
				sb.Append(span.Id.ToString());
				sb.Append("\">");
				PrintDataLink(sb, span);
				sb.Append(WebUtility.HtmlEncode(span.DisplayName.Replace("\r\n", " ")));
				sb.Append("</label>");
				sb.Append("<ul>");
				PrintTimings(session, span.Id, sb, factor);
				sb.Append("</ul>");
			}
			else
			{
				sb.Append("<span class=\"leaf\">");
				PrintDataLink(sb, span);
				sb.Append(WebUtility.HtmlEncode(span.DisplayName.Replace("\r\n", " ")));
				sb.Append("</span>");
			}
			sb.Append("</li>");
		}

		private static void PrintDataLink(StringBuilder sb, ProfileSpan span)
		{
			if (span.Tags == null || !span.Tags.Any())
				return;

			sb.Append("[<a href=\"#data_");
			sb.Append(span.Id.ToString());
			sb.Append("\" onclick=\"document.getElementById('data_");
			sb.Append(span.Id.ToString());
			sb.Append("').style.display='block';\" class=\"openModal\">Tags</a>] ");
		}
	}
}
