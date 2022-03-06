using Restaurante.api.Data.Schemas;
using MongoDB.Driver;
using Restaurante.api.Domain.Entities;
using Restaurante.api.Domain.ValueObjects;
using Restaurante.api.Domain.Enums;
using MongoDB.Bson;

namespace Restaurante.api.Data.Repositories
{
    public class RestauranteRepository : IRestauranteRepository
    {
        private readonly IMongoCollection<RestauranteSchema> _restaurantes;
        private readonly IMongoCollection<AvaliacaoSchema> _avaliacoes;

        public RestauranteRepository(MongoDB mongoDB)
        {
            _restaurantes = mongoDB.DB.GetCollection<RestauranteSchema>("restaurantes");
            _avaliacoes = mongoDB.DB.GetCollection<AvaliacaoSchema>("avaliacoes");
        }

        public void Inserir(RestauranteEntity restaurante)
        {
            var document = new RestauranteSchema
            {
                Nome = restaurante.Nome ?? "",
                Cozinha = restaurante.Cozinha,
                Endereco = new EnderecoSchema
                {
                    Logradouro = restaurante?.Endereco?.Logradouro,
                    Numero = restaurante?.Endereco?.Numero,
                    Cidade = restaurante?.Endereco?.Cidade,
                    Cep = restaurante?.Endereco?.Cep,
                    UF = restaurante?.Endereco?.UF
                }
            };

            _restaurantes.InsertOneAsync(document);
        }

        public async Task<IEnumerable<RestauranteEntity>> ObterTodos()
        {
            var restaurantes = new List<RestauranteEntity>();
            var filter = Builders<RestauranteSchema>.Filter.Empty;
            await _restaurantes.AsQueryable().ForEachAsync(d =>
                {
                    var r = new RestauranteEntity(d?.Id?.ToString() ?? "", d?.Nome ?? "", d?.Cozinha ?? Domain.Enums.ECozinha.Japonesa);
                    var e = new Endereco(d?.Endereco?.Logradouro, d?.Endereco?.Numero, d?.Endereco?.Cidade, d?.Endereco?.UF, d?.Endereco?.Cep);
                    r.AtribuirEndereco(e);
                    restaurantes.Add(r);
                }
            );

            return restaurantes;
        }

        public RestauranteEntity ObterPorId(string id)
        {
            var document = _restaurantes.AsQueryable().FirstOrDefault(a => a.Id == id);

            if (document is not null)
                return document.ConverterParaDomain();

            return new RestauranteEntity();

        }

        public async Task<bool> AlterarCompleto(RestauranteEntity restaurante)
        {
            var document = new RestauranteSchema
            {
                Id = restaurante.Id,
                Nome = restaurante.Nome ?? "",
                Cozinha = restaurante.Cozinha,
                Endereco = new EnderecoSchema
                {
                    Logradouro = restaurante?.Endereco?.Logradouro,
                    Numero = restaurante?.Endereco?.Numero,
                    Cidade = restaurante?.Endereco?.Cidade,
                    Cep = restaurante?.Endereco?.Cep,
                    UF = restaurante?.Endereco?.UF
                }
            };

            var restauranteId = restaurante?.Id ?? "";
            var resultado = await _restaurantes.ReplaceOneAsync(r => r.Id == restauranteId, document);
            return resultado.ModifiedCount > 0;
        }

        public async Task<bool> AlterarCozinha(string id, ECozinha cozinha)
        {
            var atualizacao = Builders<RestauranteSchema>.Update.Set(c => c.Cozinha, cozinha);
            var resultado = await _restaurantes.UpdateOneAsync(r => r.Id == id, atualizacao);
            return resultado.ModifiedCount > 0;
        }

        public IEnumerable<RestauranteEntity> ObterPorNome(string nome)
        {
            var restaurantes = new List<RestauranteEntity>();

            _restaurantes.AsQueryable()
                .Where(r => r.Nome.ToLower().Contains(nome.ToLower()))
                .ToList()
                .ForEach(d => restaurantes.Add(d.ConverterParaDomain()));

            return restaurantes;
        }

        public async Task Avaliar(string restauranteId, Avaliacao avaliacao)
        {
            var document = new AvaliacaoSchema
            {
                RestauranteId = restauranteId,
                Estrelas = avaliacao.Estrelas,
                Comentario = avaliacao.Comentario
            };

            await _avaliacoes.InsertOneAsync(document);
        }

        public async Task<Dictionary<RestauranteEntity, double>> ObterTop3()
        {
            var retorno = new Dictionary<RestauranteEntity, double>();
            var top3 = _avaliacoes.Aggregate()
                .Group(a => a.RestauranteId, g => new { RestauranteId = g.Key, MediaEstrelas = g.Average(a => a.Estrelas) })
                .SortByDescending(m => m.MediaEstrelas)
                .Limit(3);

            await top3.ForEachAsync(m =>
            {
                var restaurante = ObterPorId(m.RestauranteId ?? "");
                _avaliacoes.AsQueryable()
                .Where(a => a.RestauranteId == m.RestauranteId)
                .ToList()
                .ForEach(a => restaurante.InserirAvaliacao(a.ConveterParaDomain()));

                retorno.Add(restaurante, m.MediaEstrelas);
            });

            return retorno;

        }

        public async Task<(long, long)> Remover(string restauranteId)
        {
            var resultadoAvaliacoes = await _avaliacoes.DeleteManyAsync(a => a.RestauranteId == restauranteId);
            var resultadoRestaurante = await _restaurantes.DeleteOneAsync(r => r.Id == restauranteId);

            return (resultadoRestaurante.DeletedCount, resultadoAvaliacoes.DeletedCount);
        }
    }
}