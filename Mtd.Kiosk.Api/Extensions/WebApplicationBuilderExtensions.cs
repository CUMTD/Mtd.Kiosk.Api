using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Mtd.Kiosk.Api.Config;
using Mtd.Kiosk.Api.Filters;
using Mtd.Kiosk.Core.Repositories;
using Mtd.Kiosk.Infrastructure.EfCore;
using Mtd.Kiosk.Infrastructure.EfCore.Repository;
using Mtd.Kiosk.RealTime;
using Polly;
using Polly.Extensions.Http;
using Polly.Timeout;
using Serilog;

namespace Mtd.Kiosk.Api.Extensions
{
	internal static class WebApplicationBuilderExtensions
	{
		public static WebApplicationBuilder Configure(this WebApplicationBuilder builder) => builder.AddConfiguration().ConfigureLogging().ConfigureApi().ConfigureDI().ConfigureHTTPClient();

		private static WebApplicationBuilder ConfigureDI(this WebApplicationBuilder builder)
		{
			_ = builder.Services.AddScoped<IKioskRepository, KioskRepository>();
			_ = builder.Services.AddScoped<IHeartbeatRepository, HeartbeatRepository>();
			_ = builder.Services.AddScoped<ITicketRepository, TicketRepository>();
			_ = builder.Services.AddScoped<ITicketNoteRepository, TicketNoteRepository>();

			_ = builder.Services.AddScoped<ApiKeyFilter>();

			return builder;
		}
		private static WebApplicationBuilder AddConfiguration(this WebApplicationBuilder builder)
		{
			_ = builder.Configuration.AddUserSecrets<Program>();

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


			_ = builder.Services.AddDbContextPool<KioskContext>((sp, options) =>
			{
				var connectionString = sp.GetRequiredService<IOptions<ConnectionStrings>>().Value.KioskConnection;
				options.UseSqlServer(connectionString);
				options.UseLazyLoadingProxies();
			});

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
			_ = builder.Services.AddSwaggerGen();
			_ = builder.Services.AddControllers(options => options.Filters.Add<ApiKeyFilter>());

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
	}
}
