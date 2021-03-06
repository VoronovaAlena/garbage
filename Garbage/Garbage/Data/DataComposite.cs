using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data
{
	public class BinaryData
	{
		[JsonProperty("boxes")]
		public List<int[]> boxes { get; set; }

		[JsonProperty("class_names")]
		public string[] classes { get; set; }
	}

	/// <summary>Вспомогательный класс для работы с Json.</summary>
	public static class JsonParse
	{
		public static T Deserialize<T>(string data) where T : class
		{
			return JsonConvert.DeserializeObject<T>(data);
		}

		public static string Serialize<T>(T data)
		{
			return JsonConvert.SerializeObject(data);
		}
	}
}