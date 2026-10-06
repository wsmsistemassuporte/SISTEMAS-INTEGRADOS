using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace WSM.SISTEMAS.Core.Usuarios
{
    public interface IAspNetUser
    {
        string Name { get; }
        Guid ObterUserId();      
        string ObterUserEmail(); 
        string ObterUserToken();
        string ObterUserRefreshToken();
        bool EstaAutenticado();
        bool PossuiRole(string role);
        IEnumerable<Claim> ObterClaims();
        HttpContext ObterHttpContext();
    }
}