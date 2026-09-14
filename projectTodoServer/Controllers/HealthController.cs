using Microsoft.AspNetCore.Mvc;

namespace projectTodoServer.Controllers;

[ApiController]
[Route("api/health")]
public class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult GetHealth()
    {
        return Ok(new
        {
            serverStatus = "ok",
            dbStatus = "ok",
            serverTime = DateTimeOffset.UtcNow
        });
    }
}