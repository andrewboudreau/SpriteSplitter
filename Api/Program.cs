using Microsoft.AspNetCore.SignalR;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSignalR();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("prepare/{uri}", (Uri? imageUrl) =>
{
    Console.WriteLine(imageUrl);
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            Guid.NewGuid().ToString()
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast")
.WithOpenApi();

string splitterHtml = File.ReadAllText(@"wwwroot/splitter.html");
string viewerHtml = File.ReadAllText(@"wwwroot/viewer.html");
string indexHtml = File.ReadAllText(@"wwwroot/index.html");

app.MapGet("", () => Results.Content(indexHtml, "text/html")).WithName("Welcome UI").WithOpenApi();
app.MapGet("splitter", () => Results.Content(splitterHtml, "text/html")).WithName("Splitter UI").WithOpenApi(); 
app.MapGet("viewer", () => Results.Content(viewerHtml, "text/html")).WithName("Viewer UI").WithOpenApi();
app.MapGet("ship/next", () => Results.Json(new 
{ 
    ImageUrl = "sprites/sprite-1.png",
    Rotate = 180,
    Scale = 0.25f
})).WithName("Next Ship").WithOpenApi();

app.MapHub<ShapeHub>("/shapeHub");

app.UseStaticFiles();
app.Run();

internal record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}

public class ShapeHub : Hub
{
    public record Position(int X, int Y, int Score);

    public async Task Mouse(Position position)
    {
        await Clients.Others.SendAsync("mouse", position);
        //Console.WriteLine($"sent shape move {position}");
    }
}