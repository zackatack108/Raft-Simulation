namespace Node_API;

public class Node
{
    private string node;
    private List<string> otherNodes;
    private int totalNodes;

    public bool IsLeader { get; set; } = false;
    public bool IsFollower { get; set; } = false;
    public bool IsCandidate { get; set; } = false;
    public string CurrentLeader { get; set; } = "None";
    public int VoteCount { get; set; } = 0;
    public int CurrentTerm { get; set; } = 0;
    public DateTime LastHeartbeatTime { get; set; }

    public Node(IConfiguration configuration)
    {
        node = configuration["NODE_ONE"] ?? "";
        otherNodes = 
        [
            configuration["NODE_TWO"] ?? "",
            configuration["NODE_THREE"] ?? ""
        ];
        totalNodes = otherNodes.Count;
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
