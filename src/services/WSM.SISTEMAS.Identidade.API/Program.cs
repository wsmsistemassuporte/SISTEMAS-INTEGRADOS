using WSM.SISTEMAS.Identidade.API.Configuration;
using WSM.SISTEMAS.WebAPI.Core.Exceptions;
using WSM.SISTEMAS.WebAPI.Core.Logging;

var builder = WebApplication.CreateBuilder(args);

builder.AddInfrastructureConfiguration("WSM.SISTEMAS.Identidade.API");

builder.Services.AddApiConfiguration();
builder.Services.AddGlobalExceptionHandler();
builder.Services.AddSwaggerConfiguration();

builder.AddIdentityConfiguration();

var app = builder.Build();

app.UseErrorFileRequestLogging();

app.UseGlobalExceptionHandler();

app.UseSwaggerConfiguration();

app.UseApiConfiguration(app.Environment);

app.Run();
