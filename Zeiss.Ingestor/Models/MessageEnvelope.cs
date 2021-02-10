using System.Text.Json.Serialization;

namespace Zeiss.Ingestor.Models
{
	class MessageEnvelope<T>
	{
		[JsonConstructor]
		public MessageEnvelope(string topic, T payload)
		{
			Topic = topic;
			Payload = payload;
		}

		public string Topic { get; private set; }
		public T Payload { get; private set; }
	}
}