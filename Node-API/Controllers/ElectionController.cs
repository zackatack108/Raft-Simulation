using Microsoft.AspNetCore.Mvc;
using Node_API.Util;

namespace Node_API.Controllers;

[ApiController]
[Route("[controller]")]
public class ElectionController : Controller
{

    private Node node;
    private readonly LogHandler logHandler;
    private Random random;

    public ElectionController(Node node, LogHandler logHandler)
    {
        this.node = node;
        this.logHandler = logHandler;
        random = new Random();
    }

    [HttpGet("VoteFor")]
    public bool VoteFor(string canadidte, int term)
    {
        bool vote = false;
        (string, object, int) logMessage = ($"Node: {node.ThisNode}, Didn't vote for: {canadidte}, Term: {term}", false, 1);
        if(term > node.CurrentTerm)
        {
            node.CurrentTerm = term;
            (string, object, int) voteMessage = ($"Node: {node.ThisNode}, Voted for node: {canadidte}, Term: {term}", true, 1);

            if(!logHandler.LogExists(voteMessage, FileType.ELECTION) && !logHandler.LogExists(logMessage, FileType.ELECTION))
            {
                int randomNumber = random.Next(10);

                if (randomNumber % 2 == 0)
                {
                    vote = true;
                    logMessage = voteMessage;
                }
            }
        }

        logHandler.WriteLog(logMessage, FileType.ELECTION);
        return vote;
    }
}
