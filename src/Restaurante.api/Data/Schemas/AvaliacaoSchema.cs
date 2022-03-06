using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Restaurante.api.Domain.ValueObjects;

namespace Restaurante.api.Data.Schemas
{
    public class AvaliacaoSchema
    {
        public ObjectId Id { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string? RestauranteId { get; set; }
        public int Estrelas { get; set; }
        public string? Comentario { get; set; }
    }


    public static class AvaliacaoSchemaExtenssao
    {
        public static Avaliacao ConveterParaDomain(this AvaliacaoSchema document)
        {
            return new Avaliacao(document.Estrelas, document.Comentario ?? "");
        }
    }
}
