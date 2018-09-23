namespace Game.Engine.Controllers
{
    using Game.API.Authentication;
    using Game.API.Common.Models;
    using Game.API.Common.Security;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    public class UserController : APIControllerBase
    {
        private readonly JWT JWT;

        public UserController(
            ISecurityContext securityContext,
            JWT jwt
        ) : base(securityContext)
        {
            this.JWT = jwt;
        }

        [
            AllowAnonymous,
            HttpPost,
            Route("authenticate")
        ]
        public TokenResponseModel Authenticate([FromBody] TokenRequestModel request)
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
    }
}