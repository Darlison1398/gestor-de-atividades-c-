using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

public abstract class BaseController : Controller
{
    protected int UserId
    {
        get
        {
            var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(claim, out var id) ? id : 0;
        }
    }
}
