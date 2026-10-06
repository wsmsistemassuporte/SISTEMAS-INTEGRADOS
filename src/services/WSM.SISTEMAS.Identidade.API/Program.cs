using WSM.SISTEMAS.Identidade.API.Configuration;
using Microsoft.AspNetCore.DataProtection;
using WSM.SISTEMAS.WebAPI.Core.Exceptions;
using WSM.SISTEMAS.WebAPI.Core.Logging;

var builder = WebApplication.CreateBuilder(args);

builder.AddErrorFileLogging("WSM.SISTEMAS.Identidade.API");

builder.Configuration.AddKeyPerFile("/run/secrets", optional: true);

if (builder.Environment.IsDevelopment())
{
    var keysPath = Path.Combine(Path.GetTempPath(), "WSM.SISTEMAS.Identidade.API", "DataProtectionKeys");
    Directory.CreateDirectory(keysPath);
    var dataProtection = builder.Services
        .AddDataProtection()
        .PersistKeysToFileSystem(new DirectoryInfo(keysPath));

    if (OperatingSystem.IsWindows())
    {
        dataProtection.ProtectKeysWithDpapi();
    }
}

builder.Services.AddControllers();
builder.Services.AddGlobalExceptionHandler();
builder.Services.AddSwaggerConfiguration();

builder.AddIdentityConfiguration();

var app = builder.Build();

app.UseErrorFileRequestLogging();

app.UseGlobalExceptionHandler();

app.UseSwaggerConfiguration();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
