using Raft_Simulation;

class Program
{
    static void Main(string[] args)
    {
        int nodeCount = 3;
        RaftNode[] nodes = new RaftNode[nodeCount];

        for (int i = 0; i < nodeCount; i++)
        {
            nodes[i] = new RaftNode(i, true, nodeCount);
            nodes[i].Start();
        }

        Thread.Sleep(5000);

        for (int i = 0; i < nodeCount; i++)
        {
            nodes[i].Stop();
        }

        Console.WriteLine("Simulation ended.");
    }
}
