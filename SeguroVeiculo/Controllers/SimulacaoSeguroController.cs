using Microsoft.AspNetCore.Mvc;
using SeguroVeiculo.Infraestrutura;

namespace SeguroVeiculo.Controllers;

[ApiController]
[Route("[controller]")]
public class SimulacaoSeguroController : ControllerBase
{
    public record struct SimulacaoSeguroRequest(Veiculo Veiculo, Proprietario Proprietario, Condutor Condutor,
        Coberturas Coberturas);

    public record struct Veiculo(string Marca, string Modelo, string Ano);
    public record struct Proprietario(string Cpf, string Nome, DateTime DataNascimento, string Residencia);
    public record struct Condutor(string Cpf, DateTime DataNascimento, string Residencia);
    public record struct Coberturas(IEnumerable<string> Cobertura);
    
    public async Task<IActionResult> SimulacaoSeguro([FromBody] SimulacaoSeguroRequest request, 
        [FromServices] FipeHttpApi fipeHttpApi,
        [FromServices] HistoricoAcidentesHttpApi historicoAcidentesHttpApi,
        CancellationToken cancellationToken)
    {
        // Verificar se dados estão Ok
        
        // Verificar tabela FIPE
        // Api Externa
        var valorVeiculo = await fipeHttpApi.BuscarValorVeiculo(request.Veiculo.Marca, request.Veiculo.Modelo, 
            request.Veiculo.Ano, cancellationToken);
        
        // Verificar Historico de Acidentes
        // Api Externa
        var historicoAcidentes =
            await historicoAcidentesHttpApi.BuscarAcidentesPorCpf(request.Condutor.Cpf, DateTime.Now.AddYears(-3).Year, 
                cancellationToken);
        
        // Calcular Nível de Risco a partir do Histórico de Acidentes, Idade e Residencia
        
        // Variáveis e Pontuações
        
        var pontosRisco = 0;
        
        var idadeCondutor = (DateTime.Now.Year - request.Condutor.DataNascimento.Year);

        if (request.Condutor.DataNascimento > DateTime.Now.AddYears(-idadeCondutor))
            idadeCondutor--;

        // Idade do Condutor:
        // - 18-25 anos: 15 pontos
        // - 26-40 anos: 5 pontos
        // - 41-60 anos: 3 pontos
        // - Acima de 60 anos: 10 pontos
        // Critério: Condutores mais jovens ou idosos têm maior risco.

        pontosRisco += idadeCondutor switch
        {
            >= 18 and <= 25 => 15,
            >= 26 and <= 40 => 5,
            >= 41 and <= 60 => 3,
            _ => 10
        };
        
        // Histórico de Acidentes:
        // - Nenhum acidente: 0 pontos
        // - 1 acidente: 10 pontos
        // - 2 acidentes: 20 pontos
        // - 3 ou mais acidentes: 30 pontos
        // Critério: Quantidade de acidentes nos últimos 3 anos.

        pontosRisco += historicoAcidentes.Count() switch
        {
            1 => pontosRisco += 10,
            2 => pontosRisco += 20,
            >= 3 => pontosRisco += 30,
            _ => 0
        };

        // Localidade de Residência:
        // - Baixo risco: 5 pontos
        // - Médio risco: 10 pontos
        // - Alto risco: 20 pontos
        // Critério: Risco associado à região.

        pontosRisco += request.Condutor.Residencia switch
        {
            "B" => pontosRisco += 5,
            "M" => pontosRisco += 10,
            "A" => pontosRisco += 20,
        };
        
        // Classificação do Nível de Risco

        // Pontuação Total e Nível de Risco:
        // - 0 - 10 pontos: Nível de Risco 1 (Baixo)
        // - 11 - 25 pontos: Nível de Risco 2
        // - 26 - 40 pontos: Nível de Risco 3
        // - 41 - 55 pontos: Nível de Risco 4
        // - 56 pontos ou mais: Nível de Risco 5 (Alto)

        var nivelRisco = pontosRisco switch
        {
            >= 0 and <= 10 => 1,
            >= 11 and <= 25 => 2,
            >= 26 and <= 40 => 3,
            >= 41 and <= 55 => 4,
            >= 56 => 5,
        };
        
        // Calculo do Valor do Seguro
        // Coberturas Básicas e Custo Base

        // Cada cobertura tem um custo base que é aplicado sobre o valor de mercado do veículo:

        // Cobertura e Custo Base:
        // - Roubo/Furto: 3% sobre o valor de mercado
        // - Colisão: 4% sobre o valor de mercado
        // - Terceiros: 1.5% sobre o valor de mercado
        // - Proteção Residencial: Taxa fixa de R$ 100
        
        // Ajuste pelo Nível de Risco

        // O custo das coberturas é ajustado com base no nível de risco do condutor:

        // Nível de Risco e Ajuste:
        // - Nível de Risco 1: sem ajuste (100% do custo base)
        // - Nível de Risco 2: +5% sobre o custo base
        // - Nível de Risco 3: +10% sobre o custo base
        // - Nível de Risco 4: +20% sobre o custo base
        // - Nível de Risco 5: +30% sobre o custo base

        // Calcular Seguro

        var valorSeguro = CalcularValorSeguro(valorVeiculo, nivelRisco, request.Coberturas.Cobertura);
        
        // Gravar Intenção de Seguro no Banco (Criar campo para Situação {Pendente, Aprovado, Reprovado})
        
        return Ok(valorSeguro);
    }
    
    decimal CalcularValorSeguro(decimal valorMercado, int nivelRisco, IEnumerable<string> coberturasSelecionadas)
    {
        var valorSeguro = 0m;

        // Definir o percentual de ajuste com base no nível de risco
        var percentualAjuste = nivelRisco switch
        {
            1 => 0m,
            2 => 0.05m,
            3 => 0.10m,
            4 => 0.20m,
            5 => 0.30m,
            _ => throw new ArgumentException("Nível de risco inválido")
        };

        foreach (var cobertura in coberturasSelecionadas)
        {
            var custoBase = 0m;
            var custoAjustado = 0m;

            switch (cobertura)
            {
                case "RouboFurto":
                    custoBase = valorMercado * 0.03m;
                    custoAjustado = custoBase * (1 + percentualAjuste);
                    break;
                case "Colisao":
                    custoBase = valorMercado * 0.04m;
                    custoAjustado = custoBase * (1 + percentualAjuste);
                    break;
                case "Terceiros":
                    custoBase = valorMercado * 0.015m;
                    custoAjustado = custoBase * (1 + percentualAjuste);
                    break;
                case "ProtecaoResidencial":
                    custoAjustado = 100m; // Taxa fixa, sem ajuste
                    break;
                default:
                    throw new ArgumentException("Cobertura inválida");
            }

            valorSeguro += custoAjustado;
        }

        // Arredondar para cima com duas casas decimais
        valorSeguro = Math.Ceiling(valorSeguro * 100) / 100m;

        return valorSeguro;
    }
}