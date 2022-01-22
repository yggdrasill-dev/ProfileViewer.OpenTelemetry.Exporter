﻿using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Exporter.ProfileViewer;

namespace Microsoft.AspNetCore.Builder
{
	public static class EndpointRouteBuilderExtensions
	{
		public static void MapProfileViewer(this IEndpointRouteBuilder endpoints)
		{
			_ = endpoints.MapGet("/profiler/view", async context =>
			{
				var buffer = ProfileViewExportProcessor.SessionBuffer.ToArray();
				// render result list view
				context.Response.ContentType = "text/html";

				var sb = new StringBuilder();
				sb.Append("<head>");
				sb.Append("<title>OpenTelemetry Latest Profiling Results</title>");
				sb.Append(
					"<style>" +
					"th { width: 200px; text-align: left; } " +
					".gray { background-color: #eee; } " +
					".nowrap { white-space: nowrap;padding-right: 20px; vertical-align:top; } " +
					".break-all { word-break: break-all; } " +
					"</style>");
				sb.Append("</head");
				sb.Append("<body>");
				sb.Append("<h1>OpenTelemetry Latest Profiling Results</h1>");

				var tagFilter = context.Request.Query["tag"];
				if (!string.IsNullOrWhiteSpace(tagFilter))
				{
					_ = sb.Append("<div><strong>Filtered by tag:</strong> ")
						.Append(tagFilter)
						.Append("<br/><br /></div>");
				}

				sb.Append("<table>");
				sb.Append("<tr><th class=\"nowrap\">Time (UTC)</th><th class=\"nowrap\">Duration (ms)</th><th style=\"width: 100%\">Activity</th></tr>");
				var latestResults = buffer
					.OrderByDescending(r => r.StartTimeUtc);

				var i = 0;
				foreach (var result in latestResults)
				{
					if (!string.IsNullOrWhiteSpace(tagFilter)
						&& (result.Tags == null
							|| !result.Tags
								.Select(p => p.Key)
								.Contains<string>(tagFilter, StringComparer.OrdinalIgnoreCase)))
					{
						continue;
					}

					_ = sb.Append("<tr");
					if ((i++) % 2 == 1)
						sb.Append(" class=\"gray\"");
					sb.Append("><td class=\"nowrap\">");
					sb.Append(result.StartTimeUtc.ToString("yyyy-MM-ddTHH:mm:ss.FFF"));
					sb.Append("</td><td class=\"nowrap\">");
					sb.Append(result.Duration.TotalMilliseconds.ToString("F2"));
					sb.Append("</td><td class=\"break-all\"><a href=\"/profiler/view/");
					sb.Append(result.TraceId);
					sb.Append("\" target=\"_blank\">");
					sb.Append(result.DisplayName.Replace("\r\n", " "));
					sb.Append("</a></td></tr>");
				}

				_ = sb.Append("</table>")
					.Append("</body>");

				await context.Response.WriteAsync(sb.ToString());
			});

			_ = endpoints.MapGet("/profiler/view/{traceId}", async context =>
			{
				var buffer = ProfileViewExportProcessor.SessionBuffer.ToArray();

				context.Response.ContentType = "text/html";

				var sb = new StringBuilder();
				_ = sb.Append("<head>")
					.Append("<meta charset=\"utf-8\" />")
					.Append("<meta http-equiv=\"X-UA-Compatible\" content=\"IE=edge\" />")
					.Append("<title>OpenTelemetry Profiling Result</title>")
					.Append("<link rel=\"stylesheet\" href=\"./profiler-resources/css\" />")
					.Append("</head")
					.Append("<body>")
					.Append("<h1>OpenTelemetry Profiling Result</h1>");

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
					_ = sb.Append("<div class=\"css-treeview\">");

					// print summary
					_ = sb.Append("<ul>")
						.Append("<li class=\"summary\">")
						.Append(result!.DisplayName.Replace("\r\n", " "))
						.Append("</li>")
						.Append("<li class=\"summary\">")
						.Append("<b>machine: </b>")
						.Append(Environment.MachineName)
						.Append(" &nbsp; ")
						.Append("</li>")
						.Append("</ul>");

					var totalLength = result.Duration.TotalMilliseconds;
					if (totalLength == 0)
					{
						totalLength = 1;
					}
					var factor = 300.0 / totalLength;

					// print ruler
					sb.Append("<ul>");
					sb.Append("<li class=\"ruler\"><span style=\"width:300px\">0</span><span style=\"width:80px\">");
					sb.Append(totalLength.ToString("F2"));
					sb.Append(
						" (ms)</span><span style=\"width:20px\">&nbsp;</span><span style=\"width:60px\">Start</span>" +
						"<span style=\"width:60px\">Duration</span><span style=\"width:20px\">&nbsp;</span><span>Timing Hierarchy</span></li>");
					sb.Append("</ul>");

					// print timings
					_ = sb.Append("<ul class=\"timing\">");
					PrintTimings(result, null, sb, factor);
					_ = sb.Append("</ul>")
						.Append("</div>");

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
						sb.Append("<textarea readonly>");
						sb.Append($"ActivitySource: {span.ActivitySourceName}");
						sb.Append("Tags => \r\n");
						foreach (var keyValue in span.Tags)
						{
							if (keyValue.Value == null)
								continue;

							sb.Append($"{keyValue.Key}:\r\n");
							sb.Append(keyValue.Value.ToString());
							sb.Append("\r\n\r\n");
						}

						sb.Append("Baggage => \r\n");
						foreach (var keyValue in span.Baggage)
						{
							sb.Append($"{keyValue.Key}:\r\n");
							if (keyValue.Value != null)
								sb.Append(keyValue.Value.ToString());
							sb.Append("\r\n\r\n");
						}
						_ = sb.Append("</textarea>")
							.Append(
								"<a href=\"#close\" title=\"Close\" onclick=\"this.parentNode.parentNode.style.display='none'\">Close</a>")
							.Append("</div>")
							.Append("</aside>");
					}
				}
				else
				{
					_ = sb.Append("Specified result does not exist!");
				}

				_ = sb.Append("</body>");

				await context.Response.WriteAsync(sb.ToString());
			});
			_ = endpoints.MapGet("/profiler/view/profiler-resources/icons", async context =>
			{
				var resourceFileProvider = context.RequestServices.GetRequiredService<IResourceFileProvider>();

				context.Response.ContentType = "image/png";

				var iconsStream = resourceFileProvider.GetFileInfo("icons.png").CreateReadStream();
				using var br = new BinaryReader(iconsStream);

				await context.Response.Body.WriteAsync(br.ReadBytes((int)iconsStream.Length), 0, (int)iconsStream.Length);
			});

