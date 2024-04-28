using TL;

namespace ParsingTools {
    class ParseMessage {
        public string? Parse(MessageBase msgBase) {
            if (msgBase is Message msg) {
                return ParseDefault(msg);
            }
            else if (msgBase is MessageService ms) {
                return ParseService(ms);
            }
            return null;

        }

        public string ParseDefault(Message msg) {
            // TODO: Parse every type of message
            return "";
        }

        public string ParseService(MessageService msg) {
            // TODO: Implement service parsing
            return "";
        }
    }
}