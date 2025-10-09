using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Calcpad.WebApi.Controllers.Base
{
    [Authorize]
    [Route("api/v1/[controller]")]
    [ApiController]
    public abstract class ControllerBaseV1 : ControllerBase
    {
    }
}
