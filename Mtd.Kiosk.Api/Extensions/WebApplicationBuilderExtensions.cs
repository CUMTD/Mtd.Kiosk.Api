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
using Serilog;

namespace Mtd.Kiosk.Api.Extensions
{
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

			builder.Configuration.AddEnvironmentVariables("Kiosk_");

			_ = builder.Services.AddOptions<ApiAuthentication>()
			.Bind(builder.Configuration.GetSection(nameof(ApiAuthentication)))
			.ValidateDataAnnotations();

			var config = builder.Configuration.Get<ConnectionStrings>();
			if (config != default)
			{
				_ = builder.Services.AddSingleton(config);
			}

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

			_ = builder.Services.AddDbContextPool<KioskContext>((sp, options) =>
			{
				var connectionString = sp.GetRequiredService<IOptions<ConnectionStrings>>().Value.KioskConnection;
				options.UseSqlServer(connectionString);
				// options.UseLazyLoadingProxies();
			});

			_ = builder.Services.AddDbContextPool<StopwatchContext>((sp, options) =>
			{
				var connectionString = sp.GetRequiredService<IOptions<ConnectionStrings>>().Value.StopwatchConnection;
				options.UseSqlServer(connectionString);
				// options.UseLazyLoadingProxies();
			});

			_ = builder.Services.AddScoped<IPublicRouteRepository<IReadOnlyCollection<PublicRoute>>, PublicRouteRepository>();
			_ = builder.Services.AddScoped<IPublicRouteGroupRepository<IReadOnlyCollection<PublicRouteGroup>>, PublicRouteGroupRepository>();
			_ = builder.Services.AddScoped<IRouteRepository<IReadOnlyCollection<Stopwatch.Core.Entities.Transit.Route>>, RouteRepository>();

			return builder;
		}
		private static WebApplicationBuilder ConfigureHTTPClient(this WebApplicationBuilder builder)
		{
			var timeoutPolicy = Policy.TimeoutAsync<HttpResponseMessage>(TimeSpan.FromSeconds(5));

			var retryPolicy = HttpPolicyExtensions
				.HandleTransientHttpError()
				.Or<TimeoutRejectedException>()
				.WaitAndRetryAsync(2, count => TimeSpan.FromMilliseconds(500 * Math.Pow(count, 2))); // 500, then 2000

			var shortCircuitPolicy = HttpPolicyExtensions
				.HandleTransientHttpError()
				.Or<TimeoutRejectedException>()
				.AdvancedCircuitBreakerAsync(0.75, TimeSpan.FromSeconds(60), 4, TimeSpan.FromSeconds(120));

			var defaultPolicy = Policy.WrapAsync(timeoutPolicy, retryPolicy, shortCircuitPolicy);

			builder.Services.AddHttpClient<RealTimeClient>().AddPolicyHandler(defaultPolicy);
			return builder;
		}
		private static WebApplicationBuilder ConfigureLogging(this WebApplicationBuilder builder)
		{
			builder.Services.AddSerilog();

			_ = builder
				.Host
				.UseSerilog((context, loggerConfig) => loggerConfig.ReadFrom.Configuration(context.Configuration));

			return builder;
		}
		private static WebApplicationBuilder ConfigureApi(this WebApplicationBuilder builder)
		{
			_ = builder.Services.AddControllers();

			_ = builder.Services.AddCors(options => options.AddPolicy(
				"AllowDashboard",
				policy => policy.WithOrigins("https://localhost:3000")
				.WithOrigins("http://localhost:3000")
				.AllowAnyHeader()
				.AllowAnyMethod()));

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
			/*var conectionString = builder.Configuration.GetConnectionString("StopwatchConnection");
			if (string.IsNullOrWhiteSpace(conectionString))
			{
				throw new InvalidOperationException("StopwatchConnection is not configured in user secrets");
			}

			_ = builder.Services.AddDbContextPool<StopwatchContext>((serviceProvider, options) =>
			{
				options.UseLazyLoadingProxies();
				options.UseSqlServer(conectionString);
			});*/

			return builder;
		}
		private static WebApplicationBuilder ConfigureDI(this WebApplicationBuilder builder)
		{
			_ = builder.Services.AddScoped<IKioskRepository, KioskRepository>();
			_ = builder.Services.AddScoped<IHeartbeatRepository, HeartbeatRepository>();
			_ = builder.Services.AddScoped<ITicketRepository, TicketRepository>();
			_ = builder.Services.AddScoped<ITicketNoteRepository, TicketNoteRepository>();

			_ = builder.Services.AddMemoryCache();

			return builder;
		}
	}
}
