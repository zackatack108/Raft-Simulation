using Microsoft.AspNetCore.Mvc;
using RaftShared;
using System.Text;
using System.Text.Json;

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

    [HttpGet("GetItems")]
    public async Task<IEnumerable<RaftItem>> GetItems(string keyword)
    {
        List<RaftItem> items = new();

        try
        {
            Random random = new();
            string selectedNode = nodes[random.Next(nodes.Count)];

            string uri = $"http://{selectedNode}/Raft/GetItems?keyword={keyword}";

            using (HttpClient client = new())
            {
                HttpResponseMessage response = await client.GetAsync(uri);
                response.EnsureSuccessStatusCode();

                string responseBody = await response.Content.ReadAsStringAsync();
                items = JsonSerializer.Deserialize<List<RaftItem>>(responseBody);
            }

            logger.LogInformation($"Successfully retrieved items from {selectedNode} for keyword '{keyword}'");

        }
        catch(Exception ex)
        {
            logger.LogError($"Error retreiving items: {ex.Message}");
        }
        return items;
    }

    [HttpGet("GetItem")]
    public async Task<RaftItem> GetItem(string keyword)
    {
        RaftItem item = new();

        try
        {
            Random random = new();
            string selectedNode = nodes[random.Next(nodes.Count)];

            string uri = $"http://{selectedNode}/Raft/GetItem?keyword={keyword}";

            using (HttpClient client = new())
            {
                HttpResponseMessage response = await client.GetAsync(uri);
                response.EnsureSuccessStatusCode();

                string responseBody = await response.Content.ReadAsStringAsync();
                item = JsonSerializer.Deserialize<RaftItem>(responseBody);
            }

            logger.LogInformation($"Successfully retrieved item from {selectedNode} for keyword '{keyword}'");

        }
        catch (Exception ex)
        {
            logger.LogError($"Error retreiving item: {ex.Message}");
        }
        return item;
    }

    [HttpGet("StrongGetItems")]
    public async Task<IEnumerable<RaftItem>> GetItemsFromLeader(string keyword)
    {
        try
        {
            string leaderNode = await GetLeaderNode();

            if (string.IsNullOrEmpty(leaderNode))
            {
                logger.LogError("No leader node found.");
                return new List<RaftItem>();
            }

            string uri = $"http://{leaderNode}/Gateway/GetItems?keyword={keyword}";

            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage response = await client.GetAsync(uri);
                response.EnsureSuccessStatusCode();

                string responseBody = await response.Content.ReadAsStringAsync();
                List<RaftItem> items = JsonSerializer.Deserialize<List<RaftItem>>(responseBody);

                logger.LogInformation($"Successfully retrieved items from the leader node '{leaderNode}' for keyword '{keyword}'");

                return items;
            }
        }
        catch (Exception ex)
        {
            logger.LogError($"Error retrieving items from leader node: {ex.Message}");
            return new List<RaftItem>();
        }
    }

    [HttpGet("StrongGetItem")]
    public async Task<RaftItem> GetItemFromLeader(string keyword)
    {
        try
        {
            string leaderNode = await GetLeaderNode();

            if (string.IsNullOrEmpty(leaderNode))
            {
                logger.LogError("No leader node found.");
                return new RaftItem();
            }

            string uri = $"http://{leaderNode}/Gateway/GetItem?keyword={keyword}";

            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage response = await client.GetAsync(uri);
                response.EnsureSuccessStatusCode();

                string responseBody = await response.Content.ReadAsStringAsync();
                RaftItem item = JsonSerializer.Deserialize<RaftItem>(responseBody);

                logger.LogInformation($"Successfully retrieved item from the leader node '{leaderNode}' for keyword '{keyword}'");

                return item;
            }
        }
        catch (Exception ex)
        {
            logger.LogError($"Error retrieving item from leader node: {ex.Message}");
            return new RaftItem();
        }
    }

    private async Task<string> GetLeaderNode()
    {
        try
        {
            foreach (var node in nodes)
            {
                string uri = $"http://{node}/Raft/GetLeader";
                using (HttpClient client = new HttpClient())
                {
                    HttpResponseMessage response = await client.GetAsync(uri);
                    response.EnsureSuccessStatusCode();

                    string responseBody = await response.Content.ReadAsStringAsync();

                    if (!string.IsNullOrEmpty(responseBody))
                    {
                        return responseBody;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError($"Error getting leader node: {ex.Message}");
        }

        return null;
    }

    [HttpPost("SaveItem")]
    public async Task<IActionResult> SaveItemToLeader([FromBody] RaftItem item)
    {
        string leaderNode = await GetLeaderNode();
        try
        {

            if (string.IsNullOrEmpty(leaderNode))
            {
                logger.LogError("No leader node found.");
                return StatusCode(StatusCodes.Status500InternalServerError, "No leader node found.");
            }

            string uri = $"http://{leaderNode}/Raft/SaveItem";

            using (HttpClient client = new HttpClient())
            {
                string jsonItem = JsonSerializer.Serialize(item);
                HttpContent content = new StringContent(jsonItem, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await client.PostAsync(uri, content);
                response.EnsureSuccessStatusCode();

                logger.LogInformation($"Item saved to leader node '{leaderNode}'");
                return Ok();
            }
        }
        catch (Exception ex)
        {
            logger.LogError($"Error saving item to leader node {leaderNode}: {ex.Message}");
            return StatusCode(StatusCodes.Status500InternalServerError, "Error saving item to leader node.");
        }
    }

}
