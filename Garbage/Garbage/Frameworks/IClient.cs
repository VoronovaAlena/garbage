using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Garbage.Frameworks
{
	/// <summary>Клиент для универсальных взаимодействий.</summary>
	public interface IClient
	{
		/// <summary>Асинхронный запрос.</summary>
		Task<HttpResponseMessage> SendAsync(string head, byte[] data);

		/// <summary>Запрос.</summary>
		HttpResponseMessage Send(string head, byte[] data);
		
	}
}
