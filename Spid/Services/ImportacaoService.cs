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
                var dataCell = ws.Cell(row, 1);
                var nomeColab = ws.Cell(row, 2).GetString().Trim();
                var cpf = ws.Cell(row, 3).GetString().Trim();
                var centroCusto = ws.Cell(row, 4).GetString().Trim();
                var origem = ws.Cell(row, 5).GetString().Trim();
                var destino = ws.Cell(row, 6).GetString().Trim();
                var parceiroNome = ws.Cell(row, 7).GetString().Trim();
                var valorCotadoCell = ws.Cell(row, 8);
                var valorFinalCell = ws.Cell(row, 9);
                var statusOrigem = ws.Cell(row, 10).GetString().Trim();
                var horaInicioCell = ws.Cell(row, 11);
                var horaFimCell = ws.Cell(row, 12);
                var distanciaCell = ws.Cell(row, 13);
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

                // Parsear valores numéricos e datas de forma segura
                var dataViagem = ParseDateSafe(dataCell);
                var valorCotado = ParseDecimalSafe(valorCotadoCell);
                var valorFinal = ParseDecimalSafe(valorFinalCell);
                var horaInicio = ParseTimeOnlySafe(horaInicioCell);
                var horaFim = ParseTimeOnlySafe(horaFimCell);
                var distancia = (double)ParseDecimalSafe(distanciaCell);
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

    private static DateOnly ParseDateSafe(IXLCell cell)
    {
        if (cell.DataType == XLDataType.DateTime)
        {
            return DateOnly.FromDateTime(cell.GetDateTime());
        }

        var str = cell.GetString()?.Trim();
        if (string.IsNullOrWhiteSpace(str)) return default;

        if (DateTime.TryParseExact(str, new[] { "dd/MM/yyyy", "d/M/yyyy", "dd/MM/yy", "yyyy-MM-dd" }, new CultureInfo("pt-BR"), DateTimeStyles.None, out var dt))
        {
            return DateOnly.FromDateTime(dt);
        }
        
        if (DateTime.TryParse(str, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dtFallback))
        {
            return DateOnly.FromDateTime(dtFallback);
        }

        return default;
    }

    private static decimal ParseDecimalSafe(IXLCell cell)
    {
        if (cell.DataType == XLDataType.Number)
        {
            return cell.GetValue<decimal>();
        }

        var str = cell.GetString()?.Trim();
        if (string.IsNullOrWhiteSpace(str)) return 0m;

        // Limpar possíveis símbolos de moeda e espaços
        str = str.Replace("R$", "").Replace("$", "").Trim();

        // Se usar vírgula, assumimos que é formato pt-BR (ex: "1.500,50" ou "12,95")
        if (str.Contains(','))
        {
            str = str.Replace(".", ""); // Remove separadores de milhar
            if (decimal.TryParse(str, NumberStyles.Any, new CultureInfo("pt-BR"), out decimal valBr))
            {
                return valBr;
            }
        }
        else
        {
            // Se não usar vírgula, pode ser Invariant "1500.50" ou apenas inteiro "1500"
            if (decimal.TryParse(str, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal valInv))
            {
                return valInv;
            }
            
            // Fallback para pt-BR
            if (decimal.TryParse(str, NumberStyles.Any, new CultureInfo("pt-BR"), out decimal valPt))
            {
                return valPt;
            }
        }

        return 0m;
    }

    private static TimeOnly ParseTimeOnlySafe(IXLCell cell)
    {
        if (cell.DataType == XLDataType.TimeSpan)
        {
            var ts = cell.GetTimeSpan();
            // Evitar erro se passar de 24 horas
            return new TimeOnly(Math.Max(0, ts.Hours % 24), Math.Max(0, ts.Minutes), Math.Max(0, ts.Seconds));
        }
        else if (cell.DataType == XLDataType.DateTime)
        {
            return TimeOnly.FromDateTime(cell.GetDateTime());
        }

        var value = cell.GetString()?.Trim();
        if (string.IsNullOrWhiteSpace(value)) return new TimeOnly(0, 0);

        var parts = value.Split(':');
        if (parts.Length > 0 && int.TryParse(parts[0], out var hour))
        {
            var minute = parts.Length > 1 && int.TryParse(parts[1], out var m) ? m : 0;
            return new TimeOnly(Math.Clamp(hour % 24, 0, 23), Math.Clamp(minute % 60, 0, 59));
        }

        return new TimeOnly(0, 0);
    }
}
