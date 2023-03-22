using System;
using System.Linq;

using Newtonsoft.Json.Linq;

using Eirshy.PB.PressXToJson.Entities;
using Eirshy.PB.PressXToJson.Enums;
using Eirshy.PB.PressXToJson.Exceptions;

namespace Eirshy.PB.PressXToJson.InstructionProcessing {
    internal class PathedInstructionProcessor {
        /// <summary>
        /// Throws in the event the instruction fails.
        /// </summary>
        public JObject Apply(Instruction ins, JObject target) {
            //Bail if we have no Target - we can't apply any pathed commands without one.
            if(target == null) return target;

            //JPath-ful commands
            var tokens = target.SelectTokens(ins.Target).ToList();
            foreach(var tok in tokens) {
                var toka = tok as JArray;
                var toko = tok as JObject;
                switch(ins.Command) {
                    //Target-Typed .. ... ... ... ... ... ... ... ...
                    #region Replace . ... ... ... ... ... ... ... ...

                    case Command.Replace:
                        if(tok.Type != JTokenType.Null) {
                            CommandTargetTypeException.ThrowIfMismatch(ins.Command, ins.Data.Type, tok.Type);
                        }
                        tok.Replace(ins.Data);
                        break;

                    #endregion

                    //Any-Typed . ... ... ... ... ... ... ... ... ...
                    #region Remove .. ... ... ... ... ... ... ... ...

                    case Command.Remove:
                        switch(tok.Type) {
                            case JTokenType.Array:
                            case JTokenType.Object:
                            case JTokenType.Property:
                                tok.Remove();
                                break;
                            default:
                                tok.Parent.Remove();
                                break;
                        }
                        break;

                    #endregion

                    //Array-Typed ... ... ... ... ... ... ... ... ...
                    #region Concat .. ... ... ... ... ... ... ... ...

                    case Command.Concat:
                        switch(tok.Type) {
                            case JTokenType.Array:
                                foreach(var ele in ins.DataArr) toka.Add(ele);
                                break;
                            case JTokenType.Null:
                                tok.Replace(ins.Data);
                                break;
                            default:
                                throw new CommandTargetTypeException(ins.Command, JTokenType.Array, tok.Type);
                        }
                        break;

                    #endregion
                    #region SpreadAfter & SpreadBefore .. ... ... ...

                    case Command.SpreadAfter:
                        CommandTargetTypeException.ThrowIfMismatch(ins.Command, JTokenType.Array, tok.Parent.Parent.Type, true);
                        foreach(var ele in ins.DataArr.Reverse()) {
                            tok.Parent.AddAfterSelf(ele);
                        }
                        break;
                    case Command.SpreadBefore:
                        CommandTargetTypeException.ThrowIfMismatch(ins.Command, JTokenType.Array, tok.Parent.Parent.Type, true);
                        foreach(var ele in ins.DataArr) {
                            tok.Parent.AddBeforeSelf(ele);
                        }
                        break;

                    #endregion

                    //Object-Typed .. ... ... ... ... ... ... ... ...
                    #region Merge ... ... ... ... ... ... ... ... ...

                    case Command.Merge:
                        switch(tok.Type) {
                            case JTokenType.Object:
                                toko.Merge(ins.Data);//maybe?
                                break;
                            case JTokenType.Null:
                                tok.Replace(ins.Data);
                                break;
                            default:
                                throw new CommandTargetTypeException(ins.Command, JTokenType.Object, tok.Type);
                        }
                        break;

                        #endregion
                }
                if(ins.FirstMatchOnly) break;
            }
            return target;
        }
    }
}
