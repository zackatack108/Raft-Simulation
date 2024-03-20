using Microsoft.Extensions.Logging;

namespace Node_API.Util;

public class RaftHandler
{
    private Node node;
    private ElectionHandler electionHandler;
    private LogHandler logHandler;
    private ILogger<RaftHandler> logger;
    private Random random = new Random();
    private TimeSpan minHeartBeatInterval = TimeSpan.FromSeconds(1000);

    public RaftHandler(Node node, ElectionHandler electionHandler, LogHandler logHandler, ILogger<RaftHandler> logger)
    {
        this.node = node;
        this.electionHandler = electionHandler;
        this.logHandler = logHandler;
        this.logger = logger;     
    }

    public void Initialize()
    {
        Task.Run(StartPulse);
        this.logger.LogInformation("Raft handler Initialized");
    }

    public async Task StartPulse()
    {
        while (true)
        {

            if (node.IsLeader)
            {
                var otherNodes = node.OtherNodes();
                foreach (var otherNode in otherNodes)
                {
                    try
                    {
                        using (HttpClient client = new HttpClient())
                        {
                            string uri = $"http://{otherNode}/Raft/HeartBeat?leaderNode={node.ThisNode()}&term={node.CurrentTerm}";
                            client.BaseAddress = new Uri(uri);

                            HttpResponseMessage response = await client.PostAsync("", null);
                            response.EnsureSuccessStatusCode();
                        }
                        logger.LogInformation($"Node: {node.ThisNode()}, Sent Heartbeat to node: {otherNode}, Term: {node.CurrentTerm}");
                        logHandler.AppendLogEntry(node.CurrentTerm, node.ThisNode(), "Sent Heartbeat", FileType.NORMAL);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError($"Error sending heartbeat - {ex.Message}");
                    }
                }
            }

            StartElectionIfNoResponse();

            //logger.LogInformation($"Node: {node.ThisNode()}, Leader: {node.CurrentLeader}, Follower: {node.IsFollower}");
            Thread.Sleep(random.Next(2000,10000));
        }
    }

    public void StartElectionIfNoResponse()
    {
        if(!node.IsLeader && DateTime.UtcNow - node.LastHeartbeatTime > TimeSpan.FromSeconds(random.Next(5,10)))
        {
            logger.LogInformation($"Node: {node.ThisNode()} detected leader {node.CurrentLeader} is unavailable. Starting election for term {node.CurrentTerm + 1}");
            logHandler.AppendLogEntry(node.CurrentTerm + 1, node.ThisNode(), "Started Election", FileType.NORMAL);
            electionHandler.StartElection();
        }
    }

    public void OnLeaderHeartbeatReceived(string leader, int term)
    {
        logger.LogInformation($"Node: {node.ThisNode()} received heartbeat from {leader} for term {term}");
        logHandler.AppendLogEntry(term, node.ThisNode(), "Received Heartbeat", FileType.NORMAL);
        node.CurrentLeader = leader;
        node.CurrentTerm = term;

        node.IsLeader = false;
        node.IsCandidate = false;
        node.IsFollower = true;

        node.LastHeartbeatTime = DateTime.UtcNow;
    }

}
