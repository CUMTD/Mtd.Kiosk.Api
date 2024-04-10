using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Mtd.Kiosk.Api.Config;
using Mtd.Kiosk.Infrastructure.EfCore;
using Mtd.Kiosk.RealTime;
using Polly;
using Polly.Extensions.Http;
using Polly.Timeout;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddUserSecrets<Program>();

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddCors(options => options.AddPolicy(
	"AllowDashboard",
		policy => policy.WithOrigins("https://localhost:3000")
			.WithOrigins("http://localhost:3000")
			.AllowAnyHeader()
			.AllowAnyMethod()));

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();



var config = builder.Configuration.Get<ConnectionStrings>();
if (config != default)
{
	_ = builder.Services.AddSingleton(config);
}

builder.Services
	.AddOptions<ConnectionStrings>()
	.BindConfiguration("ConnectionStrings")
	.ValidateDataAnnotations()
	.ValidateOnStart();

builder.Services.AddOptions<SeqStrings>()
	.BindConfiguration("Seq")
	.ValidateDataAnnotations()
	.ValidateOnStart();

builder.Services.AddDbContextPool<KioskContext>((sp, options) =>
{
	var connectionString = sp.GetRequiredService<IOptions<ConnectionStrings>>().Value.KioskConnection;
	options.UseSqlServer(connectionString);
	options.UseLazyLoadingProxies();
});

builder.Services.AddLogging(logging =>
{
	logging.AddSeq(builder.Configuration["Seq:Host"], apiKey: builder.Configuration["Seq:ApiKey"]);
});

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

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.UseCors("AllowDashboard");
app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
