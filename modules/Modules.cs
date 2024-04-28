namespace TelegramData
{

    class Chat
    {
        public required string name { get; set; }
        public required long id { get; set; }
    }

    class Chats
    {
        public required List<Chat> chats { get; set; }
        public required int total { get; set; }
    }
    
}