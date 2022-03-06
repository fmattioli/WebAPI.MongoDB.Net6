using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Restaurante.api.Domain.Entities;
using Restaurante.api.Domain.Enums;
using Restaurante.api.Domain.ValueObjects;

namespace Restaurante.api.Data.Schemas
{
    public class RestauranteSchema
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }
        public string Nome { get; set; }
        public ECozinha Cozinha { get; set; }
        public EnderecoSchema? Endereco { get; set; }
        public RestauranteSchema()
        {
            this.Nome = "";
        }
    }


    public static class RestauranteSchemaExtensao
    {
        public static RestauranteEntity ConverterParaDomain(this RestauranteSchema document)
        {
            var restaurante = new RestauranteEntity(document?.Id ?? "", document?.Nome ?? "", document?.Cozinha ?? ECozinha.Japonesa);
            var endereco = new Endereco(document?.Endereco?.Logradouro, document?.Endereco?.Numero, document?.Endereco?.Cidade, document?.Endereco?.UF, document?.Endereco?.Cidade);
            restaurante.AtribuirEndereco(endereco);
            return restaurante;
        }
    }
}