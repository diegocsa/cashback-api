using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace Cashback.Infra.CrossCutting.Auth
{
    public class Jwt
    {
        public static string GenerateToken(string issuer, string audience, DateTime expAt, string chave)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(chave));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(issuer: issuer,
                                             audience: audience,
                                             expires: expAt,
                                             signingCredentials: credentials);
            var tokenHandler = new JwtSecurityTokenHandler();
            return tokenHandler.WriteToken(token);
        }
    }
}
