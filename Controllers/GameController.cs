using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using Tictactoe_service.Hubs;

namespace Tictactoe_service.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GameController : ControllerBase
    {
        private readonly IHubContext<MessageHub> _hub;

        public GameController(IHubContext<MessageHub> hub)
        {
            _hub = hub;
        }

        [HttpGet("{message}")] 
        public async Task SendMessage(string message)
        {
            await _hub.Clients.All.SendAsync("LobbyMessage", message);
        }
    }
}
