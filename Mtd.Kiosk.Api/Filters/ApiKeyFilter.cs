using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Mtd.Kiosk.Api.Filters
{
	internal class ApiKeyFilter : IAuthorizationFilter
	{
		// todo replace
		private readonly string[] _keys = ["mtddev"];
		private readonly ILogger<ApiKeyFilter> _logger;

		public ApiKeyFilter(ILogger<ApiKeyFilter> logger)
		{

			_logger = logger;
		}

		public void OnAuthorization(AuthorizationFilterContext context)
		{
			_logger.LogTrace("Executing {filterName}", nameof(ApiKeyFilter));

			var keyFromRequest = string.Empty;
			if (context.HttpContext.Request.Headers.TryGetValue("X-ApiKey", out var headerKey))
			{
				_logger.LogDebug("Got key from 'X-ApiKey' header.");
				keyFromRequest = headerKey;
			}
			else if (context.HttpContext.Request.Query.TryGetValue("key", out var queryKey))
			{
				_logger.LogDebug("Got key from 'key' query string param");
				keyFromRequest = queryKey;
			}
			else
			{
				_logger.LogInformation("No API key provided.");
			}

			if (_keys.Any(k => string.Equals(k, keyFromRequest, StringComparison.OrdinalIgnoreCase)))
			{
				_logger.LogTrace("Good API key.");
				return;
			}
			else
			{
				_logger.LogWarning("API Key provided but no matches. {key}", keyFromRequest);
			}

			context.Result = new UnauthorizedResult();
		}
	}
}
