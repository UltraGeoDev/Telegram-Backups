using TL;
using WTelegram;
using TelegramData;

using System.Text.Json;

namespace Backups {
    class CreateBackup (Client client) {
        public Client client = client;

        public static Chats Get_chats() {
            string filename = "data/chats.json";
            string jsonString = File.ReadAllText(filename);
            Chats chats = JsonSerializer.Deserialize<Chats>(jsonString)!;
            return chats; 
        }

        public async Task Get_messages(TelegramData.Chat chat) {
            // Get messages
            InputPeerChat peer = new(chat.id);
            
            var messagesBase = await client.Messages_GetHistory(peer, 0, default, 0, 1000, 0, 0, 0);
            Console.WriteLine($"Got messgaes for {chat.name}");
        }
        
        public async Task Create() {
            // Login
            User user = await client.LoginUserIfNeeded();

            // Get chats to backup
            Chats chats = Get_chats();

            foreach (var chat in chats.chats) {
                // TODO: Get messages from chat
            }
        }
    }
}