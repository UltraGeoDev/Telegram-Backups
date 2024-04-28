using TL;
using TelegramData;

using System.Text.Json;

namespace Backups {

    class CreateBackup (WTelegram.Client client) 
    {

        public WTelegram.Client client = client;

        public static Chats Get_chats() 
        {

            // Get chats
            string filename = "data/chats.json";
            string jsonString = File.ReadAllText(filename);

            // Parse chats
            Chats chats = JsonSerializer.Deserialize<Chats>(jsonString)!;
            
            return chats; 
        }

        public async Task Get_messages(TelegramData.Chat required_chat) 
        {
            User user = new();
            InputPeerUser peer = new(required_chat.id, user.access_hash);

            var messages = await client.Messages_GetHistory(peer);

            foreach (var msgBase in messages.Messages) {
                // TODO: Parse message
            }
        }
        
        public async Task Create() 
        {

            // Login
            User user = await client.LoginUserIfNeeded();

            // Get chats to backup
            Chats chats = Get_chats();

            foreach (var chat in chats.chats) 
            {
                await Get_messages(chat);

                // TODO: Save messages
            }
        }
    }
}