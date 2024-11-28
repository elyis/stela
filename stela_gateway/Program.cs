using Ocelot.DependencyInjection;
using Ocelot.Middleware;

var builder = WebApplication.CreateBuilder(args);

var configurationBuilder = new ConfigurationBuilder();
configurationBuilder.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);

builder.Services.AddOcelot(configurationBuilder.Build());
var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

await app.UseOcelot();

app.MapGet("/", () => "Gateway works!");

app.Run();