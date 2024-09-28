using MongoDB.Bson.Serialization.Attributes;

namespace Stock.API.Entities
{
    public class Stock
    {
        [BsonId]
        [BsonRepresentation(MongoDB.Bson.BsonType.Int64)]
        [BsonElement(Order = 0)]
        public int Id { get; set; }

        [BsonRepresentation(MongoDB.Bson.BsonType.Int64)]
        [BsonElement(Order = 1)]
        public int ProductId { get; set; }

        [BsonRepresentation(MongoDB.Bson.BsonType.Int64)]
        [BsonElement(Order = 2)]
        public int Quantity { get; set; }
    }
}

