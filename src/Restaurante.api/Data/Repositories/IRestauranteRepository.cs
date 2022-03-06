using Restaurante.api.Domain.Entities;
using Restaurante.api.Domain.Enums;
using Restaurante.api.Domain.ValueObjects;

namespace Restaurante.api.Data.Repositories
{
    public interface IRestauranteRepository
    {
        void Inserir(RestauranteEntity restaurante);
        Task<IEnumerable<RestauranteEntity>> ObterTodos();
        RestauranteEntity ObterPorId(string id);
        Task<bool> AlterarCompleto(RestauranteEntity restaurante);
        Task<bool> AlterarCozinha(string id, ECozinha cozinha);
        IEnumerable<RestauranteEntity> ObterPorNome(string nome);
        Task Avaliar(string restauranteId, Avaliacao avaliacao);
        Task<Dictionary<RestauranteEntity, double>> ObterTop3();
        Task<(long, long)> Remover(string restauranteId);
    }
}
