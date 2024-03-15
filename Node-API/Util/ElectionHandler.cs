namespace Node_API.Util;

public class ElectionHandler
{
    private Node node;
    private LogHandler logHandler;
    private ILogger<ElectionHandler> logger;

    public ElectionHandler(Node node, LogHandler logHandler, ILogger<ElectionHandler> logger)
    {
        this.node = node;
        this.logger = logger;
        this.logHandler = logHandler;
    }

    public async void StartElection()
    {
        node.IsFollower = false;
        node.IsLeader = false;
        node.IsCandidate = true;

        node.CurrentTerm++;
        node.VoteCount = 1;
        var otherNodes = node.OtherNodes();
        foreach(var otherNode in otherNodes)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    string uri = $"http://{otherNode}/Election/VoteFor?candidate={node.ThisNode()}&term={node.CurrentTerm}";
                    client.BaseAddress = new Uri(uri);

                    HttpResponseMessage response = await client.GetAsync("");
                    response.EnsureSuccessStatusCode();

                    bool responseBody = await response.Content.ReadFromJsonAsync<bool>();
                    if (responseBody)
                    {
                        node.VoteCount++;
                    }
                }
            }
            catch(Exception ex)
            {
                logger.LogError(ex.Message);
            }
        }

        CheckElection();
    }

    public void CheckElection()
    {
        int votesNeeded = (node.TotalNodes() / 2) + 1;
        (string, object, int) logMessage;
        string message = "";
        if(node.VoteCount >= votesNeeded)
        {
            node.CurrentLeader = node.ThisNode();
            node.IsLeader = true;
            node.IsCandidate = false;
            node.IsFollower = false;
            message = $"Node: {node.ThisNode()} elected for term: {node.CurrentTerm}";
            logMessage = (message, true, 1);
        }
        else
        {
            node.IsLeader = false;
            node.IsCandidate = false;
            node.IsFollower = true;
            message = $"Node: {node.ThisNode()} not elected for term: {node.CurrentTerm}";
            logMessage = (message, false, 1);
        }
        logger.LogInformation(message);
        logHandler.WriteLog(logMessage, FileType.ELECTION);
    }

}
