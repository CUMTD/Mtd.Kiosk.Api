﻿using CoordinateSharp;
using Microsoft.AspNetCore.Mvc;
using Mtd.Kiosk.Api.Models;
using NodaTime;


namespace Mtd.Kiosk.Api.Controllers;

/// <summary>
/// 
/// </summary>
[ApiController]
[Produces("application/json")]
[Route("time")]
public class TimeController : ControllerBase
{
	private readonly ILogger<TimeController> _logger;

	/// <summary>
	/// </summary>
	/// <param name="logger"></param>
	public TimeController(ILogger<TimeController> logger)
	{
		ArgumentNullException.ThrowIfNull(logger);

		_logger = logger;
	}

	/// <summary>
	/// Returns if dark mode should be enabled based on the current time of day.
	/// </summary>
	/// <returns>True if dark mode should be enabled.</returns>
	[HttpGet("dark-mode")]
	[ProducesResponseType<DarkModeResponseModel>(StatusCodes.Status200OK)]
	[ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
	public ActionResult<DarkModeResponseModel> GetDarkModeStatus()
	{
		var zone = DateTimeZoneProviders.Tzdb["America/Chicago"];
		var zonedDateTime = SystemClock.Instance.GetCurrentInstant().InZone(zone);

		// Convert to UTC
		var utcDateTime = zonedDateTime.ToDateTimeUtc();

		var c = new Coordinate(40.11560, -88.19520, utcDateTime);
		var sunUp = c.CelestialInfo.IsSunUp;

		return new DarkModeResponseModel(!sunUp);
	}
}
