using Microsoft.AspNetCore.Mvc;

namespace SeguroVeiculo.Controllers;

[ApiController]
[Route("[controller]")]
public class AprovarSeguroController : ControllerBase
{
    public IActionResult AprovarSeguro(CancellationToken cancellationToken)
    {
        return Ok();
    }
}