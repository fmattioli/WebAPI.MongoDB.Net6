using Microsoft.AspNetCore.Mvc;
using Restaurante.api.Controllers.Inputs;
using Restaurante.api.Controllers.Outputs;
using Restaurante.api.Data.Repositories;
using Restaurante.api.Domain.Entities;
using Restaurante.api.Domain.Enums;
using Restaurante.api.Domain.ValueObjects;

namespace Restaurante.api.Controllers;

[ApiController]
public class RestauranteController : ControllerBase
{
    private readonly IRestauranteRepository restauranteRepository;
    public RestauranteController(IRestauranteRepository restauranteRepository)
    {
        this.restauranteRepository = restauranteRepository;
    }

    [HttpPost("restaurante")]
    public ActionResult IncluirRestaurante([FromBody] RestauranteInclusao restauranteInclusao)
    {
        var cozinha = ECozinhaHelper.ConverterDeInteiro(restauranteInclusao.Cozinha);

        var restaurante = new RestauranteEntity(restauranteInclusao.Nome, cozinha);
        var endereco = new Endereco
        (
            restauranteInclusao?.Logradouro,
            restauranteInclusao?.Numero,
            restauranteInclusao?.Cidade,
            restauranteInclusao?.UF,
            restauranteInclusao?.Cep
        );

        restaurante.AtribuirEndereco(endereco);

        if (!restaurante.Validar())
        {
            return BadRequest(new { errors = restaurante?.ValidationResult?.Errors.Select(a => a.ErrorMessage) });
        }

        restauranteRepository.Inserir(restaurante);

        return Ok(new { data = "Restaurante inserido com sucesso!" });
    }

    [HttpGet("restaurante/todos")]
    public async Task<ActionResult> ObterRestaurantes()
    {
        var restaurantes = await restauranteRepository.ObterTodos();
        var listagem = restaurantes.Select(a => new RestauranteListagem
        {
            Id = a?.Id ?? "",
            Nome = a?.Nome ?? "",
            ECozinha = Convert.ToInt32(a?.Cozinha),
            Cidade = a?.Endereco?.Cidade
        });

        var retorno = new { data = listagem };

        return Ok(retorno);
    }

    [HttpGet("restaurante/{id}")]
    public ActionResult ObterRestaurante(string id)
    {
        var restaurante = restauranteRepository.ObterPorId(id);

        if (restaurante is null)
            return NotFound();

        var exibicao = new RestauranteExibicao
        {
            Id = restaurante.Id,
            Nome = restaurante.Nome,
            Cozinha = Convert.ToInt32(restaurante.Cozinha),
            Endereco = new EnderecoExibicao
            {
                Logradouro = restaurante?.Endereco?.Logradouro,
                Numero = restaurante?.Endereco?.Numero,
                Cidade = restaurante?.Endereco?.Cidade,
                Cep = restaurante?.Endereco?.Cep,
                UF = restaurante?.Endereco?.UF,
            }
        };

        var retorno = new { data = exibicao };

        return Ok(retorno);
    }

    [HttpPut("restaurante")]
    public async Task<ActionResult> AlterarRestaurante([FromBody] RestauranteAlteracaoCompleta restauranteAlteracaoCompleta)
    {
        var restaurante = restauranteRepository.ObterPorId(restauranteAlteracaoCompleta.Id ?? "");

        if (restaurante == null)
            return NotFound();

        var cozinha = ECozinhaHelper.ConverterDeInteiro(restauranteAlteracaoCompleta.Cozinha);
        restaurante = new RestauranteEntity(restauranteAlteracaoCompleta.Id ?? "", restauranteAlteracaoCompleta?.Nome ?? "", cozinha);
        var endereco = new Endereco(
            restauranteAlteracaoCompleta?.Logradouro,
            restauranteAlteracaoCompleta?.Numero,
            restauranteAlteracaoCompleta?.Cidade,
            restauranteAlteracaoCompleta?.UF,
            restauranteAlteracaoCompleta?.Cep);

        restaurante.AtribuirEndereco(endereco);

        if (!restaurante.Validar())
            return BadRequest(new { errors = restaurante?.ValidationResult?.Errors.Select(_ => _.ErrorMessage) });

        var retorno = await restauranteRepository.AlterarCompleto(restaurante);
        if (!retorno)
            return BadRequest(new { errors = "Nenhum documento foi alterado" });

        return Ok(new { data = "Restaurante alterado com sucesso" });
    }

    [HttpPatch("restaurante/{id}")]
    public async Task<ActionResult> AlterarCozinha(string id, [FromBody] RestauranteAlteracaoParcial restauranteAlteracaoParcial)
    {
        var restaurante = restauranteRepository.ObterPorId(id);

        if (restaurante == null)
            return NotFound();

        var cozinha = ECozinhaHelper.ConverterDeInteiro(restauranteAlteracaoParcial.Cozinha);
        var retorno = await restauranteRepository.AlterarCozinha(id, cozinha);
        if (!retorno)
        {
            return BadRequest(new { errors = "Nenhum documento foi alterado" });
        }

        return Ok(new { data = "Restaurante alterado com sucesso" });
    }

    [HttpGet("restaurante")]
    public ActionResult ObterRestaurantePorNome([FromQuery] string nome)
    {
        var restaurantes = restauranteRepository.ObterPorNome(nome);

        var listagem = restaurantes.Select(r => new RestauranteListagem
        {
            Id = r.Id ?? "",
            Nome = r.Nome ?? "",
            ECozinha = Convert.ToInt32(r.Cozinha),
            Cidade = r?.Endereco?.Cidade
        });

        return Ok(new { data = listagem });
    }

    [HttpPatch("restaurante/{id}/avaliar")]
    public async Task<ActionResult> AvaliarRestaurante(string id, [FromBody] AvaliacaoInclusao avaliacaoInclusao)
    {
        var restaurante = restauranteRepository.ObterPorId(id);

        if (restaurante == null)
            return NotFound();

        var avaliacao = new Avaliacao(avaliacaoInclusao.Estrelas, avaliacaoInclusao.Comentario ?? "");

        if (!avaliacao.Validar())
        {
            return BadRequest(new { errors = avaliacao?.ValidationResult?.Errors.Select(_ => _.ErrorMessage) });
        }

        await restauranteRepository.Avaliar(id, avaliacao);

        return Ok(new { data = "Restaurante avaliado com sucesso" });
    }

    [HttpGet("restaurante/top3")]
    public async Task<ActionResult> ObterTop3Restaurantes()
    {
        var top3 = await restauranteRepository.ObterTop3();

        var listagem = top3.Select(a => new RestauranteTop3
        {
            Id = a.Key.Id,
            Nome = a.Key.Nome,
            Cozinha = (int)a.Key.Cozinha,
            Cidade = a.Key?.Endereco?.Cidade,
            Estrelas = a.Value
        });

        return Ok(new { data = listagem });
    }

    [HttpDelete("restaurante/{id}")]
    public async Task<ActionResult> Remover(string id)
    {
        var restaurante = restauranteRepository.ObterPorId(id);

        if (restaurante.Nome == "")
            return NotFound();

        (var totalRestauranteRemovido, var totalAvaliacoesRemovidas) = await restauranteRepository.Remover(id);

        return Ok(new { data = $"Total de exclusões: {totalRestauranteRemovido} restaurante com {totalAvaliacoesRemovidas} avaliações" });
    }
}
