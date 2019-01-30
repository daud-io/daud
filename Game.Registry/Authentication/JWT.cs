namespace Game.Registry.API.Authentication
{
    using Game.API.Common.Models;
    using Game.API.Common.Security;
    using Game.Engine;
    using Microsoft.IdentityModel.Tokens;
    using System;
    using System.Collections.Generic;
    using System.IdentityModel.Tokens.Jwt;
    using System.Security.Claims;
    using System.Security.Cryptography;
    using System.Text;

    public class JWT
    {
        private readonly GameConfiguration Configuration;

        public static byte[] SecretBytes = null;

        public static void InitializeSecret(GameConfiguration config)
        {
            if (SecretBytes == null)
            {
                if (config.TokenValidationSecret != null)
                    SecretBytes = Encoding.ASCII.GetBytes(config.TokenValidationSecret);
                else
                {
                    SecretBytes = new byte[20];
                    RandomNumberGenerator.Fill(SecretBytes);
                }
            }
        }

        public JWT(GameConfiguration configuration)
        {
            this.Configuration = configuration;
        }

        public string CreateUserToken(UserModel user, string clientClaims = null)
        {
            var now = DateTime.UtcNow;

            var claims = new List<Claim>
            {
                new Claim(SecurityTokenClaimKeys.SECURITY_IDENTIFIERS, string.Join(" ", user.UserAccessIdentifiers)),
                new Claim(SecurityTokenClaimKeys.USER_KEY, user.Identifier.UserKey),
                new Claim(SecurityTokenClaimKeys.CLIENT_CLAIMS, clientClaims ?? string.Empty),
                new Claim(JwtRegisteredClaimNames.Sub, user.Identifier.UserKey),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, ToUnixEpochDate(now).ToString(), ClaimValueTypes.Integer64)
            };

            InitializeSecret(this.Configuration);

            var signingKey = new SymmetricSecurityKey(SecretBytes);

            var expires = TimeSpan.FromSeconds(Configuration.TokenExpirationSeconds);

            var jwt = new JwtSecurityToken(
                issuer: Configuration.TokenIssuer,
                audience: Configuration.TokenAudience,
                claims: claims,
                notBefore: now,
                expires: now.Add(expires),
                signingCredentials: new SigningCredentials(
                    signingKey, SecurityAlgorithms.HmacSha256
                ));


            return new JwtSecurityTokenHandler().WriteToken(jwt);
        }

        public JwtSecurityToken Read(string token)
        {
            return new JwtSecurityTokenHandler().ReadJwtToken(token);
        }

        /// <summary>
        /// Get this datetime as a Unix epoch timestamp (seconds since Jan 1, 1970, midnight UTC).
        /// </summary>
        /// <param name="date">The date to convert.</param>
        /// <returns>Seconds since Unix epoch.</returns>
        private static long ToUnixEpochDate(DateTime date)
            => (long)Math.Round((date.ToUniversalTime() - new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero)).TotalSeconds);
    }
}
