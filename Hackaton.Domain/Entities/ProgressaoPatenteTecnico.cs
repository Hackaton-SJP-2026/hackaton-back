namespace Hackaton.Domain.Entities;

public sealed record DefinicaoPatenteTecnico(string Nome, int PontosMinimos, string Simbolo);

public sealed record ProgressoPatenteTecnico(
    DefinicaoPatenteTecnico Patente,
    DefinicaoPatenteTecnico? Proxima,
    int Pontos,
    int Faltam,
    int ProgressoPercentual);

public static class ProgressaoPatenteTecnicoCalculator
{
    public static readonly IReadOnlyList<DefinicaoPatenteTecnico> Patentes =
    [
        new("Soldado raso", 0, "▰"),
        new("Soldado", 100, "❯"),
        new("Cabo", 250, "❯❯"),
        new("Sargento", 500, "❯❯❯"),
        new("Subtenente", 800, "◆"),
        new("Tenente", 1200, "✦"),
        new("Capitão", 1800, "✦✦"),
        new("Major", 2600, "✦✦✦"),
        new("Coronel", 3600, "★"),
        new("General", 5000, "★★")
    ];

    public static ProgressoPatenteTecnico Calcular(int pontos)
    {
        var total = Math.Max(0, pontos);
        var indice = 0;
        for (var i = 1; i < Patentes.Count && total >= Patentes[i].PontosMinimos; i++)
            indice = i;

        var patente = Patentes[indice];
        var proxima = indice + 1 < Patentes.Count ? Patentes[indice + 1] : null;
        if (proxima is null) return new(patente, null, total, 0, 100);

        var faixa = proxima.PontosMinimos - patente.PontosMinimos;
        var progresso = (int)Math.Clamp((total - patente.PontosMinimos) * 100d / faixa, 0, 100);
        return new(patente, proxima, total, Math.Max(0, proxima.PontosMinimos - total), progresso);
    }
}
