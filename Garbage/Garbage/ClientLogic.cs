using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

using Api;

namespace Garbage
{
	public class ClientLogic
	{
		private HttpClient Client;

		private string _base;

		public ClientLogic(string basePath = @"http://127.0.0.1:5000/")
		{
			Client = new HttpClient();
			_base = basePath;
		}

		public async Task<HttpResponseMessage> PostAsync(string api, byte[] ser)
		{
			return await Client.Post(_base + api, ser);
		}

		public HttpResponseMessage Post(string api, byte[] ser)
		{
			return Task.Run(() => Client.Post(_base + api, ser)).Result;
		}
	}
}
