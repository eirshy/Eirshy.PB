using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Eirshy.PB.PressXToJson.Enums {

    internal enum Command {
        Unknown,
        #region Macros  ... ... ... ...

        MacroDefine,
        MacroExec,
        MacroInline,
        MacroVar,

        #endregion
        #region Create  ... ... ... ...

        /// <summary>
        /// PATH TARGET: Ignored
        /// <br />DATA TYPE: As File Type
        /// <br />Creates a new instance of the file's type
        /// <br /><c>configs[FileName] = Data;</c>
        /// </summary>
        New,
        /// <summary>
        /// PATH TARGET: Ignored
        /// <br />DATA TYPE: As File Type
        /// <br />Creates a new instance of the file's type
        /// <br /><c>configs[FileName] = Data;</c>
        /// </summary>
        Overwrite,

        #endregion
        #region Copy .. ... ... ... ... 

        /// <summary>
        /// PATH TARGET: Ignored
        /// <br />DATA TYPE: same-type name
        /// <br />Clones another type with the given name. Fails on Unique data types.
        /// <br /><c>configs[FileName](configs[Data]);</c>
        /// </summary>
        PriorityCopy,

        /// <summary>
        /// PATH TARGET: Ignored
        /// <br />DATA TYPE: same-type name
        /// <br />Clones another type with the given name. Fails on Unique data types.
        /// <br /><c>configs[FileName](configs[Data]);</c>
        /// </summary>
        Copy,

        #endregion
        #region Modify  ... ... ... ...

        /// <summary>
        /// PATH TARGET: Any
        /// <br />DATA TYPE: Ignored
        /// <br />Removes all targets
        /// <br /><c>target = undefined;</c>
        /// </summary>
        Remove,
        /// <summary>
        /// PATH TARGET: Any
        /// <br />DATA TYPE: As Target
        /// <br />Replaces all instances targeted
        /// <br /><c>target = Data;</c>
        /// </summary>
        Replace,
        /// <summary>
        /// PATH TARGET: Lists
        /// <br />DATA TYPE: As Target
        /// <br />Appends all elements in Data to all targeted lists
        /// <br /><c>target = [...target, ...Data];</c>
        /// </summary>
        Concat,
        /// <summary>
        /// PATH TARGET: Element(s) in List(s)
        /// <br />DATA TYPE: As Containing List
        /// <br />Inserts all elements in Data before all targets
        /// <br /><c>parent = [...earlierSiblings, ...Data, target, ...laterSiblings];</c>
        /// </summary>
        SpreadBefore,
        /// <summary>
        /// PATH TARGET: Element(s) in List(s)
        /// <br />DATA TYPE: As Containing List
        /// <br />Inserts all elements in Data after all targets
        /// <br /><c>parent = [...earlierSiblings, target, ...Data, ...laterSiblings];</c>
        /// </summary>
        SpreadAfter,
        /// <summary>
        /// PATH TARGET: Objects
        /// <br />DATA TYPE: As Matching
        /// <br />Merges all keys into the targets, replacing pre-existing keys.
        /// <br /><c>target = {...target, ...Data};</c>
        /// </summary>
        Merge,

        #endregion

        //dots. ... ... ... ... ... ...
    }

    internal static class CommandExtensions {
        public static CommandPriority ToPriority(this Command cmd) {
            switch(cmd) {
                case Command.MacroDefine:
                case Command.MacroExec:
                case Command.MacroInline:
                case Command.MacroVar:
                    return CommandPriority.Meta;

                case Command.New:
                    return CommandPriority.RootCreate;

                case Command.PriorityCopy:
                    return CommandPriority.RootCreateFrom;

                case Command.Unknown:
                    return CommandPriority.Error;

                default:
                    return CommandPriority.General;
            }
        }
        public static int ComparePriority(this Command cmd, Command other) {
            return (int)cmd.ToPriority() - (int)other.ToPriority();
        }
    }
}
