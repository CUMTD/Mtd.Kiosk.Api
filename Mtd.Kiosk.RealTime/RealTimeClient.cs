using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mtd.Kiosk.RealTime.Config;
using Mtd.Kiosk.RealTime.Entities;
using System.Text.Json;

namespace Mtd.Kiosk.RealTime;

public class RealTimeClient
{
	public readonly RealTimeClientConfig _config;
	public readonly HttpClient _client;
	public readonly ILogger<RealTimeClient> _logger;

	public RealTimeClient(IOptions<RealTimeClientConfig> config, HttpClient client, ILogger<RealTimeClient> logger)
	{
		ArgumentNullException.ThrowIfNull(config?.Value, nameof(config));
		ArgumentNullException.ThrowIfNull(client, nameof(client));
		ArgumentNullException.ThrowIfNull(logger, nameof(logger));

		_config = config.Value;
		_client = client;
		_logger = logger;
	}

	public async Task<GeneralMessage[]?> GetGeneralMessagesAsync(CancellationToken cancellationToken)
	{

		HttpResponseMessage httpResponseMessage;
		try
		{
			// TODO DEV
			httpResponseMessage = await _client.GetAsync(_config.GmUri, cancellationToken);
			httpResponseMessage.EnsureSuccessStatusCode();

		}
		catch (HttpRequestException ex)
		{
			_logger.LogError(ex, "Error getting general messages from realtime API. Server returned an {code} code.", ex.StatusCode);
			return null;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error getting general messages from realtime API.");
			return null;
		}

		try
		{
			using var contentStream = await httpResponseMessage.Content.ReadAsStreamAsync(cancellationToken);
			var gmResult = await JsonSerializer.DeserializeAsync<GeneralMessage[]>(contentStream, cancellationToken: cancellationToken);
			return gmResult;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error deserializing general messaging");
			return null;
		}
	}

	public async Task<Departure[]?> GetRealTimeForStop(string stopId, CancellationToken cancellationToken)
	{
		HttpResponseMessage httpResponseMessage;
		try
		{
			var url = $"{_config.SmUri?.ToString()}?stopIds={stopId}&previewTime=60";
			httpResponseMessage = await _client.GetAsync(url, cancellationToken);
			httpResponseMessage.EnsureSuccessStatusCode();
		}
		catch (HttpRequestException ex)
		{
			_logger.LogError(ex, "Error getting real-time data for stop {stopId}. Server returned an {code} code.", stopId, ex.StatusCode);
			return null;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error getting real-time data for stop {stopId}", stopId);
			return null;
		}

		try
		{
			using var contentStream = await httpResponseMessage.Content.ReadAsStreamAsync(cancellationToken);

			if (contentStream.Length == 0)
			{
				_logger.LogWarning("Got a response with no content for stop {stopId}", stopId);
				return null;
			}

			var smResult = await JsonSerializer.DeserializeAsync<Departure[]>(contentStream, cancellationToken: cancellationToken);
			return smResult;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error deserializing real-time data for stop {stopId}", stopId);
			return null;
		}
	}
}
