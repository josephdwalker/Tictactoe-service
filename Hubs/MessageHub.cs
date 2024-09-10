using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tictactoe_service.Hubs
{
    public class MessageHub : Hub
    {
        public static Dictionary<string, string> users = new Dictionary<string, string>();
        public static Dictionary<string, string> availableGames = new Dictionary<string, string>();
        public static Dictionary<string, List<string>> currentGames = new Dictionary<string, List<string>>();

        public async override Task OnConnectedAsync()
        {
            await Clients.Caller.SendAsync("AvailableGames", availableGames.Keys);
            await base.OnConnectedAsync();
        }

        public async override Task OnDisconnectedAsync(Exception exception)
        {
            var username = users[Context.ConnectionId];
            await Clients.All.SendAsync("LobbyChatMessage", username + " has left the lobby");
            users.Remove(Context.ConnectionId);
            var group = currentGames.FirstOrDefault(x => x.Value.Contains(username)).Key;
            if (group != null)
            {
                await LeaveGame(group, username);
            }
            var removeKey = availableGames.FirstOrDefault(x => x.Value == Context.ConnectionId).Key;
            if (removeKey != null) { 
                availableGames.Remove(removeKey);
            }
            await Clients.All.SendAsync("AvailableGames", availableGames.Keys);
            await base.OnDisconnectedAsync(exception);
        }

        public async Task Register(string username)
        {
            if (!users.Values.Contains(username))
            {
                users.Add(Context.ConnectionId, username);
                await Clients.All.SendAsync("LobbyChatMessage", username + " has joined the lobby!");
            }
        }

        public async Task SendChatMessage(string username, string message)
        {
            await Clients.All.SendAsync("LobbyChatMessage", username + ": " + message);
        }

        public async Task CreateGame(string username)
        {
            availableGames.Add(username, Context.ConnectionId);
            currentGames.Add(username, new List<string> { username });
            await Clients.All.SendAsync("AvailableGames", availableGames.Keys);
            await Groups.AddToGroupAsync(Context.ConnectionId, username);
        }

        public async Task JoinGame(string opponentUsername, string username)
        {
            availableGames.Remove(opponentUsername);
            currentGames[opponentUsername].Add(username);
            await Clients.All.SendAsync("AvailableGames", availableGames.Keys);
            await Groups.AddToGroupAsync(Context.ConnectionId, opponentUsername);
            await Clients.Group(opponentUsername).SendAsync("GameChatMessage", username + " has joined the game!");
        }

        public async Task LeaveGame(string group, string username)
        {
            await Clients.Group(group).SendAsync("GameChatMessage", username + " has left the game");
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, group);
            currentGames[group].Remove(username);
            if (currentGames[group].Count == 0) 
            {
                currentGames.Remove(group);
            }
        }

        public async Task SendGameMove(string group, string username, int move)
        {
            await Clients.Group(group).SendAsync("GameMove", username, move);
        }

        public async Task SendGameChatMessage(string group, string username, string message)
        {
            await Clients.Group(group).SendAsync("GameChatMessage", username + ": " + message);
        }
    }
}
