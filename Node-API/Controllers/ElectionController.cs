using Microsoft.AspNetCore.Mvc;
using Node_API.Util;

namespace Node_API.Controllers;

[ApiController]
[Route("[controller]")]
public class ElectionController : Controller
{

    private Node node;
    private LogHandler logHandler;
    private ILogger<ElectionController> logger;
    private Random random;

    public ElectionController(Node node, LogHandler logHandler, ILogger<ElectionController> logger)
    {
        this.node = node;
        this.logHandler = logHandler;
        this.logger = logger;
        random = new Random();
    }

    [HttpGet("VoteFor")]
    public bool VoteFor(string candidate, int term)
    {
        bool vote = false;
        string message = $"Node: {node.ThisNode()}, Didn't vote for: {candidate}, Term: {term}";
        (string, object, int) logMessage = (message, false, 1);
        if(term > node.CurrentTerm)
        {
            node.CurrentTerm = term;
            string voteMessage = $"Node: {node.ThisNode()}, Voted for node: {candidate}, Term: {term}";
            (string, object, int) voteLogMessage = (voteMessage, true, 1);

            if(!logHandler.LogExists(voteLogMessage, FileType.ELECTION) && !logHandler.LogExists(logMessage, FileType.ELECTION))
            {
                int randomNumber = random.Next(10);

                if (randomNumber % 2 == 0)
                {
                    vote = true;
                    message = voteMessage;
                    logMessage = voteLogMessage;
                }
            }
        }

        logger.LogInformation(message);
        logHandler.WriteLog(logMessage, FileType.ELECTION);
        return vote;
    }
}
