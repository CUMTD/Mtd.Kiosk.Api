using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Mtd.Kiosk.Api.Models.Enums;
using Mtd.Kiosk.Core.Entities;
using Mtd.Kiosk.Core.Repositories;
using Mtd.Kiosk.RealTime;
using Mtd.Stopwatch.Core.Repositories.Transit;

namespace Mtd.Kiosk.Api.Controllers;

/// <summary>
/// A collection of endpoints for fetching departure data for display on kiosks.
/// </summary>
[Route("departures")]
[ApiController]
public class DeparturesController : ControllerBase
{
	private readonly RealTimeClient _realTimeClient;
	private readonly IHeartbeatRepository _heartbeatRepository;
	private readonly IRouteRepository<IReadOnlyCollection<Stopwatch.Core.Entities.Transit.Route>> _routeRepository;
	private readonly IMemoryCache _cache;
	private readonly ILogger<DeparturesController> _logger;

	/// <summary>
	/// Constructor for the Departures controller.
	/// </summary>
	/// <param name="realTimeClient"></param>
	/// <param name="heartbeatRepository"></param>
	/// <param name="routeRepository"></param>
	/// <param name="cache"></param>
	/// <param name="logger"></param>
	public DeparturesController(
		RealTimeClient realTimeClient,
		IHeartbeatRepository heartbeatRepository,
		IRouteRepository<IReadOnlyCollection<Stopwatch.Core.Entities.Transit.Route>> routeRepository,
		IMemoryCache cache,
		ILogger<DeparturesController> logger)
	{
		ArgumentNullException.ThrowIfNull(realTimeClient, nameof(realTimeClient));
		ArgumentNullException.ThrowIfNull(heartbeatRepository, nameof(heartbeatRepository));
		ArgumentNullException.ThrowIfNull(routeRepository, nameof(routeRepository));
		ArgumentNullException.ThrowIfNull(cache, nameof(cache));
		ArgumentNullException.ThrowIfNull(logger, nameof(logger));

		_realTimeClient = realTimeClient;
		_heartbeatRepository = heartbeatRepository;
		_routeRepository = routeRepository;
		_cache = cache;
		_logger = logger;
	}

