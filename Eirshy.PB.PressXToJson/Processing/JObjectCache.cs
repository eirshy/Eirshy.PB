using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json.Linq;

using Eirshy.PB.PressXToJson.Entities;

namespace Eirshy.PB.PressXToJson.Processing {
    internal class JObjectCache<T> {
        private IDictionary<string, T> _backing { get; }
        private Dictionary<string, JObject> _cache { get; }
        private Dictionary<string, HashSet<Logger>> _keyedLoggers { get; }

        public JObjectCache(IDictionary<string, T> backedBy) {
            _backing = backedBy;
            _cache = new Dictionary<string, JObject>(backedBy.Count);
            _keyedLoggers = new Dictionary<string, HashSet<Logger>>(backedBy.Count);
        }


        private JObject _get(string key) {
            if(!_cache.TryGetValue(key, out var ret)) {
                if(_backing.TryGetValue(key, out var original)) {
                    ret = original.ToJObject();
                } else ret = null;
                _cache.Add(key, ret);
            }
            return ret;
        }
        /// <summary>
        /// Gets the key, optionally adding the passed logger to the list that need informing
        /// </summary>
        /// <param name="logsAs">If null, will not be considered modified due to this getter.</param>
        public JObject Get(string key, Logger logsAs = null) {
            if(logsAs != null) _ = _keyedLoggers.GetOrNew(key).Add(logsAs);
            return _get(key);
        }
        /// <summary>
        /// Mimics a Dictionary's TryGetValue, still generates a new cache entry on miss.
        /// </summary>
        /// <param name="logsAs">If null, will not be considered modified due to this getter.</param>
        public bool TryGetValue(string key, out JObject value, Logger logsAs = null) {
            value = _get(key);
            if(logsAs != null) _ = _keyedLoggers.GetOrNew(key).Add(logsAs);
            return value != null;
        }

        public void Set(string key, JObject value, Logger logsAs) {
            if(logsAs != null) _ = _keyedLoggers.GetOrNew(key).Add(logsAs);
            _cache[key] = value;
        }

        public IEnumerable<KeyValuePair<string, JObject>> All() {
            var keys = _backing.Keys;
            foreach(var key in keys) {
                var value =_get(key);
                if(value is null) continue;
                yield return new KeyValuePair<string, JObject>(key, value);
            }
        }
        public IEnumerable<KeyValuePair<string, JObject>> Cached() => _cache;
        public IEnumerable<KeyValuePair<string, JObject>> Modified() 
            => _cache.Where(kvp => _keyedLoggers.ContainsKey(kvp.Key));

        public int CountAll => _backing.Count + _cache.Count(kvp => !_backing.ContainsKey(kvp.Key));
        public int CountCached => _cache.Count;
        public int CountModified => _keyedLoggers.Count;

        public void Save() {
            Logger.Orphan.Info($"Saving {CountModified} entries");
            foreach(var keylogs in _keyedLoggers) {
                var val = _cache[keylogs.Key];
                if(val is null) {
                    _ = _backing.Remove(keylogs.Key);
                    continue;
                };
                try {
                    var typed = val.ToType<T>();
                    keylogs.Value.LogForSet(logger => logger.DeserializeSuccess(keylogs.Key, typed));
                    _backing[keylogs.Key] = typed;
                } catch (Exception ex) {
                    keylogs.Value.LogForSet(logger => logger.DeserializeError(keylogs.Key, typeof(T), val, ex));
                }
            }
        }

        public void LogForKey(string key, Action<Logger> logAction) {
            if(_keyedLoggers.TryGetValue(key, out var loggers)) {
                loggers.LogForSet(logAction);
            }
        }
    }
}
