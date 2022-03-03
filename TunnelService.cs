using CliWrap;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using System.Text.Json.Nodes;

namespace jiraWebhookTest2
{
  public class TunnelService : BackgroundService
  {
    private readonly IServer _server;
    private readonly IHostApplicationLifetime _hostApplicationLifetime;
    private readonly IConfiguration _configuration;
    private readonly ILogger<TunnelService> _logger;

    public TunnelService(IServer server, IHostApplicationLifetime hostApplicationLifetime, IConfiguration configuration, ILogger<TunnelService> logger)
    {
      _server = server ?? throw new ArgumentNullException(nameof(server));
      _hostApplicationLifetime = hostApplicationLifetime ?? throw new ArgumentNullException(nameof(hostApplicationLifetime));
      _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
      _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
      await WaitForApplicationStarted();

      var urls = _server.Features.Get<IServerAddressesFeature>()!.Addresses;

      var localUrl = urls.Single(u => u.StartsWith("https://"));
      _logger.LogInformation("Starting ngrok tunnel for {LocalUrl}", localUrl);

      var ngrokTask = StartNgrokTunnel(localUrl, stoppingToken);

      var publicUrl = await GetNgrokPublicUrl();
      _logger.LogInformation("Public ngrok URL: {NgrokPublicUrl}", publicUrl);

      try
      {
        await ngrokTask;
        _logger.LogInformation("Ngrok tunnel stopped");
      }
      finally
      {
        ngrokTask.Dispose();
      }
    }

    private async Task<string> GetNgrokPublicUrl()
    {
      using var httpClient = new HttpClient();
      for (int i = 0; i < 10; i++)
      {
        _logger.LogDebug("Get ngrok tunnels attempt: {RetryCount}", i + 1);

        try
        {
          var json = await httpClient.GetFromJsonAsync<JsonNode>("http://127.0.0.1:4040/api/tunnels");
          var publicUrl = json["tunnels"].AsArray()
            .Select(e => e["public_url"].GetValue<string>())
            .SingleOrDefault(u => u.StartsWith("https://"));

          if (!string.IsNullOrEmpty(publicUrl)) return publicUrl;
        }
        catch
        {
          // ignored
        }

        await Task.Delay(200);
      }

      throw new Exception("Ngrok dashboard did not start in 10 tries");
    }

    private CommandTask<CommandResult> StartNgrokTunnel(string localUrl, CancellationToken stoppingToken)
    {
      var ngrokTask = Cli.Wrap("ngrok")
        .WithArguments(args => args
          .Add("http")
          .Add(localUrl)
          .Add("--log")
          .Add("stdout"))
        .WithStandardOutputPipe(PipeTarget.ToDelegate(s => _logger.LogDebug(s)))
        .WithStandardErrorPipe(PipeTarget.ToDelegate(s => _logger.LogError(s)))
        .WithValidation(CommandResultValidation.None)
        .ExecuteAsync(stoppingToken);

      return ngrokTask;
    }

    private Task WaitForApplicationStarted()
    {
      var completionSource = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
      _hostApplicationLifetime.ApplicationStarted.Register(() => completionSource.TrySetResult());

      return completionSource.Task;
    }
  }
}
