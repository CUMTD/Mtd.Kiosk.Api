using Serilog;

namespace Mtd.Kiosk.Api.Extensions;

internal static class WebApplicationExtensions
{
	public static WebApplication ConfigureApp(this WebApplication app)
	{
		if (app.Environment.IsProduction())
		{
			_ = app.UseHsts();
		}

		// Log HTTP requests in Serilog
		_ = app.UseSerilogRequestLogging();

		_ = app.UseHttpsRedirection();

		_ = app.UseSwagger();
		_ = app.UseSwaggerUI(options =>
		{
			options.DocumentTitle = "Kiosk API";
			options.RoutePrefix = string.Empty;
			options.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.List);

			options.DisplayRequestDuration();

			options.InjectStylesheet("/css/swagger-ui.css");

			options
				.SwaggerEndpoint(
					$"/swagger/v1.0/swagger.json",
					$"Kiosk API - v1.0".Trim()
				);
		});

		_ = app.UseRouting();

		_ = app.UseAuthorization();

		_ = app.UseStaticFiles();

		var corsPolicyName = app.Configuration["Cors:PolicyName"] ?? throw new InvalidOperationException("Cors:PolicyName not defined");
		_ = app.UseCors(corsPolicyName);

		_ = app.MapControllers();

		return app;
	}
}
