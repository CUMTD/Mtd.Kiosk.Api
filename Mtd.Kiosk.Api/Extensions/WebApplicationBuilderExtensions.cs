using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Mtd.Kiosk.Api.Config;
using Mtd.Kiosk.Api.Filters;
using Mtd.Kiosk.Core.Repositories;
using Mtd.Kiosk.Infrastructure.EfCore;
using Mtd.Kiosk.Infrastructure.EfCore.Repository;
using Mtd.Kiosk.RealTime;
using Mtd.Kiosk.RealTime.Config;
using Mtd.Stopwatch.Core.Entities.Schedule;
using Mtd.Stopwatch.Core.Repositories.Schedule;
using Mtd.Stopwatch.Core.Repositories.Transit;
using Mtd.Stopwatch.Infrastructure.EFCore;
using Mtd.Stopwatch.Infrastructure.EFCore.Repositories.Schedule;
using Mtd.Stopwatch.Infrastructure.EFCore.Repositories.Transit;
using Polly;
using Polly.Extensions.Http;
using Polly.Timeout;
using Polly.Wrap;
using Serilog;

namespace Mtd.Kiosk.Api.Extensions;

internal static class WebApplicationBuilderExtensions
{

	public static WebApplicationBuilder Configure(this WebApplicationBuilder builder) => builder
		.AddConfiguration()
		.ConfigureLogging()
		.ConfigureApi()
		.ConfigureDB()
		.ConfigureDI()
		.ConfigureHTTPClient();

