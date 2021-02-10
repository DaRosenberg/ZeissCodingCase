using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Nito.AsyncEx;
using StackExchange.Redis;
using Zeiss.Api.Models;

namespace Zeiss.Api.Controllers
{
	[ApiController]
	[Route("machines")]
	public class MachineStatusController : ControllerBase
	{
		public MachineStatusController(IConfiguration configuration)
		{
			var connectionString = configuration["RedisConnectionString"];
			mMultiplexerLazy = new AsyncLazy<ConnectionMultiplexer>(() => ConnectionMultiplexer.ConnectAsync(connectionString));
		}

		private readonly AsyncLazy<ConnectionMultiplexer> mMultiplexerLazy;

		[HttpGet]
		public async Task<IEnumerable<MachineStatus>> GetAsync()
		{
			var redis = await mMultiplexerLazy;
			var db = redis.GetDatabase();
			
			var entries = await db.HashGetAllAsync("MachineStatus");

			var result =
				entries
					.Select(entry => new MachineStatus(Guid.Parse(entry.Name), entry.Value))
					.ToArray();

			return result;
		}
	}
}
