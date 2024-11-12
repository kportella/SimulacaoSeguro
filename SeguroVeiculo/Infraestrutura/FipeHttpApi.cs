namespace SeguroVeiculo.Infraestrutura;

public class FipeHttpApi(IHttpContextAccessor httpContextAccessor)
{
    
    private readonly string urlFipe = "api/Fipe";
    
    public async Task<decimal> BuscarValorVeiculo(string marca, string modelo, string ano, 
        CancellationToken cancellationToken)
    {
        return 1000;
    }
}