using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eirshy.PB.PressXToJson.Entities {
    internal class ModSettingsFile {
        public const string FILENAME = "json.settings.json";

        #region Owner & Physical Source

        internal ModSettings Owner { get; set; }
        internal string PhysicalSource { get; set; }

        #endregion
        #region Exception Spooling

        private List<Exception> _errors = new List<Exception>();
        /// <summary>
        /// Adds the passed exception to our error list, and disables us.
        /// <br />NOT THREADSAFE
        /// </summary>
        internal void AddError(Exception ex) {
            //Disabled = true;
            _errors.Add(ex);
        }
        internal void FlushErrors(bool useShortName = false) {
            var flushing = _errors;
            _errors = new List<Exception>();
            var name = useShortName ? ShortName: Name;
            foreach(var ex in flushing) {
                Owner.Log.Error(name, ex);
            }
        }

        #endregion

        internal string Name => $"{Owner.Name}::{ShortName}";
        internal string ShortName => "Settings";
    }
}
