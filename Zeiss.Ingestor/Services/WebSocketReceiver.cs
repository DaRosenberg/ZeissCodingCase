using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Websocket.Client;
using Zeiss.Ingestor.Models;

namespace Zeiss.Ingestor.Services
{
	class WebSocketReceiver : IReceiver, IDisposable
	{
		public WebSocketReceiver(IConfiguration configuration, IEnumerable<IProcessor> processors)
		{
			var endpointUrlString = configuration["WebSocketEndpointUrl"];
			var endpointUrl = new Uri(endpointUrlString);

			mClient = new WebsocketClient(endpointUrl);
			mClient.ReconnectTimeout = TimeSpan.FromSeconds(30);

			mProcessors = processors;

			mMessageSubscription =
				mClient.MessageReceived
					.ObserveOn(TaskPoolScheduler.Default) // Ensure we're not blocking incoming messages
					.SelectParallelAsync((message, cancellationToken) => ProcessMessageAsync(message, cancellationToken), TaskPoolScheduler.Default)
					.Subscribe();

			mSerializerOptions =
				new JsonSerializerOptions()
				{
					PropertyNameCaseInsensitive = true,
					Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
				};
		}

		private readonly WebsocketClient mClient;
		private readonly IEnumerable<IProcessor> mProcessors;
		private readonly IDisposable mMessageSubscription;
		private readonly JsonSerializerOptions mSerializerOptions;
		private bool mIsDisposed;

		public async Task StartAsync(CancellationToken cancellationToken = default)
		{
			if (mIsDisposed)
			{
				throw new ObjectDisposedException(nameof(WebSocketReceiver));
			}

			await mClient.StartOrFail();

			Console.WriteLine($"Listering on {mClient.Url}.");
		}

		public async Task StopAsync(CancellationToken cancellationToken = default)
		{
			if (mIsDisposed)
			{
				throw new ObjectDisposedException(nameof(WebSocketReceiver));
			}

			var success = await mClient.StopOrFail(WebSocketCloseStatus.NormalClosure, statusDescription: null);
		}

		private async Task ProcessMessageAsync(ResponseMessage responseMessage, CancellationToken cancellationToken = default)
		{
			Console.WriteLine($"Receiving message:\n{responseMessage.Text}");

			try
			{
				if (responseMessage.MessageType == WebSocketMessageType.Text)
				{
					var messageEnvelope = JsonSerializer.Deserialize<MessageEnvelope<MachineStatusMessage>>(responseMessage.Text, mSerializerOptions);
					var message = messageEnvelope.Payload;

					// TODO: Could parallelize this for better perf with multiple processors, but we only have one now so...
					foreach (var processor in mProcessors)
					{
						cancellationToken.ThrowIfCancellationRequested();
						await processor.ProcessMessageAsync(message, cancellationToken);
					}
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error while receiving message: {ex.Message}");
			}
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!mIsDisposed)
			{
				if (disposing)
				{
					mMessageSubscription?.Dispose();
					mClient?.Dispose();
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