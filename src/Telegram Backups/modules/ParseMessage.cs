using TL;
using WTelegram;

namespace ParsingTools
{
    class ParseMessage(Messages_MessagesBase messages, Client client)
    {
        public Messages_MessagesBase messages = messages;
        public Client client = client;

        // Main parse function
        public async Task<Dictionary<string, string?>> Parse(MessageBase msgBase)
        {
            if (msgBase is Message msg)
            {
                Dictionary<string, string?> result = await ParseDefault(msg);
                return result;
            }
            else if (msgBase is MessageService ms)
            {
                return ParseService(ms);
            }
            return [];
        }

        // Parse default messages
        public async Task<Dictionary<string, string?>> ParseDefault(Message msg)
        {
            var from_user = messages.UserOrChat(msg.From ?? msg.Peer);
            string? media_path = await ParseMedia(msg);

            var parsed = new Dictionary<string, string?>
            {
                { "from", from_user.ToString() },
                { "date", msg.Date.ToString() },
                { "text", msg.message == "" ? null : msg.message },
                { "media", media_path }
            };

            return parsed;
        }

        // Parse service messages
        public Dictionary<string, string?> ParseService(MessageService msg)
        {
            var from_user = messages.UserOrChat(msg.From ?? msg.Peer);

            var result = new Dictionary<string, string?>()
            {
                { "from", from_user.ToString() },
                { "date", msg.Date.ToString() },
                { "data", msg.action.GetType().Name[13..] }
            };

            return result;
        }

        // Parse and download photos and documents
        public async Task<string?> ParseMedia(Message msg)
        {
            // Base path to store files
            string base_path = "data/media/" + Guid.NewGuid().ToString() + ".";

            if (msg.media is MessageMediaDocument { document: Document document })
            {
                // Create file stream and filepath
                string ext = document.mime_type[(document.mime_type.IndexOf('/') + 1)..];
                using var fileStream = File.Create(base_path + ext);

                // Download document
                await client.DownloadFileAsync(document, fileStream);

                fileStream.Close();
                return base_path + ext;
            }
            else if (msg.media is MessageMediaPhoto { photo: Photo photo })
            {
                // Create file stream and filepath
                string ext = "jpg";
                using var fileStream = File.Create(base_path + ext);

                // Download photo
                var type = await client.DownloadFileAsync(photo, fileStream);

                fileStream.Close();

                // Rename file if needed
                if (type is not Storage_FileType.unknown and not Storage_FileType.partial)
                    File.Move(base_path + ext, base_path + type, true);

                return base_path + type;
            }

            return null;
        }
    }
}
