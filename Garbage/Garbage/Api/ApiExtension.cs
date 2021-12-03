using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Api
{
	/// <summary>Статический класс расширения запросов.</summary>
	public static class ApiExtension
	{
		/// <summary>Пост запрос.</summary>
		/// <param name="client">Управляемый клиент.</param>
		/// <param name="request">Запрос.</param>
		/// <param name="stream">Контекст запроса.</param>
		/// <returns>Ответ запроса.</returns>
		public static async Task<HttpResponseMessage> Post(this HttpClient client, string request, byte[] stream)
		{
			string res = string.Format(request);
			return await client.PostAsync(res, new ByteArrayContent(stream));
		}
	}
}