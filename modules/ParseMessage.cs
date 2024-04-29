using TL;

namespace ParsingTools {
    class ParseMessage (Messages_MessagesBase messages) {
        public Messages_MessagesBase messages = messages;
        
        public Dictionary<string, string?> Parse(MessageBase msgBase) {
            if (msgBase is Message msg) {
                return ParseDefault(msg);
            }
            else if (msgBase is MessageService ms) {
                return ParseService(ms);
            }
            return new Dictionary<string, string?>();

        }

        public Dictionary<string, string?> ParseDefault(Message msg) {
            var from_user = messages.UserOrChat(msg.From ?? msg.Peer);

            var result = new Dictionary<string, string?> () {
                {"from", from_user.ToString()},
                {"date", msg.Date.ToString()},
                {"text", msg.message},
                {"media", msg.media.ToString()}
            };

            return result;
        }

        public Dictionary<string, string?> ParseService(MessageService msg) {
            var from_user = messages.UserOrChat(msg.From ?? msg.Peer);

            var result = new Dictionary<string, string?> () 
            {
                {"from", from_user.ToString()},
                {"date", msg.Date.ToString()},
                {"data", msg.action.GetType().Name[13..]}
            };

            return result;
        }
    }
}