using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace DemoWebApi.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
	private static readonly string[] _Summaries = new[]
	{
		"Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
	};

	private static readonly ActivitySource _Source = new("DemoWebApi.Controllers.WeatherForecastController");

	[HttpGet]
	public IEnumerable<WeatherForecast> Get()
	{
		using var a = _Source.StartActivity("Test");
		_ = (a?.AddTag("test", "123"));

		var rng = new Random();
		return Enumerable.Range(1, 5).Select(index => new WeatherForecast
		{
			Date = DateTime.Now.AddDays(index),
			TemperatureC = rng.Next(-20, 55),
			Summary = _Summaries[rng.Next(_Summaries.Length)]
		})
		.ToArray();
	}
}
