using System;
using System.Collections.Generic;
using System.Linq;

using Newtonsoft.Json.Linq;

using Eirshy.PB.PressXToJson.Entities;
using Eirshy.PB.PressXToJson.Enums;
using Eirshy.PB.PressXToJson.Exceptions;

namespace Eirshy.PB.PressXToJson.Processing {
    internal class PathedCommandProcessor {
        static readonly JsonMergeSettings _concatWriteNull = new JsonMergeSettings(){
            MergeArrayHandling = MergeArrayHandling.Concat,
            MergeNullValueHandling = MergeNullValueHandling.Merge,
        };
        static readonly JsonMergeSettings _unionWriteNull = new JsonMergeSettings(){
            MergeArrayHandling = MergeArrayHandling.Union,
            MergeNullValueHandling = MergeNullValueHandling.Merge,
        };
        static readonly JsonMergeSettings _replaceWriteNull = new JsonMergeSettings(){
            MergeArrayHandling = MergeArrayHandling.Replace,
            MergeNullValueHandling = MergeNullValueHandling.Merge,
        };

        /// <summary>
        /// Throws in the event the instruction fails.
        /// </summary>
        public JObject Apply(Instruction ins, JObject target) {
            //Bail if we have no Target - we can't apply any pathed commands without one.
            if(target == null) return target;

            //JPath-ful commands
            var tokens = target.SelectTokens(ins.Target).ToList();
            var mergeType = _concatWriteNull;
            foreach(var tok in tokens) {
                var toka = tok as JArray;
                var toko = tok as JObject;
                switch(ins.Command) {
                    //Target-Typed .. ... ... ... ... ... ... ... ...
                    #region Replace . ... ... ... ... ... ... ... ...

                    case Command.Replace:
                        if(tok.Type != JTokenType.Null) {
                            CommandTargetTypeException.ThrowIfMismatch(
                                ins.Command, ins.Data.Type, tok.Type, 
                                allowNumber: true
                            );
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
                    #region Concat & Union .. ... ... ... ... ... ...

                    case Command.Concat:
                        switch(tok.Type) {
                            case JTokenType.Array:
                                toka.Merge(ins.Data, mergeType);
                                break;
                            case JTokenType.Null:
                                tok.Replace(ins.Data);
                                break;
                            default:
                                throw new CommandTargetTypeException(ins.Command, JTokenType.Array, tok.Type);
                        }
                        break;
                    case Command.ConcatNew:
                        mergeType = _unionWriteNull;
                        goto case Command.Concat;

                    #endregion
                    #region SpreadAfter & SpreadBefore .. ... ... ...

                    case Command.SpreadPost:
                        CommandTargetTypeException.ThrowIfMismatch(
                            ins.Command, JTokenType.Array, tok.Parent.Parent.Type, 
                            isParent: true
                        );
                        foreach(var ele in ins.DataArr.Reverse()) {
                            tok.AddAfterSelf(ele);
                        }
                        break;
                    case Command.SpreadPre:
                        CommandTargetTypeException.ThrowIfMismatch(
                            ins.Command, JTokenType.Array, tok.Parent.Parent.Type, 
                            isParent: true
                        );
                        foreach(var ele in ins.DataArr) {
                            tok.AddBeforeSelf(ele);
                        }
                        break;

                    #endregion
                    

                    //Object-Typed .. ... ... ... ... ... ... ... ...
                    #region The Almighty Merge .. ... ... ... ... ...

                    case Command.Merge:
                        switch(tok.Type) {
                            case JTokenType.Object:
                                toko.Merge(ins.Data, mergeType);
                                break;
                            case JTokenType.Null:
                                tok.Replace(ins.Data);
                                break;
                            default:
                                throw new CommandTargetTypeException(ins.Command, JTokenType.Object, tok.Type);
                        }
                        break;
                    case Command.MergePatch:
                        mergeType = _unionWriteNull;
                        goto case Command.Merge;
                    case Command.MergeHard:
                        mergeType = _replaceWriteNull;
                        goto case Command.Merge;

                        #endregion
                }
                if(ins.FirstMatchOnly) break;
            }
            return target;
        }

    }
}
