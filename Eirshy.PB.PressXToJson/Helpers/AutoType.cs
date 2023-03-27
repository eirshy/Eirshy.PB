using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Reflection;

using Newtonsoft.Json.Linq;

using Eirshy.PB.PressXToJson.Exceptions;

namespace Eirshy.PB.PressXToJson.Helpers {
    internal struct AutoType {
        static readonly Regex _autoType = new Regex(@"^((?:\w+\.)*)(\w+)(?:$|[\s,]+([\w.-]*)$)");
        static readonly ConcurrentDictionary<string, Type> _types = new ConcurrentDictionary<string, Type>();

        public string NameSpace { get; }
        public string ClassName { get; }
        public string AssemblyName { get; }

        public bool IsValid => ClassName != null;

        private AutoType(string nameSpace, string className, string assemblyName) {
            NameSpace = nameSpace;
            ClassName = className;
            AssemblyName = assemblyName;
        }

        public static AutoType GetFrom(string typeString, string defaultNamespace, string defaultAssembly) {
            var value = typeString?.Trim();
            if(string.IsNullOrEmpty(value)) return default;

            var processed = _autoType.Match(value);
            if(!processed.Success) throw new AutoTypingException(value);

            var ns = processed.Groups[1].Value == "" ? defaultNamespace : processed.Groups[1].Value;
            var cls = processed.Groups[2].Value;
            var asm = processed.Groups[3].Value == "" ? defaultAssembly : processed.Groups[3].Value;
            if(ns.Last() == '.') ns = ns.Substring(0, ns.Length -1);
            return new AutoType(ns, cls, asm);
        }
        public static AutoType GetFrom(Type type) {
            return new AutoType(type.Namespace, type.Name, type.Assembly.GetName().Name);
        }

        /// <summary>
        /// Returns the format that `$type` expects
        /// </summary>
        public override string ToString() => $"{NameSpace}.{ClassName}, {AssemblyName}";
        public JProperty ToJProp() => new JProperty("$type", ToString());
        public Type ToType() {
            var str = ToString();
            if(_types.TryGetValue(str, out var ret)) return ret;
            try {
                return Type.GetType(ToString());
            } catch (Exception ex) {
                throw new AutoTypingException(str, ex);
            }
        }
    }
}