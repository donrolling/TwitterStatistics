using Business.Services;
using Domain.Constants;
using Domain.Interfaces;
using Domain.Models.Application;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using System.Reflection;

namespace ProjectConfiguration
{
	public static class Bootstrapper
	{
		private const string serviceName = $"TwitterStats";

		public static (IHost Host, string ServiceName) GetHost(string[] args, Assembly assembly)
		{
			var hostBuilder = Host.CreateDefaultBuilder(args)
				.ConfigureAppConfiguration((context, builder) => {
					if (OperatingSystem.IsWindows())
					{
						builder.SetBasePath(AppContext.BaseDirectory);
					}
					var path = "appsettings.json";
					builder.AddJsonFile(path, optional: false, reloadOnChange: true);
					builder.AddUserSecrets(assembly);
				})
				.ConfigureServices((hostContext, services) => {
					services.Configure<AppSettings>(hostContext.Configuration.GetSection(nameof(AppSettings)));
					services.Configure<TwitterSecrets>(hostContext.Configuration.GetSection(nameof(TwitterSecrets)));
					services.AddOptions();
					services.AddHostedService<Worker>();
					services.AddTransient<ITweetStreamer, TweetStreamer>();
					// usually I would validate all settings, but I didn't spend the time here
					var appSettings = hostContext.Configuration.GetSection(nameof(AppSettings)).Get<AppSettings>();
					if (appSettings is null)
					{
						throw new Exception("AppSettings was null");
					}
					// usually I would validate all settings, but I didn't spend the time here
					var twitterSecrets = hostContext.Configuration.GetSection(nameof(TwitterSecrets)).Get<TwitterSecrets>();
					if (twitterSecrets is null)
					{
						throw new Exception("TwitterSecrets was null");
					}
					services.AddHttpContextAccessor();
					services.AddHttpClient(HttpClients.Twitter, client => {
						client.BaseAddress = new Uri(appSettings.TwitterBaseUrl);
						client.DefaultRequestHeaders.Add("Authorization", $"Bearer {twitterSecrets.BearerToken}");
					});
				})
				.UseSerilog((hostContext, services, loggerConfiguration) => {
					var appSettings = hostContext.Configuration.GetSection(ConfigKeys.AppSettings).Get<AppSettings>();
					if (appSettings is null)
					{
						throw new Exception("App Settings object is null.");
					}
					var logPath = Path.Combine(AppContext.BaseDirectory, appSettings.LogFilePath).Replace("\\", "/");
					loggerConfiguration
						.ReadFrom.Configuration(hostContext.Configuration)
						.Enrich.WithProperty("Service Name", serviceName)
						.WriteTo.Console()
						.WriteTo.File(logPath, rollingInterval: RollingInterval.Day);
				});
			if (OperatingSystem.IsWindows())
			{
				hostBuilder.UseWindowsService();
			}
			var host = hostBuilder.Build();
			return (host, serviceName);
		}
	}
}