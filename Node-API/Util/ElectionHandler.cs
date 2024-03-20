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
        string message = "";
        if(node.VoteCount >= votesNeeded)
        {
            node.CurrentLeader = node.ThisNode();
            node.IsLeader = true;
            node.IsCandidate = false;
            node.IsFollower = false;
            message = "Elected";
        }
        else
        {
            node.IsLeader = false;
            node.IsCandidate = false;
            node.IsFollower = true;
            message = $"Not Elected";
        }
        logger.LogInformation(message);
        logHandler.AppendLogEntry(node.CurrentTerm, node.ThisNode(), message, FileType.ERROR);
    }

}
