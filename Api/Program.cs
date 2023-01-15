using Microsoft.AspNetCore.SignalR;

using SpriteSplitter.Tools;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSignalR();
builder.Services.AddSingleton(new Folder("", "scores"));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

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

app.MapHub<SpaceSpider>("/SpaceSpider");

app.UseStaticFiles();
app.Run();


public class SpaceSpider : Hub
{
    public record Position(int X, int Y, int Score);

    public async Task Mouse(Position position)
    {
        await Clients.Others.SendAsync("mouse", position);
    }

    public async Task HighScore(int score, Folder folder)
    {
        folder.AppendTo("highscore.json", file => File.AppendAllText(file, $"{Context.ConnectionId}={score},"));
        await Clients.Others.SendAsync("highScore", score);
    }
}