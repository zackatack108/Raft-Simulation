using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Node_API.Util;

namespace Node_API.Controllers;

[ApiController]
[Route("[controller]")]
public class RaftController : Controller
{
    private readonly RaftHandler raftHandler;
    private readonly LogHandler logHandler;
    private readonly Node node;

    public RaftController(Node node, RaftHandler raftHandler, LogHandler logHandler)
    {
        this.node = node;
        this.raftHandler = raftHandler;
        this.logHandler = logHandler;
    }

    [HttpGet("ping")]
    public string Ping()
    {
        return "Pong";
    }

    [HttpGet("GetItem")]
    public object GetItem(string key)
    {
        return logHandler.GetLogItem(key, FileType.NORMAL);
    }

    [HttpGet("GetItemWithVersion")]
    public (object, int) GetItemWithVersion(string key)
    {
        return logHandler.GetLogItemWithVersion(key, FileType.NORMAL);
    }

    [HttpGet("StrongGetItem")]
    public async Task<object> StrongGetItem(string key)
    {
        List<(object Value, int Version)> items = new();
        items.Add(logHandler.GetLogItemWithVersion(key, FileType.NORMAL));

        var otherNodes = node.OtherNodes();
        foreach (var otherNode in otherNodes)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    string uri = $"http://{otherNode}/Raft/GetItemWithVersion?key={key}";
                    client.BaseAddress = new Uri(uri);

                    HttpResponseMessage response = await client.GetAsync("");
                    response.EnsureSuccessStatusCode();

                    var responseBody = await response.Content.ReadFromJsonAsync<(object, int)>();
                    items.Add(responseBody);                    
                }
            }
            catch (Exception ex)
            {
            }
        }

        (object Value, int Version) strongestItem = default;
        foreach(var item in items)
        {
            if (item.Version > strongestItem.Version)
            {
                strongestItem = item;
            }
        }

        return strongestItem.Value;
    }

    [HttpPost("SaveItem")]
    public async Task SaveItem(string key, object value)
    {
        if (node.IsLeader)
        {
            (string, object, int) item = (key, value, 1);
            logHandler.WriteLog(item, FileType.NORMAL);
        }
        else
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    string uri = $"http://{node.CurrentLeader}/Raft/SaveItem?key={key}&object={value}";
                    client.BaseAddress = new Uri(uri);

                    HttpResponseMessage response = await client.PostAsync("", null);
                    response.EnsureSuccessStatusCode();
                }
            }
            catch (Exception ex)
            {
            }
        }

    }

    [HttpPost("UpdateLog")]
    public void UpdateLog((string, object, int) logEntry)
    {
        logHandler.WriteLog(logEntry, FileType.NORMAL);
    }

    [HttpPost("HeartBeat")]
    public void ReceivedHeartbeat(string leaderNode)
    {
        raftHandler.OnLeaderHeartbeatReceived(leaderNode);
    }

}
