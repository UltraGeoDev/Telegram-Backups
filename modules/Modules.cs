namespace TelegramData
{
    class Message(string text, string type)
    {
        public readonly string text = text;
        public readonly string type = type;

    }

    class Chat
    {
        public required string name { get; set; }
        public required int id { get; set; }
    }

    class Chats
    {
        public required List<Chat> chats { get; set; }
        public required int total { get; set; }
    }
}