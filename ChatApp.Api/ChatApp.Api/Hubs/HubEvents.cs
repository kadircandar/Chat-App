namespace ChatApp.Api.Hubs
{
    public static class HubEvents
    {
        public const string FriendListUpdated = "FriendListUpdated";
        public const string OnlineUsers = "OnlineUsers";
        public const string PendingRequestsUpdated = "PendingRequestsUpdated";
        public const string ReceiveMessage = "ReceiveMessage";
        public const string SendMessage = "SendMessage";
        public const string UpdateOnlineFriends = "UpdateOnlineFriends";
        public const string UserConnected = "UserConnected";
        public const string UserDisconnected = "UserDisconnected";
    }

}
