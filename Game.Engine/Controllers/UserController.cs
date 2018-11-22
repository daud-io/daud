namespace Game.Engine.Controllers
{
    using Game.API.Authentication;
    using Game.API.Common.Models;
    using Game.API.Common.Security;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using System;

    public class UserController : APIControllerBase
    {
        private readonly JWT JWT;
        private readonly GameConfiguration Config;

        public UserController(
            ISecurityContext securityContext,
            JWT jwt,
            GameConfiguration config
        ) : base(securityContext)
        {
            this.JWT = jwt;
            this.Config = config;
        }

        [
            AllowAnonymous,
            HttpPost,
            Route("authenticate")
        ]
        public TokenResponseModel Authenticate([FromBody] TokenRequestModel request)
        {
            if (request.Identifier.UserKey == "Administrator" &&
                (request.Password == Config.AdministratorPassword 
                || Config.AdministratorPassword == null)
            )
            {
                var user = new UserModel
                {
                    Identifier = request.Identifier,
                    NickName = request.Identifier.UserKey,
                    UserAccessIdentifiers = new string[0]
                };

                SecurityContext.AssumeUser(user);

                return new TokenResponseModel
                {
                    Token = JWT.CreateUserToken(user, request.ClientClaims),
                    User = user
                };
            }
            else
                throw new Exception("Invalid Auth");
        }
    }
}