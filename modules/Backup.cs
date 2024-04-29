using TL;
using TelegramData;
using ParsingTools;
using Newtonsoft.Json;

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
            Chats chats = System.Text.Json.JsonSerializer.Deserialize<Chats>(jsonString)!;
            
            return chats; 
        }

        public async Task<List<Dictionary<string, string?>>> Get_messages(TelegramData.Chat required_chat) 
        {
            User user = new();
            InputPeerUser peer = new(required_chat.id, user.access_hash);

            var messages = await client.Messages_GetHistory(peer);
            var parsing = new ParseMessage(messages);

            List<Dictionary<string, string?>> result = new(); // TODO: <Dictionary<string, string?>>

            foreach (var msgBase in messages.Messages) {
                // TODO: Parse message
                Dictionary<string, string?> parsed_msg = parsing.Parse(msgBase);
                
                result.Insert(0, parsed_msg);
            }
            return result;
        }
        
        public async Task Create() 
        {

            // Login
            await client.LoginUserIfNeeded();

            // Get chats to backup
            Chats chats = Get_chats();

            foreach (var chat in chats.chats) 
            {
                // Get messages
                var messages = await Get_messages(chat);

                // Serialize to string
                string parsed_string = JsonConvert.SerializeObject(messages, Formatting.Indented);

                // Write to json file
                string path = $"data/backup/{chat.name}.json";
                File.WriteAllText(path, parsed_string);
            }
        }
    }
}