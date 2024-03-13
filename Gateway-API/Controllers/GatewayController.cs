using Microsoft.AspNetCore.Mvc;

namespace Gateway_API.Controllers;

[ApiController]
[Route("[controller]")]
public class GatewayController : Controller
{
    private List<string> nodes;
    private ILogger<GatewayController> logger;

    public GatewayController(IConfiguration configuration, ILogger<GatewayController> logger)
    {
        nodes =
        [
            configuration["NODE_ONE"] ?? "",
            configuration["NODE_TWO"] ?? "",
            configuration["NODE_THREE"] ?? "",
        ];

        this.logger = logger;
    }

    [HttpGet("PingNodes")]
    public async Task<IEnumerable<string>> PingNodes()
    {
        List<string> returnMessages = new();

        foreach(var node in nodes)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    string uri = $"http://{node}/Raft/ping";
                    client.BaseAddress = new Uri(uri);

                    HttpResponseMessage response = await client.GetAsync("");
                    response.EnsureSuccessStatusCode();

                    string responseBody = await response.Content.ReadAsStringAsync();
                    string message = $"{node}: {responseBody}";
                    returnMessages.Add(message);
                    logger.LogInformation(message);
                }
            }
            catch (Exception ex)
            {
                logger.LogError($"{node}: Error - {ex.Message}");
            }       
            
        }
        return returnMessages;
    }
    
}
