using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using Restaurante.api.Data.Schemas;
using Restaurante.api.Domain.Entities;
using Restaurante.api.Domain.Enums;

namespace Restaurante.api.Data
{
    public class MongoDB
    {
        public IMongoDatabase DB {get;}
        public MongoDB(IConfiguration configuration)
        {
            try
            {
                var client = new MongoClient(configuration["ConnectionString"]);
                DB = client.GetDatabase(configuration["NomeBanco"]);
                MapClasses();
            }
            catch(Exception ex)
            {
                throw new MongoException("Não foi possível se conectar ao MongoDB", ex);
            }
        }
        
        private void MapClasses()
        {
            if(!BsonClassMap.IsClassMapRegistered(typeof(RestauranteSchema)))
            {
                BsonClassMap.RegisterClassMap<RestauranteSchema>(i => {
                    i.AutoMap();
                    i.MapIdMember(c => c.Id);
                    i.MapMember(c => c.Cozinha).SetSerializer(new EnumSerializer<ECozinha>(BsonType.Int32));
                    i.SetIgnoreExtraElements(true);
                });
            }
        }
    }
}