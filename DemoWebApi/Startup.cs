using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry.Exporter.ProfileViewer.Filters;
using OpenTelemetry.Trace;

namespace DemoWebApi
{
	public class Startup
	{
		public IConfiguration Configuration { get; }

		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}

			app.UseHttpsRedirection();

			app.UseRouting();

			app.UseAuthorization();

			app.UseEndpoints(endpoints =>
			{
				endpoints.MapControllers();
				endpoints.MapProfileViewer();
			});
		}

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
			services.AddControllers();

			services.AddOpenTelemetryTracing(
				(builder) => builder
					.AddSource("DemoWebApi.*")
					.AddAspNetCoreInstrumentation()
					.AddHttpClientInstrumentation()
					.AddProfileViewExporter(builder =>
					{
						builder.AddFilter(new FileExtensionFilter("ico"));
					}));
			services.AddProfileViewer();
		}
	}
}
