using System;

using Newtonsoft.Json.Linq;

using Eirshy.PB.PressXToJson.Enums;

namespace Eirshy.PB.PressXToJson.Exceptions {
    internal class CommandDataTypeException : CommandException {
        private static string _write(JTokenType expected, JTokenType got) {
            return $"expected Data of type {expected}, got {got}";
        }
        public static void ThrowIfMismatch(Command cmd, JTokenType expected, JTokenType got) {
            if(expected != got) throw new CommandDataTypeException(cmd, expected, got);
        }
        public CommandDataTypeException(Command cmd, JTokenType expected, JTokenType got) : base(cmd, _write(expected, got)) { }
    }
}
