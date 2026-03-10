using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class TestController : ControllerBase
{
    [HttpGet]
    public IActionResult GetTest()
    {
        return Ok(new { message = "API is working!", timestamp = DateTime.Now });
    }
}
