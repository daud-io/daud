namespace Game.Registry.API.Authentication
{
    using Microsoft.AspNetCore.Authentication.JwtBearer;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.IdentityModel.Tokens;
    using System;

    public static class JWTServicesExtensions
    {
        public static IServiceCollection UseJWTAuthentication(this IServiceCollection services)
        {
            var sp = services.BuildServiceProvider();
            var config = sp.GetService<GameConfiguration>();

            // API JWT Validation options
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(o =>
            {
                JWT.InitializeSecret(config);

                o.TokenValidationParameters = new TokenValidationParameters
                {
                    // The signing key must match!
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(JWT.SecretBytes),

                    // Validate the JWT Issuer (iss) claim 
                    ValidateIssuer = true,
                    ValidIssuer = config.TokenIssuer,

                    // Validate the JWT Audience (aud) claim
                    ValidateAudience = true,
                    ValidAudience = config.TokenAudience,

                    // Validate the token expiry
                    ValidateLifetime = true,

                    // If you want to allow a certain amount of clock drift, set that here:
                    ClockSkew = TimeSpan.FromMinutes(5)
                };
            });

            return services;
        }
    }
}
