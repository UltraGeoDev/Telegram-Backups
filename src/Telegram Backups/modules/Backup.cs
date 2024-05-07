using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using ParsingTools;
using TelegramData;
using TL;
using TL.Methods;
using YaDiskTools;

namespace Backups
{
    class CreateBackup(WTelegram.Client client, ILogger logger, string? client_id, int limit = 100)
    {
        public WTelegram.Client client = client;
        public int limit = limit;
        public ILogger logger = logger;
        public YaDisk yaDisk = new(client_id);

        public static Chats Get_chats()
        {
            // Get chats
            string filename = "data/chats.json";
            string jsonString = File.ReadAllText(filename);

            // Parse chats
            Chats chats = System.Text.Json.JsonSerializer.Deserialize<Chats>(jsonString)!;

            return chats;
        }

        public async Task<List<Dictionary<string, string?>>> Get_messages(
            TelegramData.Chat required_chat
        )
        {
            User user = new();
            InputPeerUser peer = new(required_chat.id, user.access_hash);

            // Parsed messages
            List<Dictionary<string, string?>> result = [];

            // Init takeout
            Account_Takeout takeout;

            try
            {
                // Takeout session
                takeout = await client.Account_InitTakeoutSession(
                    message_chats: true,
                    message_channels: true,
                    message_users: true
                );
            }
            catch (RpcException)
            {
                // wait for access
                Console.WriteLine(
                    "---WARNING---\nplease allow access to messages in the telegram\nbackup will start in the next 30 seconds\n-------------"
                );
                Thread.Sleep(30000);

                // Takeout session
                takeout = await client.Account_InitTakeoutSession(
                    message_chats: true,
                    message_channels: true,
                    message_users: true
                );
            }

            var finishTakeout = new Account_FinishTakeoutSession();

            // Get messages
            var messages = await client.InvokeWithTakeout(
                takeout.id,
                new Messages_GetHistory() { peer = peer, limit = limit }
            );

            // Pasing tools
            var parsing = new ParseMessage(messages, client);

            // Parse messages
            try
            {
                foreach (var msgBase in messages.Messages)
                {
                    var parsed_msg = await parsing.Parse(msgBase); // Parse message
                    result.Insert(0, parsed_msg); // Insert at the beginning

                    // Upload media to yandex disk
                    if (parsed_msg.TryGetValue("media", out string? value))
                    {
                        string? media_path = value;

                        try
                        {
                            string? uploaded_path = await yaDisk.UploadBackupFile(
                                media_path,
                                "/backups/media/"
                            );

                            if (uploaded_path != null)
                                logger.LogInformation($"Uploaded {uploaded_path}");
                        }
                        catch (Exception e)
                        {
                            logger.LogError(e, $"Failed to upload {media_path} due to {e.Message}");
                        }
                    }
                }
                finishTakeout.flags = Account_FinishTakeoutSession.Flags.success;
            }
            finally
            {
                await client.InvokeWithTakeout(takeout.id, finishTakeout); // Finish takeout
            }

            return result;
        }

        public async Task Create()
        {
            // Create folders
            Directory.CreateDirectory("data/backup");
            Directory.CreateDirectory("data/media");

            // Create backup folder in yandex disk
            await yaDisk.CreateFolder("/backups");
            await yaDisk.CreateFolder("/backups/media");

            // Login
            await client.LoginUserIfNeeded();

            // Get chats to backup
            Chats chats = Get_chats();

            foreach (var chat in chats.chats)
            {
                logger.LogInformation($"Starting backup for {chat.name}");

                // Get messages
                var messages = await Get_messages(chat);

                // Serialize to string
                string parsed_string = JsonConvert.SerializeObject(messages, Formatting.Indented);

                // Write to json file
                string path = $"data/backup/{chat.name}.json";

                File.WriteAllText(path, parsed_string);

                try
                {
                    await yaDisk.UploadBackupFile(path, "/backups/");
                }
                catch (Exception)
                {
                    logger.LogError($"Failed to upload json backup for {chat.name}");
                    continue;
                }

                logger.LogInformation(
                    $"Created backup for {chat.name}. Total messages: {messages.Count}"
                );
            }
        }
    }
}
