using MongoDB.Bson;
using Newtonsoft.Json;

namespace Calcpad.WebApi.Models.Base
{
    /// <summary>
    /// newtonsoft.json ObjectId converter
    /// </summary>
    public class ObjectIdNewtonsoftConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType) => objectType == typeof(ObjectId) || objectType == typeof(ObjectId?);

        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            var s = reader.Value?.ToString();
            if (string.IsNullOrEmpty(s)) return ObjectId.Empty;
            return ObjectId.Parse(s);
        }

        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            if (value is ObjectId oid && oid != ObjectId.Empty) writer.WriteValue(oid.ToString());
            else writer.WriteValue(string.Empty);
        }
    }
}
