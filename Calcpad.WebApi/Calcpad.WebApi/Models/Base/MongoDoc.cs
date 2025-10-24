using System.Text.Json.Serialization;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Calcpad.WebApi.Models.Base
{
    [BsonIgnoreExtraElements]
    public class MongoDoc
    {
        /// <summary>
        /// Id
        /// </summary>
        [JsonPropertyName("_id"), Newtonsoft.Json.JsonProperty("_id")]
        [BsonId]
        public ObjectId Id { get; set; } = ObjectId.GenerateNewId();

        /// <summary>
        /// create date in UTC
        /// </summary>
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime CreateDate { get; set; } = DateTime.UtcNow;
    }
}
