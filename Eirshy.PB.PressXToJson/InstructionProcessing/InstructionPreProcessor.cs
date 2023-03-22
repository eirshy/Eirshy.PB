using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Newtonsoft.Json.Linq;

using PhantomBrigade.Data;

using Eirshy.PB.PressXToJson.Entities;
using Eirshy.PB.PressXToJson.Enums;
using Eirshy.PB.PressXToJson.Exceptions;

namespace Eirshy.PB.PressXToJson.InstructionProcessing {
    internal class InstructionPreProcessor {
        readonly Regex _autoType = new Regex(@"^((?:\w+\.)*)(\w+)(?:$|[\s,]+([\w.-]*)$)");

        /// <summary>
        /// Applies any applicable validations and preprocessors to the passed instruction.
        /// <br />Marks the instruction as Disabled if an error occurs.
        /// <br />IS THREADSAFE!
        /// </summary>
        public void PreProcess(Instruction ins) {
            try {
                var dataType = ins.Data?.Type ?? JTokenType.None;
                //validations
                ValidateTypes(ins, dataType);

                //Instruction translators
                if(dataType == JTokenType.Array || dataType == JTokenType.Object) {
                    TranslateAutoType(ins);
                }
            } catch(Exception ex) {
                ins.AddError(ex);
            }
        }

        internal void ValidateTypes(Instruction ins, JTokenType dataType) { //dataType is coalesced already
            //Target Type checking
            if(ins.TargetType is null) {
                //currently just "required success"
                throw InstructionParseException.NoType;
            }

            //Token Types
            switch(ins.Command) {
                //Parse Error
                case Command.Unknown: throw new CommandException(ins.Command, "Command Could Not Be Interpreted");

                //Object commands
                case Command.New:
                case Command.Merge:
                    CommandDataTypeException.ThrowIfMismatch(ins.Command, dataType, JTokenType.Object);
                    break;

                //Array commands
                case Command.Concat:
                case Command.SpreadBefore:
                case Command.SpreadAfter:
                    CommandDataTypeException.ThrowIfMismatch(ins.Command, dataType, JTokenType.Array);
                    break;

                //Target-Typed commands
                case Command.Replace: break;

                //Data-Ignoring commands
                case Command.Remove:  break;

                //String commands
                case Command.PriorityCopy:
                case Command.Copy:
                    CommandDataTypeException.ThrowIfMismatch(ins.Command, dataType, JTokenType.String);
                    CommandTargetTypeException.ThrowIfAssignalbe(ins.Command, typeof(DataContainerUnique), ins.TargetType);
                    break;

                default: throw new NotImplementedException($"Command type {ins.Command} missing {nameof(ValidateTypes)} entry.");
            }
        }

        /// <summary>
        /// Converts from @TYPE syntax (with ins's ambient namespace/assembly) to $type syntax.
        /// <br />Preserves @TYPE properties.
        /// </summary>
        internal void TranslateAutoType(Instruction ins) {
            var sel = ins.Data.SelectTokens("..['@TYPE']").ToList();
            foreach(var x in sel) {
                if(x.Type != JTokenType.String) throw new AutoTypingException(x.Type);

                var value = x.Value<string>()?.Trim();
                if(string.IsNullOrEmpty(value)) continue;//silent skip, it's ignored.

                var processed = _autoType.Match(value);
                if(!processed.Success) throw new AutoTypingException(value);

                var ns = processed.Groups[1].Value == "" ? ins.CompositeNamespace : processed.Groups[1].Value;
                var cls = processed.Groups[2].Value;
                var asm = processed.Groups[3].Value == "" ? ins.CompositeAssembly : processed.Groups[3].Value;
                x.Parent.Parent.AddFirst(new JProperty("$type", $"{ns}{cls}, {asm}"));
            }
        }
    }
}
