using FluentAssertions;
using Raft_Simulation;

namespace RaftTests;

[TestFixture]
public class Tests
{

    //[Test]
    //public void LeaderElectedWhenTwoOutOfThreeNodesAreHealthy()
    //{
    //    int nodeCount = 3;
    //    RaftNode[] nodes = new RaftNode[nodeCount];

    //    for (int i = 0; i < nodeCount; i++)
    //    {
    //        nodes[i] = new RaftNode(i, false, nodeCount);
    //        nodes[i].Start();
    //    }

    //    Thread.Sleep(5000);

    //    int leaderCount = 0;
    //    for (int i = 0; i < nodeCount; i++)
    //    {
    //        if (nodes[i].IsLeader)
    //        {
    //            leaderCount++;
    //        }
    //        nodes[i].Stop();
    //    }

    //    leaderCount.Should().Be(1);        
    //}

    //[Test]
    //public void LeaderElectedWhenThreeOutOfFiveNodesAreHealthy()
    //{
    //    int nodeCount = 5;
    //    RaftNode[] nodes = new RaftNode[nodeCount];

    //    for (int i = 0; i < nodeCount; i++)
    //    {
    //        nodes[i] = new RaftNode(i, false, nodeCount);
    //        nodes[i].Start();
    //    }

    //    Thread.Sleep(5000);

    //    int leaderCount = 0;
    //    for (int i = 0; i < nodeCount; i++)
    //    {
    //        if (nodes[i].IsLeader)
    //        {
    //            leaderCount++;
    //        }
    //        nodes[i].Stop();
    //    }

    //    leaderCount.Should().Be(1);
    //}

    //[Test]
    //public void LeaderNotElectedIfThreeOutOfFiveNodesAreUnhealthy()
    //{
    //    int nodeCount = 5;
    //    RaftNode[] nodes = new RaftNode[nodeCount];

    //    for (int i = 0; i < nodeCount; i++)
    //    {
    //        nodes[i] = new RaftNode(i, false, nodeCount);
    //        nodes[i].Start();
    //    }

    //    nodes[0].Stop();
    //    nodes[1].Stop();
    //    nodes[2].Stop();

    //    Thread.Sleep(5000);

    //    int leaderCount = 0;
    //    for (int i = 0; i < nodeCount; i++)
    //    {
    //        if (nodes[i].IsLeader)
    //        {
    //            leaderCount++;
    //        }
    //        nodes[i].Stop();
    //    }

    //    leaderCount.Should().Be(0);
    //}

    //[Test]
    //public void NodeContinuesAsLeaderWhenAllNodesAreHealthy()
    //{
    //    int nodeCount = 5;
    //    RaftNode[] nodes = new RaftNode[nodeCount];

    //    for (int i = 0; i < nodeCount; i++)
    //    {
    //        nodes[i] = new RaftNode(i, false, nodeCount);
    //        nodes[i].Start();
    //    }

    //    Thread.Sleep(10000);

    //    int leaderCount = 0;
    //    for (int i = 0; i < nodeCount; i++)
    //    {
    //        if (nodes[i].IsLeader)
    //        {
    //            leaderCount++;
    //        }
    //        nodes[i].Stop();
    //    }

    //    leaderCount.Should().Be(1);
    //}

    //[Test]
    //public void NodeWillCallAnElectionIfMessagesFromLeaderTakeTooLong()
    //{
    //    int nodeCount = 5;
    //    RaftNode[] nodes = new RaftNode[nodeCount];

    //    for (int i = 0; i < nodeCount; i++)
    //    {
    //        nodes[i] = new RaftNode(i, false, nodeCount);
    //        nodes[i].Start();
    //    }

    //    Thread.Sleep(5000);

    //    int stoppedNode = 0;

    //    for (int i = 0; i < nodeCount; i++)
    //    {
    //        if (nodes[i].IsLeader)
    //        {
    //            nodes[i].Stop();
    //            stoppedNode = i;
    //        }
    //    }
    //    nodes[stoppedNode].Start();
    //    Thread.Sleep(5000);

    //    int leaderCount = 0;
    //    for (int i = 0; i < nodeCount; i++)
    //    {
    //        if (nodes[i].IsLeader)
    //        {
    //            leaderCount++;
    //        }
    //        nodes[i].Stop();
    //    }

    //    leaderCount.Should().Be(1);
    //}

    //[Test]
    //public void NodeContinuesAsLeaderIfTwoOutOfFiveNodesBecomeUnhealthy()
    //{
    //    int nodeCount = 5;
    //    RaftNode[] nodes = new RaftNode[nodeCount];

    //    for (int i = 0; i < nodeCount; i++)
    //    {
    //        nodes[i] = new RaftNode(i, false, nodeCount);
    //        nodes[i].Start();
    //    }

    //    Thread.Sleep(5000);

    //    int leaderCount = 0;
    //    for (int i = 0; i < nodeCount; i++)
    //    {
    //        if (nodes[i].IsLeader)
    //        {
    //            leaderCount++;
    //        }
    //    }

    //    leaderCount.Should().Be(1);

    //    nodes[0].Stop();
    //    nodes[1].Stop();

    //    Thread.Sleep(5000);

    //    leaderCount = 0;
    //    for (int i = 0; i < nodeCount; i++)
    //    {
    //        if (nodes[i].IsLeader)
    //        {
    //            leaderCount++;
    //        }
    //        nodes[i].Stop();
    //    }

    //    leaderCount.Should().Be(1);
    //}

    //[Test]
    //public void NodesWontDoubleVote()
    //{
    //    int nodeCount = 5;
    //    RaftNode[] nodes = new RaftNode[nodeCount];

    //    for (int i = 0; i < nodeCount; i++)
    //    {
    //        nodes[i] = new RaftNode(i, false, nodeCount);
    //        nodes[i].Start();
    //    }

    //    Thread.Sleep(10000);

    //    int leaderCount = 0;
    //    for (int i = 0; i < nodeCount; i++)
    //    {
    //        if (nodes[i].IsLeader)
    //        {
    //            leaderCount++;
    //        }
    //        nodes[i].Stop();
    //    }

    //    leaderCount.Should().Be(1);
    //}
}