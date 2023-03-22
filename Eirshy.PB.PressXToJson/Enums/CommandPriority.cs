using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eirshy.PB.PressXToJson.Enums {
    internal enum CommandPriority {
        //order here is actual order
        Meta,
        RootCreate,
        RootCreateFrom,
        General,
        Error,
    }
}
