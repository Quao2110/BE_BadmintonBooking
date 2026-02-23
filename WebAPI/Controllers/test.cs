using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        [HttpPost("test")]
        public IActionResult Index()
        {
            return Ok(new
            {
                status = "success",
                message = "Kết nối API test thành công!"
            });
        }
    }
}