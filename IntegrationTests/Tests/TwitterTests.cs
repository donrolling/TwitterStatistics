using Domain.Interfaces;
using IntegrationTests.BaseClasses;
using Timer = System.Timers.Timer;

namespace IntegrationTests.Tests
{
	[TestClass]
	public class TwitterTests : IntegrationTestBase
	{
		private readonly ITweetStreamer _tweetStreamer;

		public TwitterTests()
		{
			_tweetStreamer = this.GetService<ITweetStreamer>();
		}

		[TestMethod]
		public async Task ReadStream_ForFiveSeconds_ShouldSucceed()
		{
			var cancellationSource = new CancellationTokenSource(TimeSpan.FromSeconds(5));
			var result = await _tweetStreamer.ReadAsync(cancellationSource.Token);
			Assert.IsNotNull(result, "Result was null.");
			Assert.IsTrue(result.Success, result.Message);
		}

		[TestMethod]
		public async Task ReadTopTen_30Seconds_CanLogWithoutBlocking()
		{
			var totalTime = 30;
			var timesRead = 0;
			var expectedTimesRead = 3;
			var readLogTime = totalTime / expectedTimesRead;
			var timer = new Timer(TimeSpan.FromSeconds(readLogTime));
			// this timer action will happen on a separate thread from the _tweetStreamer.ReadAsync method 
			timer.Elapsed += (sender, eventArgs) => {
				var topTen = _tweetStreamer.GetCurrentStatistics();
				Assert.IsFalse(string.IsNullOrWhiteSpace(topTen), "Top Ten was empty.");
				Assert.AreEqual(10, topTen.Count(a => a == '#'));
				timesRead++;
			};
			timer.Start();

			var cancellationSource = new CancellationTokenSource(TimeSpan.FromSeconds(totalTime));
			var result = await _tweetStreamer.ReadAsync(cancellationSource.Token);
			timer.Stop();
			Assert.IsNotNull(result, "Result was null.");
			Assert.IsTrue(result.Success, result.Message);
			
			// should've read the top ten twice
			Assert.AreEqual(expectedTimesRead, timesRead);
		}
	}
}