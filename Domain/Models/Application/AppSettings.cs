using System.ComponentModel.DataAnnotations;

namespace Domain.Models.Application
{
    public class AppSettings
    {
        /// <summary>
        /// Whether or not you want to log all of the tweets.
        /// </summary>
        [Required]
        public bool EnableTweetLogging { get; set; }

        /// <summary>
        /// Path path to the log file. Relative to application directory.
        /// </summary>
        [Required]
        public string LogFilePath { get; set; }

        /// <summary>
        /// Base path to the API
        /// </summary>
        [Required]
        public string TwitterBaseUrl { get; set; }

        /// <summary>
        /// Relative path for the stream
        /// </summary>
        [Required]
        public string TwitterStreamURL { get; set; }

		/// <summary>
		/// How often should we log statistics?
		/// </summary>
		[Required]
		public double LogStatisticsInSeconds { get; set; }
	}
}