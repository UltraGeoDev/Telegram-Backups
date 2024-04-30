using TL;
using TelegramData;
using ParsingTools;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;

namespace Backups {

    class CreateBackup (WTelegram.Client client, ILogger logger, int limit = 100) 
    {

        public WTelegram.Client client = client;
        public int limit = limit;
        public ILogger logger = logger;

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

            var messages = await client.Messages_GetHistory(peer, limit: limit);
            var parsing = new ParseMessage(messages, client);

            List<Dictionary<string, string?>> result = [];

            foreach (var msgBase in messages.Messages) {
                Dictionary<string, string?> parsed_msg = await parsing.Parse(msgBase);
                
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

                logger.LogInformation($"Created backup for {chat.name}. Total messages: {messages.Count}");
            }
        }
    }
}