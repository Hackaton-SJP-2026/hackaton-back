namespace Hackaton.Domain.Enums;

public enum StatusChamado { Aberto = 0, EmAnalise = 1, Resolvido = 2 }

public static class StatusChamadoTexto
{
    public static string Texto(this StatusChamado status) => status switch
    {
        StatusChamado.Aberto => "Aberto",
        StatusChamado.EmAnalise => "Em Análise",
        StatusChamado.Resolvido => "Resolvido",
        _ => throw new ArgumentOutOfRangeException(nameof(status))
    };

    public static bool TryParse(string? texto, out StatusChamado status)
    {
        status = texto switch
        {
            "Aberto" => StatusChamado.Aberto,
            "Em Análise" => StatusChamado.EmAnalise,
            "Resolvido" => StatusChamado.Resolvido,
            _ => (StatusChamado)(-1)
        };
        return Enum.IsDefined(status);
    }
}
