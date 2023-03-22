using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json.Linq;

using Eirshy.PB.PressXToJson.Enums;

namespace Eirshy.PB.PressXToJson.Exceptions {
    internal class CommandTargetTypeException : CommandException {
        public CommandTargetTypeException(Command cmd, string message) : base(cmd, message) { }

        private static string _write(Type disallow) => $"Command not allowed for entities of type {disallow.Name}";
        public static void ThrowIfAssignalbe(Command cmd, Type disallow, Type got) {
            if(disallow.IsAssignableFrom(got)) throw new CommandTargetTypeException(cmd, disallow);
        }
        public CommandTargetTypeException(Command cmd, Type disallow) : base(cmd, _write(disallow)) { }


        public static void ThrowIfMismatch(Command cmd, JTokenType expected, JTokenType got, bool isParent = false) {
            if(expected != got) throw new CommandTargetTypeException(cmd, expected, got, isParent);
        }
        private static string _write(JTokenType expected, JTokenType got, bool isParent) {
            if(isParent) {
                return $"Expected target parent of type {expected}, got {got}";
            } else return $"Expected target of type {expected}, got {got}";
        }
        public CommandTargetTypeException(Command cmd, JTokenType expected, JTokenType got, bool isParent = false) 
            : base(cmd, _write(expected, got, isParent)) 
        { }
    }
}
