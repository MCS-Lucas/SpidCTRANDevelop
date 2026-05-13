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

    public async Task<ImportacaoResult> ImportarExcelAsync(Stream stream, int usuarioId)
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

        // Desabilitar detecção automática de mudanças para performance em bulk
        _db.ChangeTracker.AutoDetectChangesEnabled = false;

        try
        {
            // Primeira passada: resolver/criar entidades auxiliares
            var novosSetores = new List<Setor>();
            var novosParceiros = new List<ParceiroViagem>();
            var novosColaboradores = new List<(int row, string cpf, string nome, Setor setor)>();

            for (int row = 2; row <= lastRow; row++)
            {
                try
                {
                    var idViagemParceiro = ws.Cell(row, 16).GetString().Trim();
                    if (string.IsNullOrWhiteSpace(idViagemParceiro) || idsExistentes.Contains(idViagemParceiro))
                        continue;

                    var centroCusto = ws.Cell(row, 4).GetString().Trim();
                    var parceiroNome = ws.Cell(row, 7).GetString().Trim();
                    var cpf = ws.Cell(row, 3).GetString().Trim();
                    var nomeColab = ws.Cell(row, 2).GetString().Trim();

                    if (!setoresCache.ContainsKey(centroCusto))
                    {
                        var setor = new Setor { Nome = centroCusto };
                        novosSetores.Add(setor);
                        setoresCache[centroCusto] = setor;
                    }

                    if (!parceirosCache.ContainsKey(parceiroNome))
                    {
                        var parceiro = new ParceiroViagem { Nome = parceiroNome };
                        novosParceiros.Add(parceiro);
                        parceirosCache[parceiroNome] = parceiro;
                    }

                    if (!colaboradoresCache.ContainsKey(cpf))
                    {
                        novosColaboradores.Add((row, cpf, nomeColab, setoresCache[centroCusto]));
                        colaboradoresCache[cpf] = null!; // placeholder
                    }
                }
                catch (Exception ex)
                {
                    result.Erros.Add($"Linha {row} (pré-processamento): {ex.Message}");
                }
            }

            // Salvar entidades auxiliares em batch
            if (novosSetores.Count > 0)
            {
                _db.Setores.AddRange(novosSetores);
                await _db.SaveChangesAsync();
            }

            if (novosParceiros.Count > 0)
            {
                _db.Parceiros.AddRange(novosParceiros);
                await _db.SaveChangesAsync();
            }

            // Criar colaboradores (precisam do SetorId já persistido)
            if (novosColaboradores.Count > 0)
            {
                var colabEntities = novosColaboradores.Select(c => new Colaborador
                {
                    Nome = c.nome,
                    Cpf = c.cpf,
                    SetorId = c.setor.Id
                }).ToList();

                _db.Colaboradores.AddRange(colabEntities);
                await _db.SaveChangesAsync();

                foreach (var colab in colabEntities)
                    colaboradoresCache[colab.Cpf] = colab;
            }

            // Segunda passada: criar viagens em lotes
            const int batchSize = 500;
            int pendingCount = 0;

            for (int row = 2; row <= lastRow; row++)
            {
                try
                {
                    var idViagemParceiro = ws.Cell(row, 16).GetString().Trim();

                    if (string.IsNullOrWhiteSpace(idViagemParceiro) || idsExistentes.Contains(idViagemParceiro))
                    {
                        result.Ignoradas++;
                        continue;
                    }

                    var dataCell = ws.Cell(row, 1);
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

                    var setor = setoresCache[centroCusto];
                    var parceiro = parceirosCache[parceiroNome];
                    var colaborador = colaboradoresCache[cpf];

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
                    pendingCount++;

                    if (pendingCount >= batchSize)
                    {
                        await _db.SaveChangesAsync();
                        pendingCount = 0;
                    }
                }
                catch (Exception ex)
                {
                    result.Erros.Add($"Linha {row}: {ex.Message}");
                }
            }

            // Salvar viagens restantes
            if (pendingCount > 0)
                await _db.SaveChangesAsync();

            if (result.Importadas > 0)
            {
                var log = new ImportacaoLog
                {
                    DataImportacao = DateTime.Now,
                    UsuarioId = usuarioId,
                    QuantidadeImportada = result.Importadas
                };
                _db.ImportacoesLog.Add(log);
                await _db.SaveChangesAsync();
            }
        }
        finally
        {
            _db.ChangeTracker.AutoDetectChangesEnabled = true;
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
