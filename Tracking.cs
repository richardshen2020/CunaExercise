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
using System.Collections.Generic;
using System.Text;

namespace CunaExercise
{
	public static class Tracking
	{
		static string Callback = "http://localhost:7071/api/TrackingCreate";
		static string ThirdPartyURI = "http://localhost:7071/api/ThirdParty";

		private static readonly HttpClient HttpClient = new HttpClient();
		[FunctionName("PostForward")]
		public static async Task<IActionResult> PostForward(
			[HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
			ILogger log)
		{
			log.LogInformation("C# HTTP trigger function processed a request.");

			string body = req.Query["body"];

			string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
			dynamic data = JsonConvert.DeserializeObject(requestBody);
			Guid guid = Guid.NewGuid();
			data.batchid = guid.ToString();
			data.callback = Callback;
			body = body ?? data?.body;

			string result = await PostToThirdParty(data);

			string responseMessage = string.IsNullOrEmpty(body)
				? "This HTTP triggered function executed successfully. Pass a body in the query string or in the request body for a personalized response."
				: $"Hello, {body}. This HTTP triggered function executed successfully. The reference id: {guid.ToString()}";

			return new OkObjectResult(responseMessage);
		}

		[FunctionName("TrackingCreate")]
		public static ActionResult<object> TrackingCreate(
			[HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
			 [CosmosDB("DemoStatus", "AuditStatus", Id = "id", ConnectionStringSetting = "CosmosDBConn", CreateIfNotExists = true, PartitionKey = "/Status")] out dynamic document,
			ILogger log)
		{
			log.LogInformation("C# HTTP trigger function processed a request.");

			string batchid = req.Query["batchid"];
			string body = req.Query["body"];
			string requestBody = new StreamReader(req.Body).ReadToEnd();
			dynamic data = JsonConvert.DeserializeObject(requestBody);
			batchid = batchid ?? data?.batchid;
			body = body ?? data?.body;

			document = new AuditStatus { id = Guid.NewGuid().ToString(), BatchId = batchid, Status = "STARTED", TimeStamp = DateTime.Now, Detail = body };

			string responseMessage = string.IsNullOrEmpty(body)
				? "This HTTP triggered function executed successfully. Pass a body in the query string or in the request body for a personalized response."
				: $"Hello, {body}. This HTTP triggered function executed successfully.";

			return new OkObjectResult(body);
		}

		[FunctionName("TrackingUpdate")]
		public static ActionResult<object> TrackingUpdate(
		[HttpTrigger(AuthorizationLevel.Function, "put", Route = null)] HttpRequest req,
		 [CosmosDB("DemoStatus", "AuditStatus", Id = "id", ConnectionStringSetting = "CosmosDBConn", CreateIfNotExists = true, PartitionKey = "/Status")] out dynamic document,
		ILogger log)
		{
			log.LogInformation("C# HTTP trigger function processed a request.");

			string batchid = req.Query["batchid"];
			string status = req.Query["status"];
			string detail = req.Query["detail"];

			string requestBody = new StreamReader(req.Body).ReadToEnd();
			dynamic data = JsonConvert.DeserializeObject(requestBody);
			batchid = batchid ?? data?.batchid;
			status = status ?? data?.status;
			detail = detail ?? data?.detail;

			document = new AuditStatus { id = Guid.NewGuid().ToString(), BatchId = batchid, Status = status, TimeStamp = DateTime.Now, Detail = detail };

			string responseMessage = string.IsNullOrEmpty(detail)
				? "This HTTP triggered function executed successfully. Pass a body in the query string or in the request body for a personalized response."
				: $"Hello, {detail}. This HTTP triggered function executed successfully.";

			return new OkObjectResult(responseMessage);
		}

		[FunctionName("TrackingByBatchId")]
		public static async Task<IActionResult> TrackingByBatchId(
		[HttpTrigger(AuthorizationLevel.Function, "get", Route = "TrackingByBatchId/{batchid}")] HttpRequest req,
		 [CosmosDB("DemoStatus", "AuditStatus",  ConnectionStringSetting = "CosmosDBConn", SqlQuery="SELECT * FROM auditstatus R where R.batchid= {batchid}")]
				 IEnumerable<AuditStatus>  Items,
		ILogger log)
		{
			log.LogInformation("C# HTTP trigger function processed a request.");

			StringBuilder sb = new StringBuilder();
			foreach (AuditStatus i in Items)
			{
				sb.Append(i.id);
				sb.Append("\t");
				sb.Append(i.BatchId);
				sb.Append("\t");
				sb.Append(i.Detail);
				sb.Append("\t");
				sb.Append(i.TimeStamp);
				sb.Append("\t");
				sb.Append(i.Status);
				sb.Append("\n");
			}

			string responseMessage = string.IsNullOrEmpty(sb.ToString())
				? "This HTTP triggered function executed successfully. nothing found"
				: sb.ToString();

			return new OkObjectResult(responseMessage);
		}


		[FunctionName("TrackingAll")]
		public static async Task<IActionResult> TrackingAll(
								[HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req,
								[CosmosDB("DemoStatus", "AuditStatus",  ConnectionStringSetting = "CosmosDBConn", SqlQuery="SELECT * FROM auditstatus")]
												IEnumerable<AuditStatus>  Items,
									ILogger log)
		{
			log.LogInformation("C# HTTP trigger function processed a request.");

			StringBuilder sb = new StringBuilder();
			foreach (AuditStatus i in Items)
			{
				sb.Append(i.id);
				sb.Append("\t");
				sb.Append(i.BatchId);
				sb.Append("\t");
				sb.Append(i.Detail);
				sb.Append("\t");
				sb.Append(i.TimeStamp);
				sb.Append("\t");
				sb.Append(i.Status);
				sb.Append("\n");
			}

			string responseMessage = string.IsNullOrEmpty(sb.ToString())
				? "This HTTP triggered function executed successfully. nothing found"
				: sb.ToString();

			return new OkObjectResult(responseMessage);
		}

		public static async Task<string> PostToThirdParty(object json)
		{
			using (var content = new StringContent(JsonConvert.SerializeObject(json), System.Text.Encoding.UTF8, "application/json"))
			{
				using (var response = await HttpClient.PostAsync(ThirdPartyURI, content))
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
