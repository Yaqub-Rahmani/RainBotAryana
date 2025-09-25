using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Utilities.Security.Jwt
{
    public static class SigningCredentialsHelper
    {
        public static SigningCredentials CreateSigningCredentials(string securityKey)
            => new SigningCredentials(SecurityKeyHelper.CreateSecurityKey(securityKey), SecurityAlgorithms.HmacSha512Signature);
    }
}
