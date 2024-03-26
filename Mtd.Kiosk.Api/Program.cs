using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Mtd.Kiosk.Api.Config;
using Mtd.Kiosk.Core.Entities;
using Mtd.Kiosk.Core.Repositories;
using Mtd.Kiosk.Infrastructure.EfCore;
using Mtd.Kiosk.Infrastructure.EfCore.Repository;
using Mtd.Kiosk.RealTime;
using Polly;
using Polly.Extensions.Http;
using Polly.Timeout;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContextPool<KioskContext>((sp, options) =>
{
	var connectionString = sp.GetRequiredService<IOptions<ConnectionStrings>>().Value.KioskConnection;
	options.UseSqlServer(connectionString);
	options.UseLazyLoadingProxies();
});

builder.Services.AddScoped<IPersonRepository<IReadOnlyCollection<Person>>, PersonRepository>();

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

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
