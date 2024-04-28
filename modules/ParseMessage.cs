using TL;

namespace ParsingTools {
    class ParseMessage {
        public string? Parse(MessageBase msgBase) {
            if (msgBase is Message msg) {
                // TODO: Parse every type of message
                return ParseDefault(msg);
            }
            else if (msgBase is MessageService ms) {
                // TODO: Implement service parsing
                return ParseService(ms);
            }
            return null;

        }

        public string ParseDefault(Message msg) {
            return "";
        }

        public string ParseService(MessageService msg) {
            return "";
        }
    }
}