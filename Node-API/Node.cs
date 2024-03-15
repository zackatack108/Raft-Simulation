namespace Node_API;

public class Node
{
    private readonly string node;
    private readonly List<string> otherNodes;
    private readonly int totalNodes;

    public bool IsLeader { get; set; }
    public bool IsFollower { get; set; }
    public bool IsCandidate { get; set; }
    public string CurrentLeader { get; set; }
    public int VoteCount { get; set; }
    public int CurrentTerm { get; set; }

    public Node(IConfiguration configuration, ILogger<Node> logger)
    {
        node = configuration["NODE_ONE"] ?? "";
        otherNodes = 
        [
            configuration["NODE_TWO"] ?? "",
            configuration["NODE_THREE"] ?? ""
        ];
        totalNodes = otherNodes.Count;
        logger.LogInformation($"Node 1: {node}, Node 2: {otherNodes[0]}, Node 3: {otherNodes[1]}");
    }

    public List<string> OtherNodes()
    {
        return otherNodes;
    }

    public string ThisNode()
    {
        return node;
    }

    public int TotalNodes()
    {
        return totalNodes;
    }
}
