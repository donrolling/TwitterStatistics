using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ProjectConfiguration;
using System.Reflection;

namespace IntegrationTests.BaseClasses
{
	public class IntegrationTestBase
	{
		protected IHost Host { get; private set; }

		protected T GetService<T>()
		{
			return Host.Services.GetService<T>();
		}

		public IntegrationTestBase()
		{
			Host = Bootstrapper.GetHost(new string[] { }, Assembly.GetExecutingAssembly()).Host;
		}
	}
}