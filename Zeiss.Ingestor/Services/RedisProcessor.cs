using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Nito.AsyncEx;
using StackExchange.Redis;
using Zeiss.Ingestor.Models;

namespace Zeiss.Ingestor.Services
{
	class RedisProcessor : IProcessor, IDisposable
	{
		public RedisProcessor(IConfiguration configuration)
		{
			var connectionString = configuration["RedisConnectionString"];
			mMultiplexerLazy = new AsyncLazy<ConnectionMultiplexer>(() => ConnectionMultiplexer.ConnectAsync(connectionString));
		}

		private readonly AsyncLazy<ConnectionMultiplexer> mMultiplexerLazy;
		private bool mIsDisposed;

		public async Task ProcessMessageAsync(MachineStatusMessage message, CancellationToken cancellationToken = default)
		{
			if (mIsDisposed)
			{
				throw new ObjectDisposedException(nameof(WebSocketReceiver));
			}

			try
			{
				var redis = await mMultiplexerLazy;
				var db = redis.GetDatabase();

				var name = message.Machine_Id.ToString();
				var value = message.Status.ToString();

				await db.HashSetAsync("MachineStatus", new[] { new HashEntry(name, value) });
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Failed to process message with ID '{message.Machine_Id}: {ex.Message}");
			}
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!mIsDisposed)
			{
				if (disposing)
				{
					// TODO: We should implement IAsyncDisposable instead really
					mMultiplexerLazy?.Task.Result.Dispose();
				}

				mIsDisposed = true;
			}
		}

		public void Dispose()
		{
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}
	}
}