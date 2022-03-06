namespace Restaurante.api.Domain.Enums
{
    public enum ECozinha
    {
        Brasileira,
        Italiana,
        Arabe,
        Japonesa,
        FastFood
    }

    public static class ECozinhaHelper
    {
        public static ECozinha ConverterDeInteiro(int valor)
        {
            if(Enum.TryParse(valor.ToString(), out ECozinha cozinha))
                return cozinha;

            throw new ArgumentOutOfRangeException("cozinha");
        }
    }
}