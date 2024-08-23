using Mtd.Kiosk.Api.Extensions;
using Serilog;

var assemblyName = System.Reflection.Assembly.GetExecutingAssembly().GetName()?.Name ?? "Kiosk API";
var builder = WebApplication.CreateBuilder(args);

try
{
	Log.Information("{assemblyName} is being configured.", assemblyName);

	// Configure the rest of the services and middleware
	builder.Configure();

	var app = builder
		.Build()
		.ConfigureApp();

	Log.Information("{assemblyName} is starting up.", assemblyName);

	await app.RunAsync();
}
catch (Exception ex)
{
	Log.Fatal(ex, "{assemblyName} failed to start correctly.", assemblyName);
	throw; // Rethrow to let the process know it has crashed
}
finally
{
	Log.Information("{assemblyName} has stopped.", assemblyName);
	Log.CloseAndFlush(); // Ensure to flush and close the log
}
