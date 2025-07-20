using Microsoft.AspNetCore.SignalR;
using SpagChat.Application.DTO.ChatRooms;
using SpagChat.Application.Interfaces.IServices;

namespace SpagChat.API.SignalR
{
    public class ChatHub : Hub
    {
        public static Dictionary<string, string> OnlineUsers = new Dictionary<string, string>();
        private readonly IChatRoomService _chatRoomService;
        public static Dictionary<string, HashSet<string>> ChatRoomConnections = new();

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
                await BroadcastOnlineUsers();

                lock (ChatRoomConnections)
                {
                    foreach (var group in ChatRoomConnections)
                    {
                        group.Value.Remove(user.Key);
                    }
                }

                Console.WriteLine($"{user.Key} disconnected.");
            }

            await base.OnDisconnectedAsync(exception);
        }

        public async Task JoinRoom(string chatRoomId)
        {
            var userId = Context.GetHttpContext()?.Request?.Query["userId"].ToString();

            if (string.IsNullOrEmpty(userId)) return;

            await Groups.AddToGroupAsync(Context.ConnectionId, chatRoomId);

            lock (ChatRoomConnections)
            {
                if (!ChatRoomConnections.ContainsKey(chatRoomId))
                {
                    ChatRoomConnections[chatRoomId] = new HashSet<string>();
                }

                ChatRoomConnections[chatRoomId].Add(userId);
            }

            Console.WriteLine($"{userId} joined chat room {chatRoomId}");
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

        //public async Task SendChatRoomCreated(string connectionId, ChatRoomDto chatRoom)
        //{
        //    await Clients.Client(connectionId).SendAsync("ChatRoomCreated", chatRoom);
        //}

        public async Task SendChatRoomDeleted(string chatRoomId)
        {
            await Clients.All.SendAsync("ChatRoomDeleted", chatRoomId);
        }

        public async Task BroadcastOnlineUsers()
        {
            var onlineUserIds = OnlineUsers.Keys.ToList();
            await Clients.All.SendAsync("OnlineUsersChanged", onlineUserIds);
        }

        public void LogGroupListeners(string chatRoomId)
        {
            if (ChatRoomConnections.TryGetValue(chatRoomId, out var userList))
            {
                Console.WriteLine($"Users listening to {chatRoomId}: {string.Join(", ", userList)}");
            }
        }


    }
}
