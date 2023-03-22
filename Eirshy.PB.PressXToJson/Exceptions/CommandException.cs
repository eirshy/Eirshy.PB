using System;

using Eirshy.PB.PressXToJson.Enums;

namespace Eirshy.PB.PressXToJson.Exceptions {
    internal class CommandException : ArgumentException {
        private static string _write(Command cmd, string msg) => $"{cmd} -- {msg}";
        public CommandException(Command cmd, string message) : base(_write(cmd, message)) { }

    }
}
