using CRIEAdvanced.ViewModels;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace CRIEAdvanced.Repository
{
    public class JWTManagerRepository : IJWTManagerRepository
    {
        private readonly IConfiguration _iconfiguration;
        private readonly IServiceProvider _serviceProvider;

        public JWTManagerRepository(IConfiguration iconfiguration, IServiceProvider serviceProvider)
        {
            _iconfiguration = iconfiguration;
            _serviceProvider = serviceProvider; //Error while validating the service descriptor 'ServiceType: CRIE.Repository.IJWTManagerRepository Lifetime: Singleton ImplementationType: CRIE.Repository.JWTManagerRepository': Cannot consume scoped service 'CRIE.Helpers.RsaHelper' from singleton 'CRIE.Repository.IJWTManagerRepository'.
        }

        public Tokens? GenerateToken(String userIP, String userID)
        {
            return GenerateJWTTokens(userIP, userID);
        }

        public Tokens? GenerateRefreshToken(String userIP, String userID)
        {
            return GenerateJWTTokens(userIP, userID);
        }

        public Tokens? GenerateJWTTokens(String userIP, String userID)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var tokenKey = Encoding.UTF8.GetBytes(_iconfiguration["JwtSettings:SignKey"]);
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Issuer = _iconfiguration["JwtSettings:Issuer"],
                    Subject = new ClaimsIdentity(new Claim[]
                    {
                        new Claim(JwtRegisteredClaimNames.Sub, userIP),
                        new Claim(JwtRegisteredClaimNames.Name, userID)
                        //new Claim(ClaimTypes.Email, userName)
                    }),
                    Expires = DateTime.Now.AddMinutes(60),
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(tokenKey), SecurityAlgorithms.HmacSha256Signature)
                };

                var token = tokenHandler.CreateToken(tokenDescriptor);
                var refreshToken = GenerateRefreshToken();

                return new Tokens { AccessToken = tokenHandler.WriteToken(token), RefreshToken = refreshToken };

            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber);
            }
        }

        public ClaimsPrincipal GetPrincipalFromExpiredToken(String token)
        {
            var Key = Encoding.UTF8.GetBytes(_iconfiguration["JwtSettings:SignKey"]);

            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Key),
                ClockSkew = TimeSpan.Zero
            };

            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);
                JwtSecurityToken? jwtSecurityToken = securityToken as JwtSecurityToken;
                if (jwtSecurityToken == null || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                {
                    throw new SecurityTokenException("Invalid token");
                }

                return principal;
            }
            catch
            {
                throw new SecurityTokenException("Invalid token");
            }
        }
    }
}
