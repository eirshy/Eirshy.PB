using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using UnityEngine;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Utilities;

namespace Eirshy.PB.PressXToJson {
    internal static class JsonExtensions {
        #region Converters : Color & Vec 2-4 & VecI 2-3 deserialize (as objects)

        private static partial class Converters {

            private class RGBA<T> {
                public T r { get; set; }
                public T g { get; set; }
                public T b { get; set; }
                public T a { get; set; }
            }
            private class XYZW<T> {
                public T x { get; set; }
                public T y { get; set; }
                public T z { get; set; }
                public T w { get; set; }
            }
            public class Clr : JsonConverter<Color> {
                public override void WriteJson(JsonWriter writer, Color value, JsonSerializer serializer) {
                    writer.WriteStartObject();
                    writer.WritePropVal(nameof(Color.r), value.r);
                    writer.WritePropVal(nameof(Color.g), value.g);
                    writer.WritePropVal(nameof(Color.b), value.b);
                    writer.WritePropVal(nameof(Color.a), value.a);
                    writer.WriteEndObject();
                }
                public override Color ReadJson(JsonReader reader, Type objectType, Color existingValue, bool hasExistingValue, JsonSerializer serializer) {
                    var rgba = serializer.Deserialize<RGBA<float?>>(reader);
                    if(rgba == null) return existingValue;
                    if(rgba.r.HasValue) existingValue.r = rgba.r.Value;
                    if(rgba.g.HasValue) existingValue.g = rgba.g.Value;
                    if(rgba.b.HasValue) existingValue.b = rgba.b.Value;
                    if(rgba.a.HasValue) existingValue.a = rgba.a.Value;
                    return existingValue;
                }
            }
            public class Vec4 : JsonConverter<Vector4> {
                public override void WriteJson(JsonWriter writer, Vector4 value, JsonSerializer serializer) {
                    writer.WriteStartObject();
                    writer.WritePropVal(nameof(Vector4.x), value.x);
                    writer.WritePropVal(nameof(Vector4.y), value.y);
                    writer.WritePropVal(nameof(Vector4.z), value.z);
                    writer.WritePropVal(nameof(Vector4.w), value.w);
                    writer.WriteEndObject();
                }
                public override Vector4 ReadJson(JsonReader reader, Type objectType, Vector4 existingValue, bool hasExistingValue, JsonSerializer serializer) {
                    var xyzw = serializer.Deserialize<XYZW<float?>>(reader);
                    if(xyzw == null) return existingValue;
                    if(xyzw.x.HasValue) existingValue.x = xyzw.x.Value;
                    if(xyzw.y.HasValue) existingValue.y = xyzw.y.Value;
                    if(xyzw.z.HasValue) existingValue.z = xyzw.z.Value;
                    if(xyzw.w.HasValue) existingValue.w = xyzw.w.Value;
                    return existingValue;
                }
            }
            public class Vec3 : JsonConverter<Vector3> {
                public override void WriteJson(JsonWriter writer, Vector3 value, JsonSerializer serializer) {
                    writer.WriteStartObject();
                    writer.WritePropVal(nameof(Vector3.x), value.x);
                    writer.WritePropVal(nameof(Vector3.y), value.y);
                    writer.WritePropVal(nameof(Vector3.z), value.z);
                    writer.WriteEndObject();
                }
                public override Vector3 ReadJson(JsonReader reader, Type objectType, Vector3 existingValue, bool hasExistingValue, JsonSerializer serializer) {
                    var xyzw = serializer.Deserialize<XYZW<float?>>(reader);
                    if(xyzw == null) return existingValue;
                    if(xyzw.x.HasValue) existingValue.x = xyzw.x.Value;
                    if(xyzw.y.HasValue) existingValue.y = xyzw.y.Value;
                    if(xyzw.z.HasValue) existingValue.z = xyzw.z.Value;
                    return existingValue;
                }
            }
            public class Vec2 : JsonConverter<Vector2> {
                public override void WriteJson(JsonWriter writer, Vector2 value, JsonSerializer serializer) {
                    writer.WriteStartObject();
                    writer.WritePropVal(nameof(Vector2.x), value.x);
                    writer.WritePropVal(nameof(Vector2.y), value.y);
                    writer.WriteEndObject();
                }
                public override Vector2 ReadJson(JsonReader reader, Type objectType, Vector2 existingValue, bool hasExistingValue, JsonSerializer serializer) {
                    var xyzw = serializer.Deserialize<XYZW<float?>>(reader);
                    if(xyzw == null) return existingValue;
                    if(xyzw.x.HasValue) existingValue.x = xyzw.x.Value;
                    if(xyzw.y.HasValue) existingValue.y = xyzw.y.Value;
                    return existingValue;
                }
            }
            public class Vec3i : JsonConverter<Vector3Int> {
                public override void WriteJson(JsonWriter writer, Vector3Int value, JsonSerializer serializer) {
                    writer.WriteStartObject();
                    writer.WritePropVal(nameof(Vector3Int.x), value.x);
                    writer.WritePropVal(nameof(Vector3Int.y), value.y);
                    writer.WritePropVal(nameof(Vector3Int.z), value.z);
                    writer.WriteEndObject();
                }
                public override Vector3Int ReadJson(JsonReader reader, Type objectType, Vector3Int existingValue, bool hasExistingValue, JsonSerializer serializer) {
                    var xyzw = serializer.Deserialize<XYZW<int?>>(reader);
                    if(xyzw == null) return existingValue;
                    if(xyzw.x.HasValue) existingValue.x = xyzw.x.Value;
                    if(xyzw.y.HasValue) existingValue.y = xyzw.y.Value;
                    if(xyzw.z.HasValue) existingValue.z = xyzw.z.Value;
                    return existingValue;
                }
            }
            public class Vec2i : JsonConverter<Vector2Int> {
                public override void WriteJson(JsonWriter writer, Vector2Int value, JsonSerializer serializer) {
                    writer.WriteStartObject();
                    writer.WritePropVal(nameof(Vector2Int.x), value.x);
                    writer.WritePropVal(nameof(Vector2Int.y), value.y);
                    writer.WriteEndObject();
                }
                public override Vector2Int ReadJson(JsonReader reader, Type objectType, Vector2Int existingValue, bool hasExistingValue, JsonSerializer serializer) {
                    var xyzw = serializer.Deserialize<XYZW<int?>>(reader);
                    if(xyzw == null) return existingValue;
                    if(xyzw.x.HasValue) existingValue.x = xyzw.x.Value;
                    if(xyzw.y.HasValue) existingValue.y = xyzw.y.Value;
                    return existingValue;
                }
            }

        }
        #endregion
        #region Configs ... ...

