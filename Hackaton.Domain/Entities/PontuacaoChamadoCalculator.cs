namespace Hackaton.Domain.Entities;

public static class PontuacaoChamadoCalculator
{
    public static int Calcular(string nivelComplexidade, string prioridade)
    {
        var pontos = nivelComplexidade switch
        {
            "Baixo" => 5,
            "Médio" => 10,
            "Alto" => 15,
            _ => throw new ArgumentOutOfRangeException(nameof(nivelComplexidade), "Complexidade inválida.")
        };

        return pontos + (prioridade == "Alta" ? 5 : 0);
    }
}
