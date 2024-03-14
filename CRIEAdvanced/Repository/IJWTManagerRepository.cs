using CRIEAdvanced.ViewModels;
using System.Security.Claims;

namespace CRIEAdvanced.Repository
{
    //https://medium.com/@chauhanshubham19765/jwt-refresh-token-61823e888bc7
    public interface IJWTManagerRepository
    {
        Tokens? GenerateToken(String userIP, String userID);
        Tokens? GenerateRefreshToken(String userIP, String userID);
        ClaimsPrincipal GetPrincipalFromExpiredToken(String token);
    }
}
