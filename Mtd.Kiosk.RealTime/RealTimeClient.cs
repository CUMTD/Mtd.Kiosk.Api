using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mtd.Kiosk.RealTime.Config;
using Mtd.Kiosk.RealTime.Entities;
using System.Text.Json;

namespace Mtd.Kiosk.RealTime
{
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

			HttpResponseMessage? httpResponseMessage;
			try
			{
				// TODO DEV
				httpResponseMessage = await _client.GetAsync(_config.GmUri, cancellationToken);

			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error getting general messages from realtime API.");
				return null;
			}

			if (httpResponseMessage == null || !httpResponseMessage.IsSuccessStatusCode)
			{
				_logger.LogError("Error getting general messages from realtime API.");

			}

			try
			{
				using var contentStream = await httpResponseMessage.Content.ReadAsStreamAsync(cancellationToken);
				// DEV: content stream
				//using var contentStream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes("[\r\n  {\r\n    \"id\": \"1722275126603@144\",\r\n    \"displayId\": \"144\",\r\n    \"text\": \"Attention all passengers catching buses southbound at the Transit Plaza, Please board at the NE corner of Sixth and Chalmers St. or the NE corner of Sixth and John St.\",\r\n    \"startTime\": \"2024-07-29T12:45:00.0000000-05:00\",\r\n    \"endTime\": \"2024-08-01T23:59:00.0000000-05:00\",\r\n    \"blockRealtime\": true,\r\n    \"stopIds\": [\r\n      \"PLAZA:3\"\r\n    ]\r\n  },\r\n  {\r\n    \"id\": \"1722275126603@142\",\r\n    \"displayId\": \"142\",\r\n    \"text\": \"Attention all passengers catching buses southbound at the Transit Plaza, Please board at the NE corner of Sixth and Chalmers St. or the NE corner of Sixth and John St.\",\r\n    \"startTime\": \"2024-07-29T12:45:00.0000000-05:00\",\r\n    \"endTime\": \"2024-08-01T23:59:00.0000000-05:00\",\r\n    \"blockRealtime\": true,\r\n    \"stopIds\": [\r\n      \"PLAZA:1\"\r\n    ]\r\n  },\r\n  {\r\n    \"id\": \"1722285281932@123\",\r\n    \"displayId\": \"123\",\r\n    \"text\": \"Attention all passengers catching buses southbound at the Transit Plaza, Please board at the NE corner of Sixth and Chalmers St. or the NE corner of Sixth and John St.\",\r\n    \"startTime\": \"2024-07-29T15:34:00.0000000-05:00\",\r\n    \"endTime\": \"2024-08-26T16:34:00.0000000-05:00\",\r\n    \"blockRealtime\": true,\r\n    \"stopIds\": [\r\n      \"CHCHNEIL:8\"\r\n, \"AAA\"    ]\r\n  }\r\n]"));

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
			HttpResponseMessage? httpResponseMessage;
			try
			{
				httpResponseMessage = await _client.GetAsync($"{_config.SmUri?.ToString()}?stopIds={stopId}&previewTime=60", cancellationToken);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error getting real-time data for stop {StopId}", stopId);
				return null;
			}

			if (httpResponseMessage == null)
			{
				_logger.LogError("Error getting real-time data for stop {StopId}", stopId);
				return null;
			}

			if (!httpResponseMessage.IsSuccessStatusCode)
			{
				_logger.LogError("Error getting real-time data for stop {StopId}: {StatusCode}", stopId, httpResponseMessage.StatusCode);
			}


			try
			{
				using var contentStream = await httpResponseMessage.Content.ReadAsStreamAsync(cancellationToken);

				if (contentStream.Length == 0)
				{
					return null;
				}

				var smResult = await JsonSerializer.DeserializeAsync<Departure[]>(contentStream, cancellationToken: cancellationToken);
				// DEV: cut the array down to just the first element
				//smResult = smResult?.Take(3).ToArray();
				//smResult = [];
				return smResult;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error deserializing real-time data for stop {StopId}", stopId);
				return null;
			}
		}
	}
}
