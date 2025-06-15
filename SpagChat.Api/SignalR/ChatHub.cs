using Microsoft.AspNetCore.SignalR;

namespace SpagChat.API.SignalR
{
    public class ChatHub : Hub
    {
        public static Dictionary<string, string> OnlineUsers = new Dictionary<string, string>();
        public override async Task OnConnectedAsync()
        {
            var userId = Context.GetHttpContext()?.Request?.Query["userId"].ToString(); 
            var connectionId = Context.ConnectionId;

            if (string.IsNullOrEmpty(userId))
            {
                throw new ArgumentNullException("Context returned a null Id");
               
            }
        
            OnlineUsers[userId] = connectionId;

            Console.WriteLine($"{userId} connected with connectionId: {connectionId}");

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
           
            var user = OnlineUsers.FirstOrDefault(x => x.Value == Context.ConnectionId);
            if (!string.IsNullOrEmpty(user.Key))
            {
                OnlineUsers.Remove(user.Key);
                Console.WriteLine($"{user.Key} disconnected.");
            }

            await base.OnDisconnectedAsync(exception);
        }

        public async Task JoinRoom(string chatRoomId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, chatRoomId);
        }

        public async Task LeaveRoom(string chatRoomId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, chatRoomId);
        }

    }
}
