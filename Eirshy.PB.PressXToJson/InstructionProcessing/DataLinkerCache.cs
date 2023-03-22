using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json.Linq;

namespace Eirshy.PB.PressXToJson.InstructionProcessing {
    internal class DataLinkerCache<T> {
        private SortedDictionary<string, T> _wrapped { get; }
        private Dictionary<string, JObject> _cache { get; }

        public DataLinkerCache(SortedDictionary<string,T> wrap) {
            _wrapped = wrap;
            _cache = new Dictionary<string, JObject>(wrap.Count);
        }

        public JObject this[string key] {
            get {
                if(!_cache.TryGetValue(key, out var ret)) {
                    if(_wrapped.TryGetValue(key, out var original)) {
                        ret = original.ToJObject();
                    } else ret = null;
                    _cache.Add(key, ret);
                }
                return ret;
            }
            set => _cache[key] = value;
        }

        /// <summary>
        /// Mimics a Dictionary's TryGetValue, still generates a new cache entry
        /// </summary>
        public bool TryGetValue(string key, out JObject value) {
            value = this[key];
            return value != null;
        }


        public IEnumerable<KeyValuePair<string, JObject>> All() {
            var keys = _wrapped.Keys;
            foreach(var key in keys) {
                var value = this[key];
                if(value is null) continue;
                yield return new KeyValuePair<string, JObject>(key, value);
            }
        }
        public IEnumerable<KeyValuePair<string, JObject>> Cached() {
            return _cache;
        }

        public void Save() {
            foreach(var kvp in _cache) {
                if(kvp.Value is null) {
                    _ = _wrapped.Remove(kvp.Key);
                    continue;
                };
                try {
                    var typed = kvp.Value.ToType<T>();
                    Logger.Orphan.DeserializeSuccess(kvp.Key, typed);
                    _wrapped[kvp.Key] = typed;
                } catch (Exception ex) {
                    Logger.Orphan.DeserializeError(kvp.Key, typeof(T), kvp.Value, ex);
                }
            }
        }

    }
}