	private static WebApplicationBuilder AddConfiguration(this WebApplicationBuilder builder)
	{
		if (builder.Environment.IsDevelopment())
		{
			_ = builder.Configuration.AddUserSecrets<Program>();
		}

		_ = builder.Configuration.AddEnvironmentVariables("Kiosk_");

		_ = builder.Services.AddOptions<ApiAuthentication>()
			.Bind(builder.Configuration.GetSection(nameof(ApiAuthentication)))
			.ValidateDataAnnotations();

		_ = builder.Services
			.AddOptions<ConnectionStrings>()
			.BindConfiguration("ConnectionStrings")
			.ValidateDataAnnotations()
			.ValidateOnStart();

		_ = builder.Services
			.AddOptions<ApiConfiguration>()
			.BindConfiguration("ApiConfiguration")
			.ValidateDataAnnotations()
			.ValidateOnStart();

		_ = builder.Services
			.AddOptions<RealTimeClientConfig>()
			.BindConfiguration("RealTimeClientConfig")
			.ValidateDataAnnotations()
			.ValidateOnStart();

		return builder;
	}
	private static WebApplicationBuilder ConfigureLogging(this WebApplicationBuilder builder)
	{
		_ = builder.Host.UseSerilog((context, loggerConfig) => loggerConfig.ReadFrom.Configuration(context.Configuration));

		return builder;
	}
	private static WebApplicationBuilder ConfigureApi(this WebApplicationBuilder builder)
	{
		var corsPolicyName = builder.Configuration["Cors:PolicyName"] ?? throw new InvalidOperationException("Cors:PolicyName not defined");
		var corsAllowedOrigin = builder.Configuration["Cors:AllowedOrigins"] ?? throw new InvalidOperationException("Cors:AllowedOrigins not defined");

		_ = builder.Services.AddControllers();

		_ = builder.Services.AddCors(options => options.AddPolicy(
			corsPolicyName,
			policy => policy
			.WithOrigins(corsAllowedOrigin)
			.AllowAnyHeader()
			.AllowAnyMethod()
		));

		_ = builder.Services.AddEndpointsApiExplorer();

		_ = builder.Services.AddSwaggerGen(options =>
		{
			options.SwaggerDoc($"v1.0", new OpenApiInfo
			{
				Version = "1.0",
				Title = $"Kiosk",
				Description = "MTD Kiosk API.",
				Contact = new OpenApiContact
				{
					Name = "MTD",
					Email = "developer@mtd.org"
				}
			});

			var authMethodName = "API Key - Header";
			var securityScheme = new OpenApiSecurityScheme
			{
				Reference = new OpenApiReference
				{
					Type = ReferenceType.SecurityScheme,
					Id = authMethodName
				},
				Description = "Provide your API key in the header using X-ApiKey.",
				In = ParameterLocation.Header,
				Name = "X-ApiKey",
				Type = SecuritySchemeType.ApiKey,
				Scheme = "Bearer"
			};
			options.AddSecurityDefinition(authMethodName, securityScheme);
			options.AddSecurityRequirement(new OpenApiSecurityRequirement() {
				{ securityScheme, Array.Empty <string>() }
			  });

		});
		_ = builder.Services.AddControllers(options => options.Filters.Add<ApiKeyFilter>());

		return builder;
	}
	private static WebApplicationBuilder ConfigureDB(this WebApplicationBuilder builder)
	{
		_ = builder.Services.AddDbContextPool<KioskContext>((sp, options) =>
		{
			var connectionString = sp.GetRequiredService<IOptions<ConnectionStrings>>().Value.KioskConnection;
			options.UseSqlServer(connectionString);
			options.UseLazyLoadingProxies();
		});

		_ = builder.Services.AddDbContextPool<StopwatchContext>((sp, options) =>
		{
			var connectionString = sp.GetRequiredService<IOptions<ConnectionStrings>>().Value.StopwatchConnection;
			options.UseSqlServer(connectionString);
			options.UseLazyLoadingProxies();
		});

		return builder;
	}
	private static WebApplicationBuilder ConfigureDI(this WebApplicationBuilder builder)
	{
		_ = builder.Services.AddScoped<IPublicRouteRepository<IReadOnlyCollection<PublicRoute>>, PublicRouteRepository>();
		_ = builder.Services.AddScoped<IPublicRouteGroupRepository<IReadOnlyCollection<PublicRouteGroup>>, PublicRouteGroupRepository>();
		_ = builder.Services.AddScoped<IRouteRepository<IReadOnlyCollection<Stopwatch.Core.Entities.Transit.Route>>, RouteRepository>();

		_ = builder.Services.AddScoped<IKioskRepository, KioskRepository>();
		_ = builder.Services.AddScoped<IHeartbeatRepository, HeartbeatRepository>();
		_ = builder.Services.AddScoped<ITicketRepository, TicketRepository>();
		_ = builder.Services.AddScoped<ITicketNoteRepository, TicketNoteRepository>();

		_ = builder.Services.AddMemoryCache();

		return builder;
	}
	private static WebApplicationBuilder ConfigureHTTPClient(this WebApplicationBuilder builder)
	{
		// Use AddHttpClient with a typed client and DI to inject logger
		builder.Services.AddHttpClient<RealTimeClient>()
			.AddPolicyHandler((serviceProvider, request) =>
			{
				// Resolve the logger from the service provider
				var logger = serviceProvider.GetRequiredService<Microsoft.Extensions.Logging.ILogger>();

				// Get the default policy with logging
				return GetDefaultPolicy(logger);
			});

		return builder;
	}
	private static AsyncPolicyWrap<HttpResponseMessage> GetDefaultPolicy(Microsoft.Extensions.Logging.ILogger logger)
	{
		var timeoutPolicy = Policy.TimeoutAsync<HttpResponseMessage>(TimeSpan.FromSeconds(5));

		var retryPolicy = HttpPolicyExtensions
			.HandleTransientHttpError()
			.Or<TimeoutRejectedException>()
			.WaitAndRetryAsync(
				2,
				count => TimeSpan.FromMilliseconds(500 * Math.Pow(count, 2)), // 500, then 2000
				(outcome, timespan, retryAttempt, context) =>
				{
					logger.LogWarning(outcome.Exception, "Retry attempt {RetryAttempt} after {Delay}ms due to {Exception}.", retryAttempt, timespan.TotalMilliseconds, outcome.Exception?.Message);
				});

		var circuitBreakerPolicy = HttpPolicyExtensions
			.HandleTransientHttpError()
			.Or<TimeoutRejectedException>()
			.AdvancedCircuitBreakerAsync(
				0.75,
				TimeSpan.FromSeconds(60),
				4,
				TimeSpan.FromSeconds(120),
				onBreak: (outcome, timespan) =>
				{
					logger.LogWarning("Circuit breaker opened for {Duration}ms due to {Exception}.", timespan.TotalMilliseconds, outcome.Exception?.Message);
				},
				onReset: () =>
				{
					logger.LogInformation("Circuit breaker reset.");
				},
				onHalfOpen: () =>
				{
					logger.LogInformation("Circuit breaker is half-open. Testing state.");
				});

		// Combine policies into a single policy
		return Policy.WrapAsync(timeoutPolicy, retryPolicy, circuitBreakerPolicy);
	}
}
