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
                { "media", media_path },
                { "type", GetMediaType(msg) }
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
            string base_path = "data/media/";

            if (msg.media is MessageMediaDocument { document: Document document })
            {
                // Create file stream and filepath
                string ext = document.mime_type[(document.mime_type.IndexOf('/') + 1)..];
                string file_id = document.id.ToString();

                using var fileStream = File.Create(base_path + $"{file_id}.{ext}");

                // Download document
                await client.DownloadFileAsync(document, fileStream);

                fileStream.Close();
                return base_path + $"{file_id}.{ext}";
            }
            else if (msg.media is MessageMediaPhoto { photo: Photo photo })
            {
                // Create file stream and filepath
                string ext = "jpg";
                string file_id = photo.id.ToString();

                using var fileStream = File.Create(base_path + $"{file_id}.{ext}");

                // Download photo
                var type = await client.DownloadFileAsync(photo, fileStream);

                fileStream.Close();

                // Rename file if needed
                if (type is not Storage_FileType.unknown and not Storage_FileType.partial)
                {
                    File.Move(
                        base_path + $"{file_id}.{ext}",
                        base_path + $"{file_id}.{type}",
                        true
                    );
                    return base_path + $"{file_id}.{type}";
                }
                return base_path + $"{file_id}.{ext}";
            }

            return null;
        }

        // Get media type
        public static string GetMediaType(Message msg)
        {
            string type;
            try
            {
                type = msg.media.GetType().Name; // Get media type
                type ??= "Text";
            }
            catch (NullReferenceException)
            {
                type = "Text"; // Get text type if media is null
            }
            return type;
        }
    }
}
