using System;
using System.Threading;

public class RaftNode
{
    private int nodeId;
    private Thread thread;
    private Random random;
    private int leaderId;
    private bool isLeader;
    private bool isCandidate;
    private bool isFollower;
    private int currentTerm;
    private int votesReceived;
    private int electionTimeout;
    private bool stopRequested;

    public RaftNode(int id)
    {
        nodeId = id;
        random = new Random();
        isLeader = false;
        isCandidate = false;
        isFollower = true;
        currentTerm = 0;
        votesReceived = 0;
        electionTimeout = random.Next(150, 300);
        stopRequested = false;
        leaderId = -1;
    }

    public void Start()
    {
        thread = new Thread(Run);
        thread.Start();
    }

    public void Stop()
    {
        stopRequested = true;
        thread.Join();
    }

    private void Run()
    {
        while (!stopRequested)
        {
            if (isFollower)
            {
                Thread.Sleep(electionTimeout);
                if (leaderId == -1)
                {
                    LogInfo($"Node {nodeId} became a Candidate.");
                    StartElection();
                }
            }
            else if (isCandidate)
            {
                Thread.Sleep(electionTimeout);
                LogInfo($"Node {nodeId} became a Follower.");
                isCandidate = false;
                isFollower = true;
            }
            else if (isLeader)
            {
                LogInfo($"Node {nodeId} is the Leader.");
                Thread.Sleep(random.Next(500, 1000));
            }
        }
    }

    private void StartElection()
    {
        isCandidate = true;
        isFollower = false;
        currentTerm++;
        votesReceived = 1;
        electionTimeout = random.Next(150, 300);
        RequestVotes();
    }

    private void RequestVotes()
    {
        for (int i = 0; i < 3; i++)
        {
            if (i != nodeId)
            {
                bool voteGranted = random.Next(0, 2) == 1;
                if (voteGranted && currentTerm == 1)
                {
                    votesReceived++;
                    LogInfo($"Node {nodeId} received a vote from Node {i}.");
                    if (votesReceived > 1)
                    {
                        leaderId = nodeId;
                        LogInfo($"Node {nodeId} became the Leader.");
                        isLeader = true;
                        isCandidate = false;
                        isFollower = false;
                        return;
                    }
                }
            }
        }
        LogInfo($"Node {nodeId} failed to become the Leader.");
        isLeader = false;
        isCandidate = false;
        isFollower = true;
    }

    private void LogInfo(string message)
    {
        string logFileName = $"Node_{nodeId}_Log.txt";
        using (StreamWriter writer = File.AppendText(logFileName))
        {
            writer.WriteLine(message);
        }
    }
}

class Program
{
    static void Main(string[] args)
    {
        RaftNode[] nodes = new RaftNode[3];

        for (int i = 0; i < 3; i++)
        {
            nodes[i] = new RaftNode(i);
            nodes[i].Start();
        }

        Thread.Sleep(5000);

        for (int i = 0; i < 3; i++)
        {
            nodes[i].Stop();
        }

        Console.WriteLine("Simulation ended.");
    }
}
