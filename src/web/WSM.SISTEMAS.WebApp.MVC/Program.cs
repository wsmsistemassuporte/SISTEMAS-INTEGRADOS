using WSM.SISTEMAS.WebApp.MVC.Configuration;
using WSM.SISTEMAS.WebAPI.Core.Logging;

var builder = WebApplication.CreateBuilder(args);

builder.AddErrorFileLogging("WSM.SISTEMAS.WebApp.MVC");

builder
    .AddMvcConfiguration()
    .AddAuthenticationConfiguration()
    .AddDependencyInjectionConfiguration()
    .AddHttpClientConfiguration();

var app = builder.Build();

app.UseErrorFileRequestLogging();
app.UseWebAppConfiguration();

app.Run();
