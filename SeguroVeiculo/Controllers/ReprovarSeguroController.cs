using Microsoft.AspNetCore.Mvc;

namespace SeguroVeiculo.Controllers;

[ApiController]
[Route("[controller]")]
public class ReprovarSeguroController : ControllerBase
{
    public IActionResult ReprovarSeguro(CancellationToken cancellationToken)
    {
        return Ok();
    }
}