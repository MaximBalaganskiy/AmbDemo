using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using System.Net.Http;
using System;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using System.Text;
using System.Configuration;
using Microsoft.Extensions.Configuration;

namespace AmbDemo
{
	public static class GetAccessToken
	{
		[FunctionName("GetAccessToken")]
		public static async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get", Route = null)]HttpRequest req, TraceWriter log, ExecutionContext context)
		{
			log.Info("C# HTTP trigger function processed a request.");

			var config = new ConfigurationBuilder()
				.SetBasePath(context.FunctionAppDirectory)
				.AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
				.AddJsonFile("local.settings.development.json", optional: true, reloadOnChange: true)
				.AddEnvironmentVariables()
				.Build();

			string accessCode = req.Query["access_code"];
			string state = req.Query["state"];

			if (string.IsNullOrEmpty(accessCode) || string.IsNullOrEmpty(state))
			{
				return new BadRequestObjectResult(new { error = "Invalid request" });
			}

			var client = new HttpClient { BaseAddress = new Uri("https://github.com/login/oauth/") };
			client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
			var request = new AccessTokenRequest
			{
				client_id = config["Github:ClientId"],
				client_secret = config["Github:ClientSecret"],
				code = accessCode,
				state = config["Github:State"]
			};
			var response = await client.PostAsync("access_token", new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json"));
			if (!response.IsSuccessStatusCode)
			{
				return new BadRequestObjectResult(new { error = "Invalid request" });
			}
			var content = await response.Content.ReadAsAsync<AccessTokenResponse>();
			if (string.IsNullOrEmpty(content.access_token))
			{
				return new BadRequestObjectResult(new { error = "Invalid request" });
			}
			return new OkObjectResult(content);
		}
	}
}
