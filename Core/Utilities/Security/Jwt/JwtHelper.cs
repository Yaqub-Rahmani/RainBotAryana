using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Entities.Concrete;
using Core.Utilities.Security.Jwt; // TokenOptions, AccessToken, SecurityKeyHelper
using Core.Extensions;            // ClaimsExtensions (AddEmail, AddName, AddRoles)

namespace Business.Security.Jwt
{
    public class JwtHelper
    {
        private readonly TokenOptions _options;

        public JwtHelper(IConfiguration configuration)
        {
            // Manuel okuma: Binder paketine gerek yok
            var s = configuration.GetSection("TokenOptions");
            _options = new TokenOptions
            {
                Audience = s["Audience"]!,
                Issuer = s["Issuer"]!,
                SecurityKey = s["SecurityKey"]!,
                AccessTokenExpiration = int.TryParse(s["AccessTokenExpiration"], out var m) ? m : 60
            };
        }

        public AccessToken CreateToken(User user, List<OperationClaim> claims)
        {
            var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(_options.SecurityKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);
            var expires = DateTime.Now.AddMinutes(_options.AccessTokenExpiration);

            var jwt = new JwtSecurityToken(
                issuer: _options.Issuer,
                audience: _options.Audience,
                claims: BuildClaims(user, claims),
                notBefore: DateTime.Now,
                expires: expires,
                signingCredentials: creds
            );

            var handler = new JwtSecurityTokenHandler();
            return new AccessToken { Token = handler.WriteToken(jwt), Expiration = expires };
        }

        private static IEnumerable<Claim> BuildClaims(User user, List<OperationClaim> roles)
        {
            var list = new List<Claim>
    {
        // Kullanıcı kimliği (id)
       // new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()), // JWT standard claim
        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),   // .NET standard claim

        // Kullanıcı bilgileri
        new Claim(ClaimTypes.Email, user.Email),
        new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}")
    };

            if (roles != null)
            {
                foreach (var r in roles)
                    list.Add(new Claim(ClaimTypes.Role, r.Name));
            }

            return list;
        }
    }
}
