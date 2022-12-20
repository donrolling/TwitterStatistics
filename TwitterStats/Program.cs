using Domain.Models.Application;
using Microsoft.Extensions.Options;
using ProjectConfiguration;
using Serilog;
using System.Reflection;

try
{
	var hostConfiguration = Bootstrapper.GetHost(args, Assembly.GetExecutingAssembly());
	var appSettings = hostConfiguration.Host.Services.GetService<IOptions<AppSettings>>();
	var logger = hostConfiguration.Host.Services.GetService<ILogger<Program>>();
	logger.LogInformation($"Starting host: {hostConfiguration.ServiceName}");
	await hostConfiguration.Host.RunAsync();
} catch (Exception ex)
{
	Log.Fatal(ex, "Host terminated unexpectedly");
} finally
{
	Log.CloseAndFlush();
}