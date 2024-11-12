using SeguroVeiculo.Infraestrutura.Responses;

namespace SeguroVeiculo.Infraestrutura;

public class HistoricoAcidentesHttpApi
{
    public async Task<IEnumerable<AcidenteResponse>> BuscarAcidentesPorCpf(string cpf, int anoInicial, CancellationToken cancellationToken)
    {
        return [new AcidenteResponse("2021", "G"), new AcidenteResponse("2022", "L")];
    }
}