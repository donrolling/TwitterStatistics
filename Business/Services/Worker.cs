using Domain.Interfaces;
using Domain.Models.Application;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Timer = System.Timers.Timer;

namespace Business.Services
{
	public sealed class Worker : BackgroundService
	{
		private readonly ITweetStreamer _tweetStreamer;
		private readonly AppSettings _appSettings;
		private readonly ILogger<Worker> _logger;

		public Worker(
			ITweetStreamer tweetStreamer,
			IOptions<AppSettings> appSettings,
			ILogger<Worker> logger
		)
		{
			_tweetStreamer = tweetStreamer;
			_appSettings = appSettings.Value;
			_logger = logger;
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			try
			{
				var timer = new Timer(TimeSpan.FromSeconds(_appSettings.LogStatisticsInSeconds));
				// this timer action will happen on a separate thread from the _tweetStreamer.ReadAsync method
				timer.Elapsed += (sender, eventArgs) => {
					var topTen = _tweetStreamer.GetCurrentStatistics();
					_logger.LogInformation(topTen);
				};
				timer.Start();
				await _tweetStreamer.ReadAsync(stoppingToken);
				timer.Stop();
			} catch (Exception ex)
			{
				_logger.LogError(ex, ex.Message);
			}
		}
	}
}