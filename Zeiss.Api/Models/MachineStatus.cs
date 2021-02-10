using System;

namespace Zeiss.Api.Models
{
	public class MachineStatus
	{
		public MachineStatus(Guid machineId, string status)
		{
			MachineId = machineId;
			Status = status;
		}

		public Guid MachineId { get; private set; }
		public string Status { get; private set; }		
	}
}