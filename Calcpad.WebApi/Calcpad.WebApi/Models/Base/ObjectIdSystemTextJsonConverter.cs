using MongoDB.Bson;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Calcpad.WebApi.Models.Base
{
    /// <summary>
    /// System.Text.Json ObjectId converter
    /// </summary>
    public class ObjectIdSystemTextJsonConverter : JsonConverter<ObjectId>
    {
        public override ObjectId Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
            {                
                return ObjectId.Empty;
            }

            var objectIdString = reader.GetString();
            if (string.IsNullOrWhiteSpace(objectIdString))
            {
                return ObjectId.Empty;
            }

            
            if (ObjectId.TryParse(objectIdString, out var oid))
            {
                return oid;
            }

            throw new JsonException($"Invalid ObjectId value: '{objectIdString}'");
        }

        public override void Write(Utf8JsonWriter writer, ObjectId value, JsonSerializerOptions options)
        {
            if (value.Equals(ObjectId.Empty)) writer.WriteStringValue(string.Empty);
            else writer.WriteStringValue(value.ToString());
        }
    }
}
