using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MotoRental.Domain.Entities
{
    public class Notification
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

        public string Message { get; set; } = string.Empty;

        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public string EventType { get; set; } = string.Empty;

        public object EventData { get; set; } = new();
    }
}
