using Microsoft.AspNetCore.Mvc;
using steamrev_backend.Server;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace steamrev_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MetricsController : ControllerBase
    {
        // GET: api/<MetricsController>
        [HttpGet]
        public async Task<Dictionary<string, object>> Get()
        {
            return null;
        }

        // GET api/<MetricsController>/5
        [HttpGet("{year}")]
        public async Task<Dictionary<string, object>> Get(int year)
        {
            Metrics metrics = new Metrics();
            var earnings = await metrics.GetAverageEarningsForYear(year);
            return earnings;
        }
    }
}
