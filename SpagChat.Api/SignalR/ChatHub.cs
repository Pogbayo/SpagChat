using Microsoft.AspNetCore.SignalR;
using SpagChat.Application.DTO.ChatRooms;
using SpagChat.Application.Interfaces.IServices;

namespace SpagChat.API.SignalR
{
    public class ChatHub : Hub
    {
        public static Dictionary<string, string> OnlineUsers = new Dictionary<string, string>();
        private readonly IChatRoomService _chatRoomService;

        public ChatHub(IChatRoomService chatRoomService)
        {
            _chatRoomService = chatRoomService;
        }
        public override async Task OnConnectedAsync()
        {
            var userIdString = Context.GetHttpContext()?.Request?.Query["userId"].ToString();
            var connectionId = Context.ConnectionId;

            if (string.IsNullOrEmpty(userIdString))
            {
                throw new ArgumentNullException("Context returned a null Id");
            }

            OnlineUsers[userIdString] = connectionId;

            Console.WriteLine($"{userIdString} connected with connectionId: {connectionId}");

            var userId = Guid.Parse(userIdString);
            var result = await _chatRoomService.GetChatRoomIdsForUserAsync(userId);

            if (result.Success && result.Data != null)
            {
                foreach (var chatRoomId in result.Data)
                {
                    await Groups.AddToGroupAsync(connectionId, chatRoomId.ToString());
                }
            }
            else
            {
                Console.WriteLine($"Failed to get chat rooms for user {userId}: {result.Message}");
            }

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

        public async Task NotifyUsersAdded(Guid chatRoomId, List<Guid> userIds)
        {
            await Clients.Group(chatRoomId.ToString()).SendAsync("UsersChanged", new
            {
                Event = "UsersAdded",
                UserIds = userIds
            });
        }

        public async Task NotifyUserRemoved(Guid chatRoomId, Guid userId)
        {
            await Clients.Group(chatRoomId.ToString()).SendAsync("UsersChanged", new
            {
                Event = "UserRemoved",
                UserId = userId
            });
        }
        public async Task SendChatRoomUpdated(string chatRoomId, string newName)
        {
            await Clients.All.SendAsync("ChatRoomUpdated", chatRoomId, newName);
        }

        public async Task SendChatRoomCreated(ChatRoomDto chatRoom)
        {
            await Clients.All.SendAsync("ChatRoomCreated", chatRoom);
        }

        public async Task SendChatRoomDeleted(string chatRoomId)
        {
            await Clients.All.SendAsync("ChatRoomDeleted", chatRoomId);
        }

    }
}
