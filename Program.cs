using jiraWebhookTest2;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);
if (builder.Environment.IsDevelopment())
  builder.Services.AddHostedService<TunnelService>();
var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.MapPost("/jirawebhook", ([FromBody] JiraWebhookPayload body) =>
{
  var jsonString = JsonSerializer.Serialize(body, new JsonSerializerOptions { PropertyNameCaseInsensitive = true, WriteIndented = true });
  Console.WriteLine(jsonString);
});

app.MapGet("/jiraRestTest", async (IConfiguration configuration) =>
{
  var consumerKey = configuration.GetValue<string>("Atlassian:consumerKey");
  var accessToken = configuration.GetValue<string>("Atlassian:accessToken");
  var tokenSecret = configuration.GetValue<string>("Atlassian:tokenSecret");
  var consumerSecret = configuration.GetValue<string>("Atlassian:consumerSecret");

  var client = Atlassian.Jira.Jira.CreateOAuthRestClient("https://shockz.atlassian.net",
    consumerKey,
    consumerSecret,
    accessToken,
    tokenSecret);

  var issue = await client.Issues.GetIssueAsync("FTP-1");

  return Results.Ok(new string[] { issue.Key.Value, issue.Summary, issue.Status.Name });
});

app.Run();
