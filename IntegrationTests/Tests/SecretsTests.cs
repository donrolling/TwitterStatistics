using Domain.Models.Application;
using IntegrationTests.BaseClasses;
using Microsoft.Extensions.Options;

namespace IntegrationTests.Tests
{
	[TestClass]
	public class SecretsTests : IntegrationTestBase
	{
		private readonly IOptions<TwitterSecrets> _secrets;

		public SecretsTests()
		{
			_secrets = GetService<IOptions<TwitterSecrets>>();
		}

		[TestMethod]
		public void ReadSecrets_NotNull()
		{
			Assert.IsFalse(string.IsNullOrWhiteSpace(_secrets.Value.BearerToken));
		}
	}
}