using Mtd.Kiosk.Api.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Configure();

var app = builder.Build();

app.UseStaticFiles();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
	options.RoutePrefix = string.Empty;
	options.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.List);
	options.DocumentTitle = "Kiosk API";

	options.DisplayRequestDuration();

	options.InjectStylesheet("/css/swagger-ui.css");

	options
		.SwaggerEndpoint(
			$"/swagger/v1.0/swagger.json",
			$"Kiosk API - v1.0".Trim()
		);
});

app.UseCors("AllowDashboard");
app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
