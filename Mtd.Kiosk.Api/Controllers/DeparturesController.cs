using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Mtd.Kiosk.Api.Models;
using Mtd.Kiosk.Core.Entities;
using Mtd.Kiosk.Core.Repositories;
using Mtd.Kiosk.RealTime;
using Mtd.Kiosk.RealTime.Entities;
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
	public async Task<ActionResult<IEnumerable<LedDeparture>>> GetLedDepartures(string stopId, [FromQuery] string? kioskId, CancellationToken cancellationToken)
	{
		Departure[]? realTimeClientDepartures;
		try
		{
			realTimeClientDepartures = await _realTimeClient.GetRealTimeForStop(stopId, cancellationToken);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Failed to get deparutres from real-time client for {stopId}.", stopId);
			return StatusCode(StatusCodes.Status500InternalServerError);
		}

		if (realTimeClientDepartures == null)
		{
			_logger.LogError("Real-time client returned null for {stopId}.", stopId);
			return StatusCode(StatusCodes.Status500InternalServerError);
		}

		// the following code takes all departures and does some sorting and filtering.
		// it also converts the departures to LedDeparture response object.
		// it groups the departures by route and direction, then selects the first departure for each group.
		// this avoids using valuable LED real-estate to show an upcoming departure for a route that
		// already has a bus coming sooner.
		var departures = realTimeClientDepartures
			.OrderBy(departure => departure.MinutesTillDeparture)
			.ThenBy(departure => departure.RouteSortNumber)
			.GroupBy(departure => new { departure.FriendlyRouteName, departure.Direction })
			.Select(group => group.First())
			.Select(departure => new LedDeparture(departure));

		// log the heartbeat. This is done in a "fire and forget" pattern.
		LogHeartbeat(HeartbeatType.LED, kioskId);

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
		// fetch new general messages from the real-time API.
		GeneralMessage[]? currentGeneralMessages = null;
		try
		{
			currentGeneralMessages = await _realTimeClient.GetGeneralMessagesAsync(cancellationToken);
		}
		catch (Exception ex)
		{
			_logger.LogWarning(ex, "Failed to get general messages from real-time client.");
		}

		LcdGeneralMessage? lcdGeneralMessage;
		if (currentGeneralMessages != null && currentGeneralMessages.Length > 0)
		{

			var generalMessage = currentGeneralMessages
				.OrderByDescending(gm => gm.BlockRealtime)
				.FirstOrDefault(gm => gm.StopIds != null && gm.StopIds.Contains(stopId));

			if (generalMessage != null)
			{
				if (generalMessage.BlockRealtime)
				{
					return new LcdDepartureResponseModel(new List<LcdDepartureGroup>(), new LcdGeneralMessage(generalMessage));

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

			var lcdDepartures = new List<LcdDepartureGroup>();

			foreach (var group in groupedDepartures)
			{
				var lcdDepartureTimes = new List<LcdDepartureTime>();

				for (var i = 0; i < 3 && i < group.Count(); i++)
				{
					lcdDepartureTimes.Add(new LcdDepartureTime(group.ElementAt(i)));
				}

				var publicRoute = routes.First(route => route.PublicRouteId != null && route.PublicRouteId == group.Key).PublicRoute;

				// public route should never be null here since we check for it in our first statement above.
				var lcdDeparture = new LcdDepartureGroup(publicRoute!, lcdDepartureTimes, group.First().Direction);

				lcdDepartures.Add(lcdDeparture);
			}

			LogHeartbeat(HeartbeatType.LCD, kioskId);

			return new LcdDepartureResponseModel(lcdDepartures.OrderBy(d => d.SortOrder), lcdGeneralMessage);

		}
	}

	#region Helpers

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

	/// <summary>
	/// Logs the kiosk heartbeat using a "fire and forget" pattern.
	/// </summary>
	/// <param name="type">The type of component to log a heartbeat for.</param>
	/// <param name="kioskId">The ID of the kiosk to log. If this is null, this method will not do anything.</param>
	private void LogHeartbeat(HeartbeatType type, string? kioskId)
	{
		if (string.IsNullOrWhiteSpace(kioskId))
		{
			_logger.LogDebug("No Kiosk ID provided, not logging heartbeat of type {heartbeatType}", type);
			return;
		}

		var heartbeat = new Heartbeat(kioskId, type);
		try
		{
			_ = _heartbeatRepository
				.AddAsync(heartbeat, CancellationToken.None)
				.ContinueWith(task =>
				{
					if (task.Exception != null)
					{
						_logger.LogWarning(task.Exception, "Failed to add heartbeat {heartbeatType} for kiosk {kioskId}", type, kioskId ?? "unknown");
					}
				}, TaskContinuationOptions.OnlyOnFaulted);

			_ = _heartbeatRepository.CommitChangesAsync(CancellationToken.None)
				.ContinueWith(task =>
				{
					if (task.Exception != null)
					{
						_logger.LogWarning(task.Exception, "Failed to commit heartbeat {heartbeatType} for kiosk {kioskId}", type, kioskId ?? "unknown");
					}
				}, TaskContinuationOptions.OnlyOnFaulted);
		}
		catch (Exception ex)
		{
			// Handle or log unexpected synchronous exceptions
			_logger.LogError(ex, "Unexpected exception while logging heartbeat.");
		}
	}

	#endregion Helpers
}
