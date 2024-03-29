﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Reflection;

using Eirshy.PB.PressXToJson.Exceptions;

namespace Eirshy.PB.PressXToJson {
    internal static class Extensions {

        internal static TValue GetOrNew<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key) where TValue : new() {
            if(dict.TryGetValue(key, out TValue value)) return value;
            value = new TValue();
            dict[key] = value;
            return value;
        }

        internal static void ForEach<T>(this IEnumerable<T> iet, Action<T> action) {
            foreach(var t in iet) action(t);
        }
        internal static void ForEach<T, TDiscarded>(this IEnumerable<T> iet, Func<T,TDiscarded> funcResultDiscarded) {
            foreach(var t in iet) _ = funcResultDiscarded(t);
        }
        internal static void ForEach<T>(this IEnumerable<T> iet, Action<T,int> action) {
            int i = 0;
            foreach(var t in iet) action(t, i++);
        }
        internal static void ForEach<T, TDiscarded>(this IEnumerable<T> iet, Func<T, int, TDiscarded> funcResultDiscarded) {
            int i = 0;
            foreach(var t in iet) _ = funcResultDiscarded(t, i++);
        }
    }
}
