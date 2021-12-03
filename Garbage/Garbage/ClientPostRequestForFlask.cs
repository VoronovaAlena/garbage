using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

using Api;
using Garbage.Frameworks;

namespace Garbage
{
	/// <summary>Класс клиент для упрощённого взаимодействия.</summary>
	public class ClientPostRequestForFlask : IClient
	{
		private HttpClient Client;

		private string _base;

		/// <summary>Создаёт пост клиент для взаимодействия с сервисом Flask на localhost по стационарному порту.</summary>
		/// <param name="basePath"></param>
		public ClientPostRequestForFlask(string basePath = @"http://127.0.0.1:5000/")
		{
			Client = new HttpClient();
			_base = basePath;
		}

		public async Task<HttpResponseMessage> SendAsync(string api, byte[] ser)
		{
			return await Client.Post(_base + api, ser);
		}

		public HttpResponseMessage Send(string api, byte[] ser)
		{
			return Task.Run(() => Client.Post(_base + api, ser)).Result;
		}
	}
}
