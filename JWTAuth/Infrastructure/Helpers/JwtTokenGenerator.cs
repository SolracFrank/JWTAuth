using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ToklenAPI.Models.Dtos.JWTToken;

namespace Infrastructure.Helpers
{
    public static class JwtTokenGenerator
    {
        public static JWTResult GenerateJWTToken(string email, JWTSettings _jwtSettings)
        {
            //Claims
            var claims = new List<Claim>
            {
                new ("email", email),
                new ("active","true")
            };
            var claimsIdentity = new ClaimsIdentity(claims, "defaultLogin");

            //JWT Configuration
            var expirationDate = DateTime.UtcNow.AddMinutes(_jwtSettings.Duration);
            var symetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));
            var signInCredentials = new SigningCredentials(symetricSecurityKey, SecurityAlgorithms.HmacSha256);

            var jwtSecurityToken = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claimsIdentity.Claims,
                expires: expirationDate,
                signingCredentials: signInCredentials
            );

            //JWT Result
            var tokenResult = new JWTResult
            {
                Email = email,
                JWToken = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken),
                JWTExpires = expirationDate,
            };


            return tokenResult;
        }
    }
}
