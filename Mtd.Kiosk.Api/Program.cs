using Mtd.Kiosk.Api.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Configure();

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