	/// <summary>
	/// Get the next departures for an LED sign.
	/// </summary>
	/// <param name="stopId">The stop Id to fetch departures for</param>
	/// <param name="kioskId">The kiosk Id, for logging a heartbeat</param>
	/// <param name="cancellationToken"></param>
	/// <returns>An array of LED departures</returns>
	[HttpGet("{stopId}/led")]
	[ProducesResponseType<IEnumerable<LedDeparture>>(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	[ProducesResponseType(StatusCodes.Status500InternalServerError)]
	public async Task<ActionResult<IEnumerable<LedDeparture>>> GetLedDepartures(string stopId, [FromQuery] string kioskId, CancellationToken cancellationToken)
	{
		var heartbeat = await _heartbeatRepository.AddAsync(new Heartbeat(kioskId, HeartbeatType.LED), cancellationToken);
		await _heartbeatRepository.CommitChangesAsync(cancellationToken);

		var result = await _realTimeClient.GetRealTimeForStop(stopId, cancellationToken);

		if (result == null)
		{
			return StatusCode(StatusCodes.Status500InternalServerError);
		}

		var departures = result
			.OrderBy(x => x.MinutesTillDeparture)
			.ThenBy(x => x.RouteSortNumber)
			.GroupBy(x => x.FriendlyRouteName)
			.Select(x => x.First())
			.Select(x => new LedDeparture(x));

		return Ok(departures);
	}

	/// <summary>
	/// Get the next departures for an LCD screen.
	/// </summary>
	/// <param name="stopId">The stop Id to fetch departures for</param>
	/// <param name="kioskId">The kiosk Id, for logging a heartbeat</param>
	/// <param name="cancellationToken"></param>
	/// <returns>An LcdDepartureResponseModel object</returns>
	[HttpGet("{stopId}/lcd")]
	[ProducesResponseType<LcdDepartureResponseModel>(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	[ProducesResponseType(StatusCodes.Status500InternalServerError)]
	public async Task<ActionResult<LcdDepartureResponseModel>> GetLcdDepartures(string stopId, [FromQuery] string? kioskId, CancellationToken cancellationToken)
	{
		if (kioskId != null) // only log heartbeat if kioskId is provided (only kiosks will send this param)
		{
			var heartbeat = await _heartbeatRepository.AddAsync(new Heartbeat(kioskId, HeartbeatType.LCD), cancellationToken);
			await _heartbeatRepository.CommitChangesAsync(cancellationToken);

		}

		LcdGeneralMessage? lcdGeneralMessage = null;

		var currentGeneralMessages = await _realTimeClient.GetGeneralMessagesAsync(cancellationToken);
		if (currentGeneralMessages != null && currentGeneralMessages.Length > 0)
		{

			var generalMessage = currentGeneralMessages.FirstOrDefault(x => x.StopIds != null && x.StopIds.Contains(stopId));

			if (generalMessage != null)
			{
				if (generalMessage.BlockRealtime)
				{
					return new LcdDepartureResponseModel(new List<LcdDeparture>(), new LcdGeneralMessage(generalMessage));

				}
				else
				{

					lcdGeneralMessage = new LcdGeneralMessage(generalMessage);
				}
			}
		}

		var departures = await _realTimeClient.GetRealTimeForStop(stopId, cancellationToken);
		if (departures == null)
		{
			return StatusCode(StatusCodes.Status500InternalServerError);
		}
		else
		{
			var routes = await GetCacheAllRoutes(cancellationToken);
			if (routes == null)
			{
				_logger.LogError("Could not load GTFS routes from DB so cannot show departures.");
				return StatusCode(StatusCodes.Status500InternalServerError);
			}

			var groupedDepartures = departures
				.OrderBy(departure => departure.MinutesTillDeparture)
				.ThenBy(departure => departure.RouteSortNumber)
				.ThenBy(departure => departure.RouteNumber)
				.ToLookup(departure => routes.FirstOrDefault(route => route.Id == departure.RouteId)?.PublicRouteId ?? "UNKNOWN_PUBLIC_ROUTE_ID")
				.Take(7)
				.OrderBy(d => d.First().RouteNumber);

			var lcdDepartures = new List<LcdDeparture>();

			foreach (var group in groupedDepartures)
			{
				var lcdDepartureTimes = new List<LcdDepartureTime>();

				for (var i = 0; i < 3 && i < group.Count(); i++)
				{
					lcdDepartureTimes.Add(new LcdDepartureTime(group.ElementAt(i)));
				}

				var publicRoute = routes.First(route => route.PublicRouteId != null && route.PublicRouteId == group.Key).PublicRoute;

				// public route should never be null here since we check for it in our first statement above.
				var lcdDeparture = new LcdDeparture(publicRoute!, lcdDepartureTimes, group.First().Direction);

				lcdDepartures.Add(lcdDeparture);
			}

			return new LcdDepartureResponseModel(lcdDepartures.OrderBy(d => d.SortOrder), lcdGeneralMessage);

		}
	}
	private async Task<IReadOnlyCollection<Stopwatch.Core.Entities.Transit.Route>?> GetCacheAllRoutes(CancellationToken cancellationToken)
	{
		const string CACHE_KEY = "GtfsRoutes";

		if (!_cache.TryGetValue(CACHE_KEY, out IReadOnlyCollection<Stopwatch.Core.Entities.Transit.Route>? publicRoutes))
		{
			try
			{
				publicRoutes = await _routeRepository.GetAllWithDirectionAsync(cancellationToken);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error getting public routes from database");
				return null;
			}

			_cache.Set(CACHE_KEY, publicRoutes, TimeSpan.FromMinutes(15));
		}

		return publicRoutes;
	}
}
