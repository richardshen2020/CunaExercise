using CunaExercise;

using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Configuration;
using System.Threading;
using System.Threading.Tasks;

[assembly: FunctionsStartup(typeof(Startup))]
namespace CunaExercise
{
	public class Startup : FunctionsStartup
	{
		static string databaseName = "DemoStatus" ;
		static string containerName = "DemoStatusContainer" ;
		static string account = "https://cosmoscunademo.documents.azure.com:443/";
		static string key = "DERmonz13VaMU331iz7mvxT3xRkWo7vNdsNp4fA1kOOqEbmI17ElUUAsv3ua8GIOuA3VCJ4xwY0bSCRCTqEcnQ==";

		public override void Configure(IFunctionsHostBuilder builder)
		{
			var cosmosDbService = InitializeCosmosClientInstanceAsync();

	
//await cosmosDbService.AddAuditStatusAsync(a);
			builder.Services.AddSingleton(cosmosDbService);
			//builder.Services.AddTransient<ICosmosDbService, CosmosDbService>();
		}

		public Startup()
		{


			
		}
		/// <summary>
		/// Creates a Cosmos DB database and a container with the specified partition key. 
		/// </summary>
		/// <returns></returns>
		private static async Task<ICosmosDbService> InitializeCosmosClientInstanceAsync()
		{

			Microsoft.Azure.Cosmos.CosmosClient client = new Microsoft.Azure.Cosmos.CosmosClient(account, key);
			CosmosDbService cosmosDbService = new CosmosDbService(client, databaseName, containerName);

			AuditStatus a = new AuditStatus { id = "0C805497-2BA1-4524-9D41-E5D556488F9D", BatchId = "C8F9F1C1-4BD1-447F-9F0E-D8E96DF8A1E7", Status = "start", TimeStamp = DateTime.Now };

		
			Microsoft.Azure.Cosmos.DatabaseResponse database = await client.CreateDatabaseIfNotExistsAsync(databaseName);
			await database.Database.CreateContainerIfNotExistsAsync(containerName, "/id");
			await cosmosDbService.AddAuditStatusAsync(a);
			return cosmosDbService;
		}
 
	}
	//public interface IRepository
	//{
	//	string GetData();
	//}
	//public class Repository : IRepository
	//{
	//	public string GetData()
	//	{
	//		return "some data!";
	//	}
	//}
}