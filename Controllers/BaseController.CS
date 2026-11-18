using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

public abstract class BaseController : Controller
{
    /// <summary>Id do usuário logado (claim NameIdentifier). 0 se não houver login.</summary>
    protected int UserId
    {
        get
        {
            var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(claim, out var id) ? id : 0;
        }
    }
}
