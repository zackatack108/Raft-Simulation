using System.Collections.Concurrent;

namespace Raft_Simulation;

public class RaftNode
{
    private readonly object lockObject = new object();
    private static ConcurrentDictionary<(int, int), int> nodeVotes = new();
    private static ConcurrentDictionary<(int, int), int> votedFor = new();
    private static readonly List<RaftNode> nodes = new List<RaftNode>();
    private static int currentTerm = 0;
    private static int leaderId = -1;

    private Thread thread;
    private Random random;

    private int nodeCount;
    private int electionTimeout;
    private bool stopRequested;
    private bool enableRandom;

    public int NodeId { get; }
    public bool IsLeader { get; private set; }
    public bool IsCandidate { get; private set; }
    public bool IsFollower { get; private set; }

    public RaftNode(int id, bool enableRandom, int nodeCount)
    {
        NodeId = id;
        random = new Random();
        IsLeader = false;
        IsCandidate = false;
        IsFollower = true;
        stopRequested = false;
        leaderId = -1;
        electionTimeout = enableRandom ? random.Next(150, 300) : 2000;
        this.enableRandom = enableRandom;
        this.nodeCount = nodeCount;
        nodes.Add(this);
    }

    public void Start()
    {
        thread = new Thread(Run);
        thread.Start();
    }

    public void Stop()
    {
        lock (lockObject)
        {
            stopRequested = true;
            IsLeader = false;
            IsFollower = false;
            IsCandidate = false;
            if(leaderId == NodeId)
                leaderId = -1;
        }
        thread.Join();
    }

    private void Run()
    {
        while (!stopRequested)
        {
            //lock (lockObject)
            //{
            //    Console.WriteLine($"Term: {currentTerm}, Node: {NodeId}, Leader: {IsLeader}, Candidate: {IsCandidate}, Follower: {IsFollower}");
            //}

            if (IsFollower)
            {
                Thread.Sleep(electionTimeout);
                lock (lockObject)
                {
                    if (leaderId == -1)
                        StartElection();
                }
            }
            else if (IsCandidate)
            {
                Thread.Sleep(electionTimeout);
                IsCandidate = false;
                IsFollower = true;
            }
            else if (IsLeader)
            {
                Thread.Sleep(enableRandom ? random.Next(500, 1000) : 1000);
            }
        }
    }

    private void StartElection()
    {
        if (stopRequested)
            return;

        int term;
        lock (lockObject)
        {
            currentTerm++;
            term = currentTerm;
            leaderId = -1;
            foreach(var node in nodes)
            {
                node.IsLeader = false;
                node.IsFollower = false;
            }
            IsCandidate = true;
            nodeVotes[(currentTerm, NodeId)] = 1;
        }
        RequestVotes(term);

    }

    private void RequestVotes(int term)
    {
        int votesNeeded = (nodeCount / 2) + 1;

        if (!IsCurrentTerm(term))
            return;
               
        foreach (var node in nodes)
        {

            if (node == this)
                continue;

            if (node.stopRequested)
                continue;

            RaftNode nodeToVoteFor = FindNodeToVoteFor(node, term);

            if(nodeToVoteFor != null)
            {
                lock (lockObject)
                {
                    nodeVotes.AddOrUpdate((term, nodeToVoteFor.NodeId), 1, (_, votes) => votes + 1);
                    votedFor.TryAdd((term, node.NodeId), nodeToVoteFor.NodeId);
                    int votes = nodeVotes[(term, nodeToVoteFor.NodeId)];
                    Console.WriteLine($"Term: {term}, Node: {node.NodeId}, Voted For: {nodeToVoteFor.NodeId}, Votes: {votes}");
                }
            }
        }

        Dictionary<int, int> totalVotesForTerm = new();
        lock (lockObject)
        {
            for (int i = 0; i < nodeCount; i++)
            {
                if (nodeVotes.ContainsKey((term, i)))
                {
                    totalVotesForTerm[i] = nodeVotes[(term, i)];
                }
                else
                {
                    totalVotesForTerm[i] = 0;
                }
            }

            var maxVoteNode = nodeVotes.Where(kv => kv.Key.Item1 == term)
                                       .OrderByDescending(kv => kv.Value)
                                       .FirstOrDefault();

            if (maxVoteNode.Value >= votesNeeded)
            {
                var keysToRemove = nodeVotes.Keys.Where(kv => kv.Item1 == term && kv.Item2 != maxVoteNode.Key.Item2).ToList();
                foreach (var key in keysToRemove)
                {
                    nodeVotes.TryRemove(key, out _);
                }

                RaftNode node = nodes.FirstOrDefault(x => x.NodeId == maxVoteNode.Key.Item2);
                if (node != null && !node.IsLeader)
                {
                    lock (lockObject)
                    {
                        leaderId = node.NodeId;
                        IsLeader = true;
                        IsCandidate = false;
                        IsFollower = false;
                    }
                }
            }
            else
            {
                lock (lockObject)
                {
                    IsLeader = false;
                    IsCandidate = false;
                    IsFollower = true;
                }
            }
        }
        Console.WriteLine($"Term: {term}, Node: {NodeId}, Leader: {IsLeader}, Candidate: {IsCandidate}, Follower: {IsFollower}");
    }

    private RaftNode FindNodeToVoteFor(RaftNode currentNode, int term)
    {
        var eligibleNodes = nodes.Where(x => x != currentNode && x.IsCurrentTerm(term) && x.IsCandidate && !x.stopRequested && !votedFor.ContainsKey((term, x.NodeId)));
        return eligibleNodes.OrderBy(_ => Guid.NewGuid()).FirstOrDefault();
    }

    private bool IsCurrentTerm(int term)
    {
        lock (lockObject)
        {
            return term == currentTerm;
        }
    }
}
