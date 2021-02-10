using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Zeiss.Api
{
	static class Program
	{
		static async Task Main(string[] args)
		{
			var configuration = BuildConfiguration();

			var webHost = 
				Host.CreateDefaultBuilder(args)
					.ConfigureWebHostDefaults(webHostBuilder =>
					{
						webHostBuilder
							.UseConfiguration(configuration)
							.UseStartup<Startup>();
					})
					.Build();

			await webHost.RunAsync();
		}

		private static IConfigurationRoot BuildConfiguration()
		{
			var defaults = new Dictionary<string, string>()
			{
				{ "RedisConnectionString", "localhost"}
			};

			return
				new ConfigurationBuilder()
					.AddInMemoryCollection(initialData: defaults)
					.AddEnvironmentVariables("API_") // Environment vars override hard-coded defaults
					.Build();
		}
	}
}
