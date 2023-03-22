using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Eirshy.PB.PressXToJson.Entities;

namespace Eirshy.PB.PressXToJson.Exceptions {
    internal class InstructionParseException : InvalidOperationException {
        private InstructionParseException(string message) : base(message) { }

        public static InstructionParseException NoType
            => new InstructionParseException($"Instruction has no data type.");
        
    }
}
