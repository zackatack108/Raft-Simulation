using Microsoft.AspNetCore.Mvc;

namespace Node_API.Controllers;

[ApiController]
[Route("[controller]")]
public class RaftController : Controller
{

    [HttpGet("ping")]
    public string Ping()
    {
        return "Pong";
    }

}
