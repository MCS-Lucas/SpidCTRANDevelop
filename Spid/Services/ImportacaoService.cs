using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using Spid.Data;
using System.Globalization;

namespace Spid.Services;

public class ImportacaoResult
{
    public int Importadas { get; set; }
    public int Ignoradas { get; set; } // duplicatas
    public List<string> Erros { get; set; } = new();
}

public class ImportacaoService
{
    private readonly AppDbContext _db;

    public ImportacaoService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<ImportacaoResult> ImportarExcelAsync(Stream stream)
    {
        var result = new ImportacaoResult();

        using var workbook = new XLWorkbook(stream);
        var ws = workbook.Worksheets.First();

        // Pegar IDs de viagens já existentes para deduplicação
        var idsExistentes = await _db.Viagens
            .Select(v => v.IdViagemParceiro)
            .ToHashSetAsync();

        // Cache de entidades para evitar consultas repetidas
        var setoresCache = await _db.Setores.ToDictionaryAsync(s => s.Nome);
        var parceirosCache = await _db.Parceiros.ToDictionaryAsync(p => p.Nome);
        var colaboradoresCache = await _db.Colaboradores.ToDictionaryAsync(c => c.Cpf);

        // Percorrer linhas (pular cabeçalho na linha 1)
        var lastRow = ws.LastRowUsed()?.RowNumber() ?? 1;

        for (int row = 2; row <= lastRow; row++)
        {
            try
            {
                var idViagemParceiro = ws.Cell(row, 16).GetString().Trim();

                // Deduplicação: ignorar se já existe
                if (string.IsNullOrWhiteSpace(idViagemParceiro) || idsExistentes.Contains(idViagemParceiro))
                {
                    result.Ignoradas++;
                    continue;
                }

                // Parsear dados da linha
                var dataStr = ws.Cell(row, 1).GetString().Trim();
                var nomeColab = ws.Cell(row, 2).GetString().Trim();
                var cpf = ws.Cell(row, 3).GetString().Trim();
                var centroCusto = ws.Cell(row, 4).GetString().Trim();
                var origem = ws.Cell(row, 5).GetString().Trim();
                var destino = ws.Cell(row, 6).GetString().Trim();
                var parceiroNome = ws.Cell(row, 7).GetString().Trim();
                var valorCotadoStr = ws.Cell(row, 8).GetString().Trim();
                var valorFinalStr = ws.Cell(row, 9).GetString().Trim();
                var statusOrigem = ws.Cell(row, 10).GetString().Trim();
                var horaInicioStr = ws.Cell(row, 11).GetString().Trim();
                var horaFimStr = ws.Cell(row, 12).GetString().Trim();
                var distanciaStr = ws.Cell(row, 13).GetString().Trim();
                var avaliacaoStr = ws.Cell(row, 14).GetString().Trim();
                var motivo = ws.Cell(row, 15).GetString().Trim();

                // Resolver ou criar Setor
                if (!setoresCache.TryGetValue(centroCusto, out var setor))
                {
                    setor = new Setor { Nome = centroCusto };
                    _db.Setores.Add(setor);
                    await _db.SaveChangesAsync();
                    setoresCache[centroCusto] = setor;
                }

                // Resolver ou criar Parceiro
                if (!parceirosCache.TryGetValue(parceiroNome, out var parceiro))
                {
                    parceiro = new ParceiroViagem { Nome = parceiroNome };
                    _db.Parceiros.Add(parceiro);
                    await _db.SaveChangesAsync();
                    parceirosCache[parceiroNome] = parceiro;
                }

                // Resolver ou criar Colaborador
                if (!colaboradoresCache.TryGetValue(cpf, out var colaborador))
                {
                    colaborador = new Colaborador
                    {
                        Nome = nomeColab,
                        Cpf = cpf,
                        SetorId = setor.Id
                    };
                    _db.Colaboradores.Add(colaborador);
                    await _db.SaveChangesAsync();
                    colaboradoresCache[cpf] = colaborador;
                }

                // Parsear valores numéricos
                var cultura = new CultureInfo("pt-BR");

                var dataViagem = DateOnly.ParseExact(dataStr, ["dd/MM/yyyy", "d/M/yyyy"], cultura);
                var valorCotado = decimal.Parse(valorCotadoStr.Replace(".", ""), cultura);
                var valorFinal = decimal.Parse(valorFinalStr.Replace(".", ""), cultura);
                var horaInicio = ParseTimeOnly(horaInicioStr);
                var horaFim = ParseTimeOnly(horaFimStr);
                var distancia = double.Parse(distanciaStr.Replace(".", "").Replace(",", "."), CultureInfo.InvariantCulture);
                var avaliacao = int.TryParse(avaliacaoStr, out var av) ? av : 0;

                var viagem = new Viagem
                {
                    DataViagem = dataViagem,
                    Origem = origem,
                    Destino = destino,
                    ValorCotado = valorCotado,
                    ValorFinal = valorFinal,
                    StatusOrigem = statusOrigem,
                    HoraInicio = horaInicio,
                    HoraFim = horaFim,
                    DistanciaKm = distancia,
                    Avaliacao = avaliacao,
                    Motivo = motivo,
                    IdViagemParceiro = idViagemParceiro,
                    ColaboradorId = colaborador.Id,
                    SetorId = setor.Id,
                    ParceiroViagemId = parceiro.Id,
                    StatusConferenciaGestor = "Pendente"
                };

                _db.Viagens.Add(viagem);
                idsExistentes.Add(idViagemParceiro);
                result.Importadas++;
            }
            catch (Exception ex)
            {
                result.Erros.Add($"Linha {row}: {ex.Message}");
            }
        }

        if (result.Importadas > 0)
        {
            await _db.SaveChangesAsync();
        }

        return result;
    }

    private static TimeOnly ParseTimeOnly(string value)
    {
        // Aceita formatos como "4:39", "17:48", "18:0"
        var parts = value.Split(':');
        var hour = int.Parse(parts[0]);
        var minute = parts.Length > 1 ? int.Parse(parts[1]) : 0;
        return new TimeOnly(hour, minute);
    }
}