			_ = endpoints.MapGet("/profiler/view/profiler-resources/css", async context =>
			{
				var resourceFileProvider = context.RequestServices.GetRequiredService<IResourceFileProvider>();

				context.Response.ContentType = "text/css";
				var cssStream = resourceFileProvider.GetFileInfo("treeview_timeline.css").CreateReadStream();
				using var sr = new StreamReader(cssStream);

				await context.Response.WriteAsync(sr.ReadToEnd());
			});
		}

		private static void PrintDataLink(StringBuilder sb, ProfileSpan span)
		{
			if (span.Tags == null || !span.Tags.Any())
				return;

			_ = sb.Append("[<a href=\"#data_")
				.Append(span.Id.ToString())
				.Append("\" onclick=\"document.getElementById('data_")
				.Append(span.Id.ToString())
				.Append("').style.display='block';\" class=\"openModal\">Tags</a>] ");
		}

		private static void PrintTiming(ProfileSession session, ProfileSpan span, StringBuilder sb, double factor)
		{
			_ = sb.Append("<li><span class=\"timing\" style=\"padding-left: ");
			var startMilliseconds = (span.StartTimeUtc - session.StartTimeUtc).TotalMilliseconds;
			var start = Math.Floor(startMilliseconds * factor);
			if (start > 300)
			{
				start = 300;
			}
			_ = sb.Append(start)
				.Append("px\"><span class=\"bar step")
				.Append("\" title=\"")
				.Append(WebUtility.HtmlEncode(span.DisplayName.Replace("\r\n", " ")))
				.Append("\" style=\"width: ");
			var width = (int)Math.Round(span.Duration.TotalMilliseconds * factor);
			if (width > 300)
			{
				width = 300;
			}
			else if (width == 0)
			{
				width = 1;
			}
			_ = sb.Append(width)
				.Append("px\"></span><span class=\"start\">+")
				.Append(startMilliseconds.ToString("F2"))
				.Append("</span><span class=\"duration\">")
				.Append(span.Duration.TotalMilliseconds.ToString("F2"))
				.Append("</span></span>");
			var hasChildTimings = session.Spans.Any(s => s.ParentId == span.Id);
			if (hasChildTimings)
			{
				_ = sb.Append("<input type=\"checkbox\" id=\"t_")
					.Append(span.Id.ToString())
					.Append("\" checked=\"checked\" /><label for=\"t_")
					.Append(span.Id.ToString())
					.Append("\">");
				PrintDataLink(sb, span);
				_ = sb.Append(WebUtility.HtmlEncode(span.DisplayName.Replace("\r\n", " ")))
					.Append("</label>")
					.Append("<ul>");
				PrintTimings(session, span.Id, sb, factor);
				_ = sb.Append("</ul>");
			}
			else
			{
				_ = sb.Append("<span class=\"leaf\">");
				PrintDataLink(sb, span);
				_ = sb.Append(WebUtility.HtmlEncode(span.DisplayName.Replace("\r\n", " ")))
					.Append("</span>");
			}
			_ = sb.Append("</li>");
		}

		private static void PrintTimings(ProfileSession session, string? parentId, StringBuilder sb, double factor)
		{
			var firstLevelSpans = session.Spans
				.Where(s => s.ParentId == parentId)
				.OrderBy(s => s.StartTimeUtc);

			foreach (var span in firstLevelSpans)
				PrintTiming(session, span, sb, factor);
		}
	}
}
