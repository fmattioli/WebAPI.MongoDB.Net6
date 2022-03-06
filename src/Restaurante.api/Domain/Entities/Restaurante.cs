using FluentValidation;
using FluentValidation.Results;
using Restaurante.api.Domain.Enums;
using Restaurante.api.Domain.ValueObjects;

namespace Restaurante.api.Domain.Entities
{
    public class RestauranteEntity : AbstractValidator<RestauranteEntity>
    {
        public RestauranteEntity()
        {

        }

        public RestauranteEntity(string? nome, ECozinha cozinha)
        {
            Nome = nome;
            Cozinha = cozinha;
        }
        public RestauranteEntity(string id, string nome, ECozinha cozinha)
        {
            Id = id;
            Nome = nome;
            Cozinha = cozinha;
        }

        public string? Id { get; private set; }
        public string? Nome { get; private set; }
        public ECozinha Cozinha { get; private set; }
        public Endereco? Endereco { get; private set; }
        public List<Avaliacao> Avaliacoes { get; private set; } = new List<Avaliacao>();
        public ValidationResult? ValidationResult { get; set; }

        public void AtribuirEndereco(Endereco endereco)
        {
            Endereco = endereco;
        }
        public void InserirAvaliacao(Avaliacao avaliacao)
        {
            Avaliacoes.Add(avaliacao);
        }
        public virtual bool Validar()
        {
            ValidarNome();
            ValidationResult = Validate(this);

            ValidarEndereco();

            return ValidationResult.IsValid;
        }

        private void ValidarNome()
        {
            RuleFor(c => c.Nome)
                .NotEmpty().WithMessage("Nome não pode ser vazio.")
                .MaximumLength(30).WithMessage("Nome pode ter no maximo 30 caracteres.");
        }

        private void ValidarEndereco()
        {
            if (Endereco is not null && Endereco.Validar())
                return;

            if (Endereco?.ValidationResult is not null)
            {
                foreach (var erro in Endereco.ValidationResult.Errors)
                    ValidationResult?.Errors.Add(erro);
            }
        }
    }
}