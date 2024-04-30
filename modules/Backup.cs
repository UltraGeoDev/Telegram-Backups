using TL;
using TelegramData;
using ParsingTools;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;
using TL.Methods;

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

            // Parsed messages
            List<Dictionary<string, string?>> result = [];

            // Takeout session
            var takeout = await client.Account_InitTakeoutSession(
                message_chats: true,
                message_channels: true,
                message_users: true
            );

            var finishTakeout = new Account_FinishTakeoutSession();


            // Get messages
            var messages = await client.InvokeWithTakeout(
                takeout.id, 
                new Messages_GetHistory() {peer=peer, limit=limit}
            );

            // Pasing tools
            var parsing = new ParseMessage(messages, client);

            // Parse messages
            try {
                foreach (var msgBase in messages.Messages) {
                    var parsed_msg = await parsing.Parse(msgBase); // Parse message
                    result.Insert(0, parsed_msg); // Insert at the beginning
                }
                finishTakeout.flags = Account_FinishTakeoutSession.Flags.success;
            }
            finally {
                await client.InvokeWithTakeout(takeout.id, finishTakeout); // Finish takeout
            }

            return result;
        }
        
        public async Task Create() 
        {
            // Create folders
            Directory.CreateDirectory("data/backup");
            Directory.CreateDirectory("data/media");

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