using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Node_API.Util;
using RaftShared;
using System.Text.Json;
using System.Text;

namespace Node_API.Controllers;

[ApiController]
[Route("[controller]")]
public class RaftController : Controller
{
    private RaftHandler raftHandler;
    private LogHandler logHandler;
    private Node node;

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

    [HttpGet("GetLogEntry")]
    public RaftLogEntry GetLogEntry(string nodeID, int? term)
    {
        return logHandler.GetLogEntry(nodeID, FileType.NORMAL, term);
    }

    [HttpGet("StrongGetItem")]
    public async Task<RaftLogEntry> StrongGetItem(string nodeID)
    {
        List<RaftLogEntry> items = new();
        RaftLogEntry item = logHandler.GetLogEntry(nodeID, FileType.NORMAL, null);

        if (item != null)
        {
            items.Add(item);
        }

        var otherNodes = node.OtherNodes();
        foreach (var otherNode in otherNodes)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    string uri = $"http://{otherNode}/Raft/GetLogEntry?nodeID={nodeID}";
                    client.BaseAddress = new Uri(uri);

                    HttpResponseMessage response = await client.GetAsync("");
                    response.EnsureSuccessStatusCode();

                    var responseBody = await response.Content.ReadFromJsonAsync<RaftLogEntry>();
                    items.Add(responseBody);
                }
            }
            catch (Exception ex)
            {
            }
        }

        RaftLogEntry strongestItem = default;
        foreach (var i in items)
        {
            if (strongestItem == null || i.Term > strongestItem.Term)
            {
                strongestItem = i;
            }
        }

        return strongestItem;
    }

    [HttpGet("GetItems")]
    public IEnumerable<RaftItem> GetItems(string keyword){
       return logHandler.GetItemsFromLog(FileType.NORMAL, keyword);
    }

    [HttpGet("GetItem")]
    public RaftItem GetItem(string keyword)
    {
        return logHandler.GetItemFromLog(FileType.NORMAL, keyword);
    }

    [HttpGet("GetLeader")]
    public string GetLeader()
    {
        return node.CurrentLeader;
    }

    [HttpPost("SaveItem")]
    public async Task SaveItem([FromBody] RaftItem item)
    {
        if (node.IsLeader)
        {
            logHandler.AppendLogEntry(node.CurrentTerm, node.ThisNode(), item, FileType.NORMAL);
        }
        else
        {
            await ForwardSaveItem(item);
        }

    }

    private async Task ForwardSaveItem(RaftItem item)
    {
        try
        {
            string uri = $"http://{node.CurrentLeader}/Raft/SaveItem";

            using (HttpClient client = new HttpClient())
            {
                string jsonItem = JsonSerializer.Serialize(item);
                HttpContent content = new StringContent(jsonItem, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await client.PostAsync(uri, content);
                response.EnsureSuccessStatusCode();
            }
        }
        catch (Exception ex)
        {
        }
    }

    [HttpPost("UpdateLog")]
    public void UpdateLog([FromBody] List<RaftItem> items)
    {
        if(items.Count == 0)
        {
            return;
        }

        foreach(var item in items)
        {
            logHandler.AppendLogEntry(node.CurrentTerm, node.ThisNode(), item, FileType.NORMAL);
        }

    }

    [HttpPost("HeartBeat")]
    public void ReceivedHeartbeat(string leaderNode, int term)
    {
        raftHandler.OnLeaderHeartbeatReceived(leaderNode, term);
    }

}