        static readonly Lazy<JsonSerializerSettings> _jss = new Lazy<JsonSerializerSettings>(() => {
            var settings = new JsonSerializerSettings(){
                Formatting = Formatting.Indented,
                NullValueHandling = NullValueHandling.Include,
                MissingMemberHandling = MissingMemberHandling.Ignore,
                TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple,
                TypeNameHandling = TypeNameHandling.Auto,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            };
            settings.Converters.Add(new Converters.Clr());
            settings.Converters.Add(new Converters.Vec4());
            settings.Converters.Add(new Converters.Vec3());
            settings.Converters.Add(new Converters.Vec2());
            settings.Converters.Add(new Converters.Vec3i());
            settings.Converters.Add(new Converters.Vec2i());
            return settings;
        }, LazyThreadSafetyMode.PublicationOnly);
        static readonly Lazy<JsonSerializer> _js = new Lazy<JsonSerializer>(
            () => JsonSerializer.Create(_jss.Value)
            , LazyThreadSafetyMode.PublicationOnly
        );
        static readonly Lazy<JsonConverter[]> _jsc = new Lazy<JsonConverter[]>(()=>_jss.Value.Converters.ToArray(), LazyThreadSafetyMode.PublicationOnly);


        #endregion

        // strings
        internal static T JsonTo<T>(this string json) {
            return JsonConvert.DeserializeObject<T>(json, _jss.Value);
        }
        internal static string ToJson<T>(this T obj) {
            return JsonConvert.SerializeObject(obj, _jss.Value);
        }


        // JObjects
        internal static JObject ToJObject(this object obj) {
            return JObject.FromObject(obj, _js.Value);
        }
        internal static T ToType<T>(this JObject jobj) {
            return jobj.ToObject<T>(_js.Value);
        }
        internal static string ToRawJson(this JObject jobj) {
            return jobj.ToString(Formatting.Indented, _jsc.Value);
        }



        //Json Converters
        internal static void WritePropVal<T>(this JsonWriter writer, string name, T value) {
            writer.WritePropertyName(name);
            writer.WriteValue(value);
        }
    }
}
