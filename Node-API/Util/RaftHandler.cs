using Microsoft.Extensions.Logging;

namespace Node_API.Util;

public class RaftHandler
{
    private readonly Node node;
    private readonly ElectionHandler electionHandler;
    private readonly LogHandler logHandler;
    private readonly ILogger<RaftHandler> logger;
    private readonly Random random = new Random();
    private readonly TimeSpan minHeartBeatInterval = TimeSpan.FromSeconds(5);

    public RaftHandler(Node node, ElectionHandler electionHandler, LogHandler logHandler, ILogger<RaftHandler> logger)
    {
        this.node = node;
        this.electionHandler = electionHandler;
        this.logHandler = logHandler;
        this.logger = logger;
        StartElectionIfNoResponse();
        _ = StartPulse();
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
                            string uri = $"http://{otherNode}/Raft/HeartBeat";
                            client.BaseAddress = new Uri(uri);

                            HttpResponseMessage response = await client.GetAsync("");
                            response.EnsureSuccessStatusCode();
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex.Message);
                    }
                }
            }

            Thread.Sleep(random.Next(500, 2500));
        }
    }

    public void StartElectionIfNoResponse()
    {
        int randomTimeout = random.Next(5000, 10000);
        Thread.Sleep(randomTimeout);

        if(!node.IsLeader && node.IsCandidate)
        {
            logger.LogInformation($"Node: {node.ThisNode} started an election for term {node.CurrentTerm}");
            electionHandler.StartElection();
        }
    }

    public void OnLeaderHeartbeatReceived(string leader)
    {
        if (!node.IsLeader)
        {
            logger.LogInformation($"Node: {node.ThisNode} received heartbeat from {leader}");
            node.CurrentLeader = leader;

            node.IsLeader = false;
            node.IsCandidate = false;
            node.IsFollower = true;

            Thread heartBeatResetThread = new Thread(StartElectionIfNoResponse);
            heartBeatResetThread.Start();
        }
    }

}
