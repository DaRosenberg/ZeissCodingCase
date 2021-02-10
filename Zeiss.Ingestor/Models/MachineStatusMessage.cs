using System;
using System.Text.Json.Serialization;

namespace Zeiss.Ingestor.Models
{
	class MachineStatusMessage
	{
		[JsonConstructor]
		public MachineStatusMessage(Guid id, Guid machine_Id, DateTime timeStamp, MachineStatus status)
		{
			Id = id;
			Machine_Id = machine_Id;
			TimeStamp = timeStamp;
			Status = status;
		}

		// TODO: Fix these names, map JSON names to proper names
		public Guid Id { get; private set; }
		public Guid Machine_Id { get; private set; }
		public DateTime TimeStamp { get; private set; }
		public MachineStatus Status { get; private set; }
	}
}