using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.Threading;
using Zeiss.Ingestor.Services;

namespace Zeiss.Ingestor
{
	static class Program
	{
		private const int mExitCodeSuccess = 0;
		private const int mExitCodeExecutionError = 2;
		private const int mExitCodeCancellation = 3;

		static async Task<int> Main(string[] args)
		{
			try
			{
				var configuration = BuildConfiguration();
				using var services = BuildServiceProvider(configuration);
				using var cancellationTokenSource = new CancellationTokenSource();

				Console.CancelKeyPress +=
					(sender, e) =>
					{
						Console.WriteLine("Cancellation requested...");
						cancellationTokenSource.Cancel();
						e.Cancel = true;
					};

				var receiver = services.GetRequiredService<IReceiver>();

				try
				{
					await receiver.StartAsync(cancellationTokenSource.Token);
					await Task.Delay(Timeout.Infinite).WithCancellation(cancellationTokenSource.Token); // Wait forever or until shell cancels with SIGINT
					await receiver.StopAsync(cancellationTokenSource.Token);
				}
				catch (OperationCanceledException ex) when (ex.CancellationToken == cancellationTokenSource.Token)
				{
					Console.WriteLine($"Program terminated by user cancellation.");
					return mExitCodeCancellation;
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Program terminated due to an unexpected error:\n{ex.Message}");
				return mExitCodeExecutionError;
			}

			return mExitCodeSuccess; // Currently we will never get here
		}

		private static IConfigurationRoot BuildConfiguration()
		{
			var defaults = new Dictionary<string, string>()
			{
				{ "WebSocketEndpointUrl", "wss://machinestream.herokuapp.com/ws"},
				{ "RedisConnectionString", "localhost"}
			};

			return
				new ConfigurationBuilder()
					.AddInMemoryCollection(initialData: defaults)
					.AddEnvironmentVariables("INGESTOR_") // Environment vars override hard-coded defaults
					.Build();
		}

		private static ServiceProvider BuildServiceProvider(IConfiguration configuration)
		{
			return
				new ServiceCollection()
					.AddSingleton(serviceProvider => configuration)
					.AddSingleton<IReceiver, WebSocketReceiver>()
					.AddSingleton<IProcessor, RedisProcessor>()
					.BuildServiceProvider();
		}
	}
}
