using Domain.Constants;
using Domain.Interfaces;
using Domain.Models.Application;
using Domain.Models.Twitter;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using static System.Net.Mime.MediaTypeNames;

namespace Business.Services
{
    public class TweetStreamer : ITweetStreamer
	{
		private readonly HttpClient _httpClient;
		private readonly AppSettings _appSettings;
		private readonly ILogger<TweetStreamer> _logger;

		private readonly Regex _regEx = new Regex(@"#\w+");
		private long _count = 0;
		private ConcurrentDictionary<string, int> _tags = new();

		public TweetStreamer(
			IOptions<AppSettings> appSettings,
			IHttpClientFactory httpClientFactory,
			ILogger<TweetStreamer> logger
		)
		{
			_appSettings = appSettings.Value;
			_logger = logger;
			_httpClient = httpClientFactory.CreateClient(HttpClients.Twitter);
		}

		public string GetCurrentStatistics()
		{
			// probably not super performant, should use some spans or something
			var take = 10;
			var header = $"Total Count: {_count}{Environment.NewLine}Top Ten:{Environment.NewLine}";
			var tags = _tags.OrderByDescending(a => a.Value).Take(take).Select(a => $"{a.Key}: {a.Value}");
			var sb = new StringBuilder();
			var order = 1;
			foreach (var tag in tags)
			{
				sb.AppendLine();
				sb.Append(order.ToString());				
				sb.Append(". ");
				sb.Append(tag);
				order++;
			}
			var result = sb.ToString();
			// for the sake of testing, I'm logging this any time it gets hit
			_logger.LogError(result);
			return result;
		}

		public async Task<OperationResult> ReadAsync(CancellationToken cancellationToken)
		{
			try
			{
				using (var response = await _httpClient.GetAsync(_appSettings.TwitterStreamURL, HttpCompletionOption.ResponseHeadersRead))
				{
					response.EnsureSuccessStatusCode();
					if (response.Content is null)
					{
						// shouldn't happen
						return OperationResult.Fail("Content was null");
					}
					using (var stream = await response.Content.ReadAsStreamAsync())
					using (var reader = new StreamReader(stream))
					{
						while (
							!reader.EndOfStream
							&& !cancellationToken.IsCancellationRequested
						)
						{
							await ReceiveNewTweet(reader);
						}
					}
					// you'll never get here if this runs forever
					return OperationResult.Ok();
				}
			} catch (HttpRequestException hex)
			{
				// probably a bad status code
				_logger.LogError(hex.Message);
				return OperationResult.Fail(hex.Message);
			} catch (OperationCanceledException)
			{
				// cancellation is not failure
				return OperationResult.Ok();
			} catch (Exception ex)
			{
				// catch-all
				_logger.LogError(ex.Message);
				return OperationResult.Fail(ex.Message);
			}
		}

		private void ManageTags(Tweet tweet)
		{
			var tagMatches = _regEx.Matches(tweet.text)
									.Cast<Match>()
									.Select(m => m.Value)
									.ToArray();
			if (tagMatches is not null && tagMatches.Any())
			{
				foreach (var tag in tagMatches)
				{
					// the 1 is the default value when the key doesn't exist
					_tags.AddOrUpdate(tag, 1, (key, oldValue) => oldValue + 1);
				}
			}
		}

		private async Task ReceiveNewTweet(StreamReader reader)
		{
			// each tweet is one line
			var text = await reader.ReadLineAsync();
			Data<Tweet> tweetData = null;
			try
			{
				// these objects are a little cumbersome, but it was fast to setup
				tweetData = JsonSerializer.Deserialize<Data<Tweet>>(text);
			} catch
			{
				// do nothing here
			}
			if (tweetData is null)
			{
				return;
			}
			var tweet = tweetData.data;
			if (string.IsNullOrWhiteSpace(tweet.text))
			{
				return;
			}
			ManageTags(tweet);
			_count++;

			if (_appSettings.EnableTweetLogging)
			{
				var author = tweetData.includes?.users?.FirstOrDefault(a => a.id == tweet.author_id);
				var item = new {
					count = _count,
					text = tweet.text,
					name = author?.name,
					username = author?.username
				};
				var logItem = JsonSerializer.Serialize(item);
				_logger.LogInformation(logItem);
			}
		}
	}
}