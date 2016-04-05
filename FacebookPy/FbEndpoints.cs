namespace FacebookPy
{
    public static class FbEndpoints
    {
        public const string SendURL = @"https://www.facebook.com/ajax/mercury/send_messages.php";
        public const string SearchURL = "https://www.facebook.com/ajax/typeahead/search.php";
        public const string MobileURL = @"https://m.facebook.com/";
        public const string BaseURL = @"https://www.facebook.com";
        public const string LoginURL = @"https://m.facebook.com/login.php?login_attempt=1";
        public const string ThreadSyncURL = @"https://www.facebook.com/ajax/mercury/thread_sync.php";
        public const string ThreadsURL = @"https://www.facebook.com/ajax/mercury/threadlist_info.php";
        public const string StickyURL = @"https://0-edge-chat.facebook.com/pull";
        public const string PingURL = @"https://0-channel-proxy-06-ash2.facebook.com/active_ping";
        public const string DeliveredURL = @"https://www.facebook.com/ajax/mercury/delivery_receipts.php";
        public const string ReadStatusURL = @"https://www.facebook.com/ajax/mercury/change_read_status.php";

        public const string GetUsersStatusURL = @"https://www.facebook.com/ajax/chat/buddy_list.php";
        public const string GetUserInfo = @"https://www.facebook.com/chat/user_info/";
        public const string GetAllFriendsURL = @"https://www.facebook.com/chat/user_info_all";

        public const string ModernSettingsURL = @"https://www.facebook.com/bluebar/modern_settings_menu/?help_type=364455653583099&show_contextual_help=1";

        public const string LogOutURL = @"https://www.facebook.com/logout.php";
    }
}