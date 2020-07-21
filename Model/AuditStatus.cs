using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace CunaExercise
{
	public class AuditStatus
	{
		[JsonProperty(PropertyName = "id")]
		public string id { get; set; }
		[JsonProperty(PropertyName = "status")]
		public string Status { get; set; }
		[JsonProperty(PropertyName = "detail")]
		public string Detail { get; set; }
		[JsonProperty(PropertyName = "batchid")]
		public string BatchId { get; set; }
		[JsonProperty(PropertyName = "timestamp")]
		public DateTime TimeStamp { get; set; }

		public override string ToString()
		{
			return JsonConvert.SerializeObject(this);
		}
	}
}
