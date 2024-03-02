using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using steamrev_backend.Server;
using System.Text.Json;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace steamrev_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GamesController : ControllerBase
    {
        // GET: api/games
        [HttpGet]
        public async Task<List<Dictionary<string, object>>> Get()
        {
            Metrics metrics = new Metrics();
            List<Dictionary<string, object>> gameList = await metrics.GetAllGames();
            return gameList;
        }

        // GET api/games/5
        [HttpGet("{id}")]
        public async Task<ContentResult> Get(int id)
        {
            Metrics metrics = new Metrics();
            Dictionary<string, object> gameDetails = await metrics.GetGameDetailsUsingID(id);
            string jsonString = JsonConvert.SerializeObject(gameDetails["details"]);

            return new ContentResult()
            {
                Content = jsonString,
                ContentType = "application/json"
            };
        }
        [Route("api/[controller]/search/{name}"), HttpGet]
        public async Task<List<Dictionary<string, object>>> Get(string name)
        {
            Metrics metrics = new Metrics();
            List<Dictionary<string, object>> gameList = await metrics.GetAllGames();
            return gameList;
        }
    }
}
