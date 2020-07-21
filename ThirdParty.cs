using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net.Http;
using System.Threading;

namespace CunaExercise
{
	public static class ThirdParty
	{

		private static readonly HttpClient HttpClient = new HttpClient();
		[FunctionName("ThirdParty")]
		public static async Task<IActionResult> Run(
			[HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
			ILogger log)
		{
			log.LogInformation("C# HTTP trigger function processed a request.");

			string body = req.Query["body"];
			string callback = req.Query["callback"];
			string batchid = req.Query["batchid"];
			string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
			dynamic data = JsonConvert.DeserializeObject(requestBody);
			body = body ?? data?.body;
			callback = callback ?? data?.callback;
			batchid = batchid ?? data?.batchid;
			string responseMessage = string.IsNullOrEmpty(body)
				? "This HTTP triggered function executed successfully. Pass a name in the query string or in the request body for a personalized response."
				: $"Hello,  body: {body}. This HTTP triggered function executed successfully.";

			var auditStatusCreaate = new AuditStatus { Status = "STARTED", Detail = body, BatchId = batchid };
			await PostToStart(auditStatusCreaate, callback);

			// Those code just simulate  the update status of the http put
			Thread.Sleep(5000);
			var auditStatus = new AuditStatus { Status = "PROCESSED", Detail = body, BatchId= batchid };
			await PutToStart(auditStatus, "http://localhost:7071/api/TrackingUpdate");

			return new OkObjectResult(responseMessage);
		}

		public static async Task<string> PostToStart(object json, string callBackURI)
		{
			using (var content = new StringContent(JsonConvert.SerializeObject(json), System.Text.Encoding.UTF8, "application/json"))
			{
				using (var response = await HttpClient.PostAsync(callBackURI, content))
				{
					if (!response.IsSuccessStatusCode)
						return null;

					var responseContent = await response.Content.ReadAsStringAsync();

					return responseContent;
				}
			}
		}

		public static async Task<string> PutToStart(object json, string putBackURI)
		{
			using (var content = new StringContent(JsonConvert.SerializeObject(json), System.Text.Encoding.UTF8, "application/json"))
			{
				using (var response = await HttpClient.PutAsync(putBackURI, content))
				{
					if (!response.IsSuccessStatusCode)
						return null;

					var responseContent = await response.Content.ReadAsStringAsync();

					return responseContent;
				}
			}
		}

	}
}
