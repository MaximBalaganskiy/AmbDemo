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
using MimeKit;
using MailKit.Net.Smtp;

namespace AmbDemo
{
	public static class SendEmail
	{
		[FunctionName("SendEmail")]
		public static async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get", Route = null)]HttpRequest req, TraceWriter log, ExecutionContext context)
		{
			log.Info("C# HTTP trigger function processed a request.");
			var config = new ConfigurationBuilder()
				.SetBasePath(context.FunctionAppDirectory)
				.AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
				.AddJsonFile("local.settings.development.json", optional: true, reloadOnChange: true)
				.AddEnvironmentVariables()
				.Build();


			var msg = new MimeMessage();
			msg.From.Add(new MailboxAddress("Gold1043"));
			msg.To.Add(new MailboxAddress("m.balaganskiy+gold@gmail.com"));
			msg.Subject = "Bingo!";
			msg.Body = new TextPart("html") { Text = $"{req.Query["title"]}, {req.Query["artist"]}\n<a href='tel:0394141043'>Call!</a>" };

			using (var client = new SmtpClient())
			{
				await client.ConnectAsync(config["Email:Server"], int.Parse(config["Email:Port"]), MailKit.Security.SecureSocketOptions.Auto);
				await client.AuthenticateAsync(config["Email:User"], config["Email:Password"]);
				await client.SendAsync(msg);
				await client.DisconnectAsync(true);
			}
			return new OkObjectResult(null);
		}
	}
}
