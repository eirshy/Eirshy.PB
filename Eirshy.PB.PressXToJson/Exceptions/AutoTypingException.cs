using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json.Linq;

using Eirshy.PB.PressXToJson.Entities;

namespace Eirshy.PB.PressXToJson.Exceptions {
    internal class AutoTypingException : InvalidOperationException {

        private static string _write(JTokenType got) => $"@TYPE expect a string token, got {got}";
        public AutoTypingException(JTokenType got) : base(_write(got)) { }

        private static string _write(string got) => $"@TYPE parse error, reading: \"{got}\"";
        public AutoTypingException(string got) : base(_write(got)) { }
    }
}
